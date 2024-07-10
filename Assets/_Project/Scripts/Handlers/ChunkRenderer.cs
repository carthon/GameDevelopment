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
        private Vector3Int _centerChunk;
        private int _chunksToRender;
        private Chunk _lastChunkVisited;
        [SerializeField] private bool _isLoading = false;
        public void GenerateChunksAround(Planet planet, Vector3 position, int nChunks) {
            _lastPlanet = planet;
            _lastPosition = position;
            _chunksToRender = nChunks;
            _lastChunkVisited ??= planet.GetClosestChunk(position);
            if(!_lastChunkVisited.IsInBounds(position)) _lastChunkVisited = planet.FindChunkAtPosition(position);
            RenderingQueue.Enqueue(_lastChunkVisited);
            if (_lastPlanet != planet) {
                _centerChunk = planet.FindChunkAtPosition(planet.Center).GetCoords();
            }
            // Determine chunks to load and unload
            if(_lastChunkVisited is not null)
                for (int x = -nChunks; x <= nChunks; x++)
                for (int y = -nChunks; y <= nChunks; y++)
                for (int z = -nChunks; z <= nChunks; z++) {
                    Vector3 positionToCenter = _lastChunkVisited.GetCoords() - _centerChunk;
                    Vector3Int chunkCoord = new Vector3Int(_lastChunkVisited.GetCoords().x + x, _lastChunkVisited.GetCoords().y + y, _lastChunkVisited.GetCoords().z + z);
                    Vector3 currentVector = chunkCoord - planet.Center;
                    float dotProduct = Vector3.Dot(positionToCenter, currentVector);
                    //Calculamos la distancia entre el punto del jugador y el punto del chunk, si el chunk está en el lado opuesto al jugador desde el centro
                    //No renderizamos los chunks para ahorrar tiempo de procesamiento
                    if (dotProduct < 0) {
                        continue;
                    }
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
                    if (chunk.IsLoaded && Application.isPlaying) {
                        if (!chunk.IsActive) {
                            chunk.GetGameObject()?.SetActive(true);
                            chunk.IsActive = true;
                        }
                    }
                    else {
                        _lastPlanet.Generate(chunk);
                    }
                    ActiveChunks.TryAdd(chunk.GetCoords(),chunk);
                }
                foreach (Chunk activeChunk in ActiveChunks.Values) {
                    float magnitude = (activeChunk.GetCoords() - _lastChunkVisited.GetCoords()).magnitude;
                    //float distance = Vector3.Distance(_lastPosition, activeChunk.GetCenter());
                    if (magnitude > _chunksToRender && activeChunk.IsActive) {
                        activeChunk.GetGameObject().SetActive(false);
                        activeChunk.IsActive = false;
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