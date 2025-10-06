using UnityEngine;
using UnityEngine.Tilemaps;
using Game.Scripts.Domain.Game;
using System.Collections.Generic;

namespace Game.Scripts.Infrastructure.Utilities
{
    public static class TilemapExtensions
    {
        public static Occupancy GetOccupiedCellsRelativeToPivotCell
        (
            Sprite sprite,
            Tilemap tilemap,
            Vector3Int cellPivotIndex,
            Vector3 lossyScale = default,
            Quaternion rotation = default,
            float minOverlapFraction = 0.02f
        )
        {
            Occupancy result = new()
            {
                Size = Vector2.zero.ToSystem(),
                OccupiedCells = new List<System.Numerics.Vector3>()
            };

            if (sprite == null || tilemap == null) return result;

            minOverlapFraction = Mathf.Clamp01(minOverlapFraction);

            // Если lossyscale не задан (равен default (0,0,0)), используем Vector3.one
            if (lossyScale == default(Vector3) || float.IsNaN(lossyScale.x) || float.IsNaN(lossyScale.y) || float.IsNaN(lossyScale.z))
            {
                lossyScale = Vector3.one;
            }

            // Защита и нормализация кватерниона: заменяем нулевой/плохой на identity
            const float kQuatEpsilon = 1e-6f;
            if (float.IsNaN(rotation.x) || float.IsNaN(rotation.y) || float.IsNaN(rotation.z) || float.IsNaN(rotation.w))
            {
                rotation = Quaternion.identity;
            }
            else
            {
                float lenSq = rotation.x * rotation.x + rotation.y * rotation.y + rotation.z * rotation.z + rotation.w * rotation.w;
                if (lenSq < kQuatEpsilon)
                {
                    rotation = Quaternion.identity;
                }
                else
                {
                    float invLen = 1.0f / Mathf.Sqrt(lenSq);
                    rotation = new Quaternion(rotation.x * invLen, rotation.y * invLen, rotation.z * invLen, rotation.w * invLen);
                }
            }

            // 1) Получаем локальные границы спрайта относительно pivot.
            Rect rect = sprite.rect;
            Vector2 pivotPx = sprite.pivot;
            float ppu = sprite.pixelsPerUnit;

            float left = -(pivotPx.x) / ppu;
            float right = (rect.width - pivotPx.x) / ppu;
            float bottom = -(pivotPx.y) / ppu;
            float top = (rect.height - pivotPx.y) / ppu;

            Vector3[] localCorners = new Vector3[4]
            {
                new Vector3(left, bottom, 0f),
                new Vector3(left, top, 0f),
                new Vector3(right, bottom, 0f),
                new Vector3(right, top, 0f)
            };

            // --- безопасно создаём TRS ---
            Matrix4x4 trs = Matrix4x4.TRS(Vector3.zero, rotation, lossyScale);

            Vector3 worldMin = new Vector3(float.PositiveInfinity, float.PositiveInfinity, 0f);
            Vector3 worldMax = new Vector3(float.NegativeInfinity, float.NegativeInfinity, 0f);

            for (int i = 0; i < localCorners.Length; i++)
            {
                Vector3 w = trs.MultiplyPoint3x4(localCorners[i]);
                if (w.x < worldMin.x) worldMin.x = w.x;
                if (w.y < worldMin.y) worldMin.y = w.y;
                if (w.x > worldMax.x) worldMax.x = w.x;
                if (w.y > worldMax.y) worldMax.y = w.y;
            }

            // 4) Получим мировую позицию центра эталонной клетки (там, где pivot расположен)
            Vector3 cellPivotWorld = tilemap.GetCellCenterWorld(cellPivotIndex);

            // Смещаем AABB в мировые координаты (pivot world)
            worldMin += (Vector3)cellPivotWorld;
            worldMax += (Vector3)cellPivotWorld;

            // 5) Диапазон клеток для проверки
            Vector3Int cellMinIndex =
                tilemap.WorldToCell(new Vector3(worldMin.x, worldMin.y, tilemap.transform.position.z));
            Vector3Int cellMaxIndex =
                tilemap.WorldToCell(new Vector3(worldMax.x, worldMax.y, tilemap.transform.position.z));

            int x0 = Mathf.Min(cellMinIndex.x, cellMaxIndex.x);
            int x1 = Mathf.Max(cellMinIndex.x, cellMaxIndex.x);
            int y0 = Mathf.Min(cellMinIndex.y, cellMaxIndex.y);
            int y1 = Mathf.Max(cellMinIndex.y, cellMaxIndex.y);

            // 6) Получаем размеры клетки и её площадь
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

            if (occupiedSet.Count == 0)
            {
                return result; // пусто
            }

            // 7) Приведём список и вычислим размер относительно pivotIndex
            result.OccupiedCells = new List<System.Numerics.Vector3>(occupiedSet);

            // Для размера нужно посчитать покрытые индексы по X и Y (количество уникальных индекс-столбцов/строк)
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
