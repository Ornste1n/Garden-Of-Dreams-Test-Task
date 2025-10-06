using UnityEngine;
using UnityEngine.Tilemaps;
using Game.Scripts.Domain.Game;
using System.Collections.Generic;

namespace Game.Scripts.Infrastructure.Utilities
{
    // Утилита для вычислений сетки
    public static class TilemapExtensions
    {
        // Метод расчитывает размер тайла, находит ячейки на которые спрайт распространяет
        public static Occupancy GetOccupiedCellsRelativeToPivotCell
        (
            Sprite sprite,
            Tilemap tilemap,
            Vector3Int cellPivotIndex,
            float minOverlapFraction = 0.02f
        )
        {
            Occupancy result = new()
            {
                Size = Vector2.zero.ToSystem(),
                OccupiedCells = new List<System.Numerics.Vector3>()
            };

            if (sprite == null || tilemap == null) return result;

            Quaternion rotation = Quaternion.identity;
            minOverlapFraction = Mathf.Clamp01(minOverlapFraction);
            
            // 1) Получаем локальные границы спрайта относительно pivot
            Rect rect = sprite.rect;
            Vector2 pivotPx = sprite.pivot;
            float ppu = sprite.pixelsPerUnit;

            float left = -(pivotPx.x) / ppu;
            float right = (rect.width - pivotPx.x) / ppu;
            float bottom = -(pivotPx.y) / ppu;
            float top = (rect.height - pivotPx.y) / ppu;

            Vector3[] localCorners = new Vector3[4]
            {
                new(left, bottom, 0f),
                new(left, top, 0f),
                new(right, bottom, 0f),
                new(right, top, 0f)
            };

            Matrix4x4 trs = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);

            Vector3 worldMin = new(float.PositiveInfinity, float.PositiveInfinity, 0f);
            Vector3 worldMax = new(float.NegativeInfinity, float.NegativeInfinity, 0f);

            for (int i = 0; i < localCorners.Length; i++)
            {
                Vector3 w = trs.MultiplyPoint3x4(localCorners[i]);
                if (w.x < worldMin.x) worldMin.x = w.x;
                if (w.y < worldMin.y) worldMin.y = w.y;
                if (w.x > worldMax.x) worldMax.x = w.x;
                if (w.y > worldMax.y) worldMax.y = w.y;
            }

            //Мировая позицию центра клетки (там, где pivot расположен)
            Vector3 cellPivotWorld = tilemap.GetCellCenterWorld(cellPivotIndex);

            // Смещаем AABB в мировые координаты (pivot world)
            worldMin += cellPivotWorld;
            worldMax += cellPivotWorld;

            // Диапазон клеток для проверки
            Vector3Int cellMinIndex =
                tilemap.WorldToCell(new Vector3(worldMin.x, worldMin.y, tilemap.transform.position.z));
            Vector3Int cellMaxIndex =
                tilemap.WorldToCell(new Vector3(worldMax.x, worldMax.y, tilemap.transform.position.z));

            int x0 = Mathf.Min(cellMinIndex.x, cellMaxIndex.x);
            int x1 = Mathf.Max(cellMinIndex.x, cellMaxIndex.x);
            int y0 = Mathf.Min(cellMinIndex.y, cellMaxIndex.y);
            int y1 = Mathf.Max(cellMinIndex.y, cellMaxIndex.y);

            // Размеры клетки и её площадь
            Vector3 cellSizeV = tilemap.cellSize;
            float cellW = Mathf.Abs(cellSizeV.x) > Mathf.Epsilon ? cellSizeV.x : 1f;
            float cellH = Mathf.Abs(cellSizeV.y) > Mathf.Epsilon ? cellSizeV.y : 1f;
            float cellArea = cellW * cellH;

            HashSet<System.Numerics.Vector3> occupiedSet = new();

            for (int x = x0; x <= x1; x++)
            {
                for (int y = y0; y <= y1; y++)
                {
                    Vector3Int cIdx = new Vector3Int(x, y, cellPivotIndex.z);
                    Vector3 cellCenter = tilemap.GetCellCenterWorld(cIdx);
                    Vector3 cellMinWorld = new Vector3(cellCenter.x - cellW * 0.5f, cellCenter.y - cellH * 0.5f, 0f);
                    Vector3 cellMaxWorld = new Vector3(cellCenter.x + cellW * 0.5f, cellCenter.y + cellH * 0.5f, 0f);

                    float overlapX = Mathf.Max(0f,
                        Mathf.Min(worldMax.x, cellMaxWorld.x) - Mathf.Max(worldMin.x, cellMinWorld.x));
                    float overlapY = Mathf.Max(0f,
                        Mathf.Min(worldMax.y, cellMaxWorld.y) - Mathf.Max(worldMin.y, cellMinWorld.y));
                    float overlapArea = overlapX * overlapY;

                    if (overlapArea > 0f && overlapArea / cellArea >= minOverlapFraction)
                    {
                        occupiedSet.Add(((Vector3)cIdx).ToSystem());
                    }
                }
            }

            if (occupiedSet.Count == 0) return result; 

            result.OccupiedCells = new List<System.Numerics.Vector3>(occupiedSet);

            // Для размера нужно посчитать покрытые индексы по X и Y
            HashSet<int> xs = new();
            HashSet<int> ys = new();
            foreach (System.Numerics.Vector3 ci in result.OccupiedCells)
            {
                xs.Add((int)ci.X);
                ys.Add((int)ci.Y);
            }

            result.Size = new System.Numerics.Vector2(xs.Count, ys.Count);
            return result;
        }
    }
}
