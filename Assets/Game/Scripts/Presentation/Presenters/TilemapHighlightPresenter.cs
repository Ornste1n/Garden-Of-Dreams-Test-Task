using System;
using Zenject;
using MessagePipe;
using UnityEngine;
using UnityEngine.Tilemaps;
using Game.Scripts.Presentation.View.Interfaces;
using Game.Scripts.Usecases.Application.Messages;

namespace Game.Scripts.Presentation.Presenters
{
    // Презентер, отвечающий за подсветку сетки
    public class TilemapHighlightPresenter : IDisposable
    {
        private IDisposable _choiceSub;
        private IDisposable _canceledSub;
        private IGridHighlightView _gridView;
        
        private Tilemap _tilemap;
        private Camera _camera;

        private Texture2D _maskTexture;
        private Vector3 _bottomLeft;
        private Vector2 _cellSize;
        private Vector2Int _maskSize;
        
        [Inject]
        private void Constructor
        (
            ISubscriber<ChoicePlacementEvent> choiceSubscriber,
            ISubscriber<CanceledPlacementEvent> canceledSubscriber,
            IGridHighlightView gridView,
            Tilemap tilemap
        )
        {
            _tilemap = tilemap;
            _gridView = gridView;
            _camera = Camera.main;
            
            _choiceSub = choiceSubscriber.Subscribe(HandleChoicePlaceEvent);
            _canceledSub = canceledSubscriber.Subscribe(HandleCanceledPlaceEvent);
        }
        
        private void HandleChoicePlaceEvent(ChoicePlacementEvent _) => StartHighlight();
        private void HandleCanceledPlaceEvent(CanceledPlacementEvent _) => ClearMaskPixels();

        private void StartHighlight()
        {
            CalculateVisibleCells();
    
            _gridView.HighlightVisible(_bottomLeft, _cellSize, _maskSize, _maskTexture);
            _gridView.SetValidity(true);
        }
        
        // Рассчитываю границы видимой зоны (что улавливает камеры)
        private void CalculateVisibleCells()
        {
            Tilemap tilemap = _tilemap;
            float planeZ = tilemap.transform.position.z;
            Vector3[] worldCorners = CameraWorldRectOnZ(_camera, planeZ);
            
            Vector3 worldMin = worldCorners[0];
            Vector3 worldMax = worldCorners[2];

            Vector3Int cellMin = tilemap.WorldToCell(worldMin);
            Vector3Int cellMax = tilemap.WorldToCell(worldMax);

            int xMin = Math.Min(cellMin.x, cellMax.x);
            int xMax = Math.Max(cellMin.x, cellMax.x);
            int yMin = Math.Min(cellMin.y, cellMax.y);
            int yMax = Math.Max(cellMin.y, cellMax.y);

            BoundsInt mapBounds = tilemap.cellBounds;
            xMin = Mathf.Max(mapBounds.xMin, xMin);
            yMin = Mathf.Max(mapBounds.yMin, yMin);
            xMax = Mathf.Min(mapBounds.xMax - 1, xMax);
            yMax = Mathf.Min(mapBounds.yMax - 1, yMax);

            int width = xMax - xMin + 1;
            int height = yMax - yMin + 1;

            CreateOrResizeMask(width, height);
            ClearMaskPixels();
            
            _maskSize = new Vector2Int(width, height);

            FillMask(Color.white);

            Vector3 cellWorld = tilemap.CellToWorld(new Vector3Int(xMin, yMin, 0));
            Vector3 cellSizeWorld =
                tilemap.CellToWorld(new Vector3Int(xMin + 1, yMin + 1, 0)) - cellWorld;

            _bottomLeft = cellWorld;
            _cellSize = new Vector2(Mathf.Abs(cellSizeWorld.x), Mathf.Abs(cellSizeWorld.y));
        }
        
        private void FillMask(Color color)
        {
            Color[] arr = _maskTexture.GetPixels();
            for (int i = 0; i < arr.Length; i++) arr[i] = color;
            _maskTexture.SetPixels(arr);
        }

        private void CreateOrResizeMask(int width, int height)
        {
            const int maxSize = 4096; // по-хорошему вынести в конфиг
            int w = Mathf.Clamp(width, 1, maxSize);
            int h = Mathf.Clamp(height, 1, maxSize);

            if (_maskTexture != null && _maskSize.x == w && _maskSize.y == h) return;

            _maskTexture = new Texture2D(w, h, TextureFormat.R8, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            
            _maskSize.x = w;
            _maskSize.y = h;

            ClearMaskPixels();
        }
        
        // Очищаем текстуру
        private void ClearMaskPixels()
        {
            if(_maskTexture == null) return;
            
            Color[] arr = _maskTexture.GetPixels();
            for (int i = 0; i < arr.Length; i++) arr[i] = Color.black;
            _maskTexture.SetPixels(arr);
            
            _gridView.ApplyMask();
        }
        
        // Границы зоны камеры
        private Vector3[] CameraWorldRectOnZ(Camera cam, float z)
        {
            Vector3[] corners = new Vector3[4];
            if (cam.orthographic)
            {
                float height = cam.orthographicSize * 2f;
                float width = height * cam.aspect;
                Vector3 center = cam.transform.position;
                center.z = z;

                corners[0] = new Vector3(center.x - width * 0.5f, center.y - height * 0.5f, z);
                corners[1] = new Vector3(center.x + width * 0.5f, center.y - height * 0.5f, z);
                corners[2] = new Vector3(center.x + width * 0.5f, center.y + height * 0.5f, z);
                corners[3] = new Vector3(center.x - width * 0.5f, center.y + height * 0.5f, z);
            }
            else
            {
                corners[0] = cam.ScreenToWorldPoint(new Vector3(0f, 0f, cam.transform.position.z - z));
                corners[1] = cam.ScreenToWorldPoint(new Vector3(Screen.width, 0f, cam.transform.position.z - z));
                corners[2] = cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, cam.transform.position.z - z));
                corners[3] = cam.ScreenToWorldPoint(new Vector3(0f, Screen.height, cam.transform.position.z - z));
            }
            return corners;
        }

        public void Dispose()
        {
            _choiceSub?.Dispose();
            _canceledSub?.Dispose();
        }
    }
}
