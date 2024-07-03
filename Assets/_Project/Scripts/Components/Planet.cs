using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using _Project.Libraries.Marching_Cubes.Scripts;
using _Project.Scripts.Handlers;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;

namespace _Project.Scripts.Components {
    public class Planet : MonoBehaviour {
        [FormerlySerializedAs("_gravityRadius")] [SerializeField]
        private float gravityRadius;
        [FormerlySerializedAs("_center")]
        private Vector3 center;
        [FormerlySerializedAs("_groundLayer")] [SerializeField]
        private LayerMask groundLayer;
        [FormerlySerializedAs("_gravity")] [SerializeField]
        private float gravity;
        [SerializeField] private int numChunks;
        public bool showChunkBoundaries;
        [HideInInspector] public float chunkGenerationRadius;
        public float Gravity => gravity;
        public int NumChunks => numChunks;
        public float GravityRadius => gravityRadius;
        public MeshGenerator MeshGenerator => _meshGenerator;
        public LayerMask GroundLayer => groundLayer;
        public Vector3 Center => center;
        private SphereCollider _collider;
        private MeshGenerator _meshGenerator;
        private Chunk[] chunks;
        // Stopwatches
        Stopwatch timer_fetchVertexData;
        Stopwatch timer_processVertexData;
        int totalVerts;

        private void Start() {
            SetUp();
        }
        public void SetUp() {
            Assert.IsFalse(numChunks <= 2, "Must be at least 3 chunks");
            _collider = GetComponent<SphereCollider>();
            _meshGenerator = GetComponent<MeshGenerator>();
            if (_collider is null) {
                _collider = gameObject.AddComponent<SphereCollider>();
            }
            _collider.radius = gravityRadius;
            _collider.center = Vector3.zero;
            center = transform.position;
            if (chunks == null || chunks.Length != numChunks * numChunks * numChunks)
                CreateChunks();
            _meshGenerator.InitMesh(this);
            GenerateDensityMap();
        }
        public void GenerateDensityMap() {
            _meshGenerator.ComputeDensity();
            _meshGenerator.CreateRenderTextures();
        }
        public void Generate() { GenerateAllChunks(); }
        public void Generate(Chunk chunk) {
            _meshGenerator.GenerateChunk(chunk);
        }
        void CreateChunks() {
            chunks = new Chunk[numChunks * numChunks * numChunks];
            float chunkSize = (_meshGenerator.boundsSize) / numChunks;
            int i = 0;

            for (int y = 0; y < numChunks; y++) {
                for (int x = 0; x < numChunks; x++) {
                    for (int z = 0; z < numChunks; z++) {
                        Vector3Int coord = new Vector3Int(x, y, z);
                        float posX = (-(numChunks - 1f) / 2 + x) * chunkSize;
                        float posY = (-(numChunks - 1f) / 2 + y) * chunkSize;
                        float posZ = (-(numChunks - 1f) / 2 + z) * chunkSize;
                        Vector3 relativeCentre = new Vector3(posX, posY, posZ);
                        Vector3 centre = relativeCentre + center; // Centro relativo del chunk

                        GameObject meshHolder = new GameObject($"Chunk ({x}, {y}, {z})");
                        meshHolder.transform.parent = transform;
                        meshHolder.layer = gameObject.layer;

                        Chunk chunk = new Chunk(coord, centre, chunkSize, _meshGenerator.numPointsPerAxis, meshHolder);
                        chunk.SetMaterial(_meshGenerator.material);
                        chunks[i] = chunk;
                        i++;
                    }
                }
            }
        }
        
        public void GenerateAllChunks() {
            Assert.IsTrue(chunks is null || chunks.Length > 0, "No chunks to generate");
            var sw = Stopwatch.StartNew();
            Debug.Log("Generation Time: " + sw.ElapsedMilliseconds + " ms");

            // Create timers:
            timer_fetchVertexData = new Stopwatch();
            timer_processVertexData = new Stopwatch();

            totalVerts = 0;
            
            if(chunks is not null)
                foreach (Chunk t in chunks) {
                    _meshGenerator.GenerateChunk(t);
                }
            Debug.Log("Total verts " + totalVerts);

            // Print timers:
            Debug.Log("Fetch vertex data: " + timer_fetchVertexData.ElapsedMilliseconds + " ms");
            Debug.Log("Process vertex data: " + timer_processVertexData.ElapsedMilliseconds + " ms");
            Debug.Log("Sum: " + (timer_fetchVertexData.ElapsedMilliseconds + timer_processVertexData.ElapsedMilliseconds));
        }
        public void Delete() {
            if (_meshGenerator is not null)
                _meshGenerator.ReleaseBuffers();
            if (!(chunks is null) && chunks.Length > 0) {
                foreach (Chunk chunk in chunks) {
                    chunk.Release();
                }
            }
            chunks = null;
            while (transform.childCount > 0) {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }
        }
        public Chunk[] GetChunks() => chunks;
        private void OnDestroy() {
            Delete();
        }
    }
}