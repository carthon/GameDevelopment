using System;
using System.Collections.Generic;
using _Project.Helper.Compute_Helper;
using _Project.Libraries.Marching_Cubes.Scripts;
using _Project.Scripts.Components;
using _Project.Scripts.Handlers;
using UnityEditor;
using UnityEngine;

namespace Editor {
    [CustomEditor(typeof(Planet))]
    public class PlanetEditor : UnityEditor.Editor {
        private Planet _planet;
        private bool _showOriginal2DMap;
        private ChunkRenderer _chunkRenderer;
        public override void OnInspectorGUI() {
            EditorGUI.BeginChangeCheck();
            DrawDefaultInspector();
            if (EditorGUI.EndChangeCheck()) {
                OnValidate();
            }
            _planet = (Planet) target;
            _planet.chunkGenerationRadius = (int) EditorGUILayout.Slider("Spawn Chunk Render Distance", _planet.chunkGenerationRadius, 1, 20);
            _showOriginal2DMap = EditorGUILayout.Toggle("Show Original 2D Map", _showOriginal2DMap);
            if(GUILayout.Button("Generate Planet")) {
                _planet.SetUp();
                _planet.Generate();
            }
            if(GUILayout.Button("Generate Spawn Chunk")) {
                _planet.SetUp();
                _chunkRenderer ??= FindObjectOfType<ChunkRenderer>();
                _chunkRenderer.Clear();
                _chunkRenderer.GenerateChunksAround(_planet, FindObjectOfType<GameManager>().spawnPoint.position, _planet.chunkGenerationRadius);
            }
            if(GUILayout.Button("Destroy Planet")) {
                _planet.Delete();
                _chunkRenderer ??= FindObjectOfType<ChunkRenderer>();
                _chunkRenderer.Clear();
            }
            if(GUILayout.Button("Refresh Planet"))
                RegeneratePlanet();
            // Si hay una RenderTexture, dibujarla
            if (_showOriginal2DMap && _planet.MeshGenerator is not null 
                && _planet.MeshGenerator.originalMap2D is not null && _planet.MeshGenerator.continentalness is not null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
                GUILayout.Label(_planet.MeshGenerator.originalMap2D.name, EditorStyles.boldLabel);
                Rect rect1 = GUILayoutUtility.GetRect(128, 128, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                EditorGUI.DrawPreviewTexture(rect1, _planet.MeshGenerator.originalMap2D);
                GUILayout.EndVertical();
                // Espacio flexible entre las texturas
                GUILayout.FlexibleSpace();
                GUILayout.BeginVertical();
                GUILayout.Label(_planet.MeshGenerator.continentalness.name, EditorStyles.boldLabel);
                Rect rect2 = GUILayoutUtility.GetRect(128, 128, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                EditorGUI.DrawPreviewTexture(rect2, _planet.MeshGenerator.continentalness);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
        }
        private void RegeneratePlanet() {
            _planet.GenerateDensityMap();
            _planet.MeshGenerator.OnValidate();
            if (_chunkRenderer.ActiveChunks.Count > 0) {
                _chunkRenderer ??= FindObjectOfType<ChunkRenderer>();
                _chunkRenderer.Clear();
                _chunkRenderer.GenerateChunksAround(_planet, FindObjectOfType<GameManager>().spawnPoint.position, _planet.chunkGenerationRadius);
            }
            else
                _planet.Generate();
        }
        private void OnValidate() {
            if (_planet.MeshGenerator is not null && _planet.MeshGenerator.updateOnEditor) {
                RegeneratePlanet();
            }
        }
        void OnSceneGUI() {
            _planet = (Planet) target;
            if (_planet.showChunkBoundaries && _planet.MeshGenerator is not null) {
                float chunkSize = _planet.MeshGenerator.boundsSize / _planet.NumChunks;
                _chunkRenderer ??= FindObjectOfType<ChunkRenderer>();
                if (_chunkRenderer.ActiveChunks is not null && _chunkRenderer.ActiveChunks.Count > 0) {
                    foreach (Chunk renderedChunk in _chunkRenderer.ActiveChunks.Values) {
                        Handles.DrawWireCube(renderedChunk.GetCenter(), Vector3.one * chunkSize);
                    }
                    return;
                }
                // Establece el color del Gizmo
                Handles.color = Color.green;
                for (int y = 0; y < _planet.NumChunks; y++)
                for (int x = 0; x < _planet.NumChunks; x++)
                for (int z = 0; z < _planet.NumChunks; z++) {
                    float posX = (-(_planet.NumChunks - 1f) / 2 + x) * chunkSize;
                    float posY = (-(_planet.NumChunks - 1f) / 2 + y) * chunkSize;
                    float posZ = (-(_planet.NumChunks - 1f) / 2 + z) * chunkSize;
                    Handles.DrawWireCube(new Vector3(posX, posY, posZ) + _planet.PlanetData.Center, Vector3.one * chunkSize);
                }
            }
        }
    }
}