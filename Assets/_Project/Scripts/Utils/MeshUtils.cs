using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Utils {
    public class MeshUtils {
        private static readonly Dictionary<(float rows, float cols, float cellSize, float thickness), Mesh> Cache
            = new Dictionary<(float, float, float, float), Mesh>();

        public static Mesh CreateFrameMesh(float rows, float cols, float cellSize, float thickness) {
            // ─── Validación ──────────────────────────────────────────────
            if (cols     <= 0 || rows     <= 0)
                throw new ArgumentException("cols/rows must be > 0");
            if (cellSize <= 0 || thickness <= 0)
                throw new ArgumentException("cellSize/thickness must be > 0");
            if (thickness > Mathf.Min(cellSize, cellSize))
                throw new ArgumentException("thickness too large (must fit inside one cell)");

            var key = (rows, cols, cellSize, thickness);
            if (Cache.TryGetValue(key, out var cached))
                return cached;

            // ─── Dimensiones de la rejilla ───────────────────────────────
            float innerW = cols * cellSize;
            float innerH = rows * cellSize;
            float outerW = innerW;  // ya no expandimos por thickness
            // float outerH = innerH;

            // ─── Offset en Z (manténlo o modifícalo) ─────────────────────
            float zOffset = cellSize;

            // ─── Rangos en Z para aplicar thickness hacia dentro ─────────
            float zTop         =  zOffset;                   // cara superior del marco
            float zBelowTopBar =  zTop - thickness;          // base de la barra superior
            float zBottom      =  zTop - innerH;             // cara inferior del marco
            float zAboveBotBar =  zBottom + thickness;       // techo de la barra inferior

            var verts = new List<Vector3>();
            var tris  = new List<int>();

            void AddBox(Vector3 min, Vector3 max) {
                Vector3 v000 = new(min.x, min.y, min.z);
                Vector3 v100 = new(max.x, min.y, min.z);
                Vector3 v110 = new(max.x, min.y, max.z);
                Vector3 v010 = new(min.x, min.y, max.z);
                Vector3 v001 = new(min.x, max.y, min.z);
                Vector3 v101 = new(max.x, max.y, min.z);
                Vector3 v111 = new(max.x, max.y, max.z);
                Vector3 v011 = new(min.x, max.y, max.z);

                int s = verts.Count;
                void Q(Vector3 a, Vector3 b, Vector3 c, Vector3 d) {
                    verts.AddRange(new[] { a, b, c, d });
                    tris.AddRange(new[] { s, s+1, s+2,  s, s+2, s+3 });
                    s += 4;
                }

                Q(v000, v100, v110, v010); // cara “abajo”  (Y=min.y)
                Q(v011, v111, v101, v001); // cara “arriba” (Y=max.y)
                Q(v001, v101, v100, v000); // cara “frontal” (Z=min.z)
                Q(v010, v110, v111, v011); // cara “trasera” (Z=max.z)
                Q(v001, v000, v010, v011); // cara “izq.”   (X=min.x)
                Q(v100, v101, v111, v110); // cara “der.”   (X=max.x)
            }

            float y1 = thickness; // altura en Y es siempre thickness

            // 1) Barra superior: de zBelowTopBar a zTop, en todo X
            AddBox(
                new Vector3(0f,         0f,    zBelowTopBar),
                new Vector3(outerW,     y1,    zTop)
            );

            // 2) Barra inferior: de zBottom a zAboveBotBar, en todo X
            AddBox(
                new Vector3(0f,         0f,    zBottom),
                new Vector3(outerW,     y1,    zAboveBotBar)
            );

            // 3) Barra izquierda: de X=0 a X=thickness, desde zBottom a zTop
            AddBox(
                new Vector3(0f,         0f,    zBottom),
                new Vector3(thickness,  y1,    zTop)
            );

            // 4) Barra derecha: de X=outerW-thickness a X=outerW, desde zBottom a zTop
            AddBox(
                new Vector3(outerW - thickness, 0f, zBottom),
                new Vector3(outerW,              y1, zTop)
            );

            // ─── Construcción final del Mesh ────────────────────────────
            var mesh = new Mesh {
                name = $"Frame_{rows:F0}x{cols:F0}_cell{cellSize:F2}_thick{thickness:F2}"
            };
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            // Optimización recomendada en Unity 2021.3
            mesh.OptimizeIndexBuffers();
            mesh.OptimizeReorderVertexBuffer();

            Cache[key] = mesh;
            return mesh;
        }
    }
}
