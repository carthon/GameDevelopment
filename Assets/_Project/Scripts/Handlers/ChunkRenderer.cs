using System;
using System.Collections.Generic;
using _Project.Libraries.Marching_Cubes.Scripts;
using _Project.Scripts.Components;
using UnityEngine;

namespace _Project.Scripts.Handlers {
    [Serializable]
    public class ChunkRenderer {
        public Queue<Chunk> RenderingQueue = new Queue<Chunk>();
        public List<Chunk> ActiveChunks = new List<Chunk>();
        public void GenerateChunksAround(Planet planet, Vector3 position, float radius) {
            Chunk[] chunks = planet.GetChunks();
            float closestDistance = float.MaxValue;
            Chunk closestChunk = null;
            foreach (Chunk chunk in chunks) {
                float distance = Vector3.Distance(position, chunk.GetCenter());
                if (distance < closestDistance) {
                    closestDistance = distance;
                    closestChunk = chunk;
                }
                if (distance < radius) {
                    RenderingQueue.Enqueue(chunk);
                }
            }
            if (RenderingQueue.Count <= 0)
                RenderingQueue.Enqueue(closestChunk);
            while (RenderingQueue.Count > 0) {
                Chunk chunk = RenderingQueue.Dequeue();
                if (chunk.IsLoaded() && Application.isPlaying) {
                    chunk.GetGameObject()?.SetActive(true);
                }
                else {
                    planet.Generate(chunk);
                }
                ActiveChunks.Add(chunk);
            }
            Queue<Chunk> deletedChunks = new Queue<Chunk>();
            foreach (Chunk chunk in ActiveChunks) {
                float distance = Vector3.Distance(position, chunk.GetCenter());
                if (distance > radius && ActiveChunks.Count > 1) {
                    chunk.GetGameObject()?.SetActive(false);
                    deletedChunks.Enqueue(chunk);
                }
            }
            while (deletedChunks.Count > 0)
                ActiveChunks.Remove(deletedChunks.Dequeue());
        }

        public void Clear() {
            RenderingQueue.Clear();
            ActiveChunks.Clear();
        }
    }
}