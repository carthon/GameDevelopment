using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using _Project.Libraries.Marching_Cubes.Scripts;
using _Project.Scripts.Components;
using UnityEngine;

namespace _Project.Scripts.Handlers {
    [Serializable]
    public class ChunkRenderer : MonoBehaviour{
        public Queue<Chunk> RenderingQueue = new Queue<Chunk>();
        public Queue<Chunk> UnloadingQueue = new Queue<Chunk>();
        public Dictionary<Vector3Int, Chunk> ActiveChunks = new Dictionary<Vector3Int, Chunk>();
        private Planet _lastPlanet;
        private Vector3 _lastPosition;
        private int _chunksToRender;
        private Chunk _lastChunkVisited;
        [SerializeField] private bool _isLoading = false;
        public void GenerateChunksAround(Planet planet, Vector3 position, float nChunks) {
            _lastPlanet = planet;
            _lastPosition = position;
            _lastChunkVisited = planet.FindChunkAtPosition(position);
            _chunksToRender = Mathf.FloorToInt(nChunks);
            _lastChunkVisited ??= planet.GetClosestChunk(position);
            RenderingQueue.Enqueue(_lastChunkVisited);
            // Determine chunks to load and unload
            if(_lastChunkVisited is not null)
                for (int x = (int)-Math.Floor(nChunks); x <= nChunks; x++)
                for (int y = (int)-Math.Floor(nChunks); y <= nChunks; y++)
                for (int z = (int)-Math.Floor(nChunks); z <= nChunks; z++) {
                    Vector3Int chunkCoord = new Vector3Int(_lastChunkVisited.GetCoords().x + x, _lastChunkVisited.GetCoords().y + y, _lastChunkVisited.GetCoords().z + z);
                    Chunk chunk = planet.GetChunkAtCoords(chunkCoord);
                    if (chunk is not null) {
                        RenderingQueue.Enqueue(chunk);
                    }
                }
            if(!_isLoading)
                StartCoroutine(LoadChunks());
            else
                Debug.LogError("Chunks are already generating!");
            while (UnloadingQueue.Count > 0)
                ActiveChunks.Remove(UnloadingQueue.Dequeue().GetCoords());
        }
        private IEnumerator LoadChunks() {
            _isLoading = true;
            try {
                while (RenderingQueue.Count > 0) {
                    Chunk chunk = RenderingQueue.Dequeue();
                    if (chunk.IsLoaded() && Application.isPlaying) {
                        chunk.GetGameObject()?.SetActive(true);
                    }
                    else {
                        _lastPlanet.Generate(chunk);
                    }
                    ActiveChunks.TryAdd(chunk.GetCoords(),chunk);
                }
                foreach (Chunk activeChunk in ActiveChunks.Values) {
                    float magnitude = (activeChunk.GetCoords() - _lastChunkVisited.GetCoords()).magnitude;
                    //float distance = Vector3.Distance(_lastPosition, activeChunk.GetCenter());
                    if (magnitude > _chunksToRender) {
                        activeChunk.GetGameObject().SetActive(false);
                        UnloadingQueue.Enqueue(activeChunk);
                    }
                }
            }
            finally {
                _isLoading = false;
            }
            yield return null;
        }

        public void Clear() {
            RenderingQueue.Clear();
            ActiveChunks.Clear();
            
        }
    }
}