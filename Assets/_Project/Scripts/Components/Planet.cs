using System;
using System.Diagnostics;
using _Project.Libraries.Marching_Cubes.Scripts;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;

namespace _Project.Scripts.Components {
    [Serializable]
    public struct PlanetData {
        public Vector3 Center;
        public float Gravity;
    }
    public class Planet : MonoBehaviour {
        [FormerlySerializedAs("_gravityRadius")] [SerializeField]
        private float gravityRadius;
        [FormerlySerializedAs("_groundLayer")] [SerializeField]
        private LayerMask groundLayer;
        [SerializeField] private int numChunks;
        public bool showChunkBoundaries;
        [HideInInspector] public int chunkGenerationRadius;
        [SerializeField]
        private PlanetData _planetData;
        public PlanetData PlanetData => _planetData;
        public int NumChunks => numChunks;
        public float GravityRadius => gravityRadius;
        public MeshGenerator MeshGenerator => _meshGenerator;
        public LayerMask GroundLayer => groundLayer;
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
            _planetData.Center = transform.position;
            if (chunks == null || chunks.Length != numChunks * numChunks * numChunks)
                CreateChunks();
            _meshGenerator.InitMesh(this);
            GenerateDensityMap();
        }
        public void GenerateDensityMap() {
            _meshGenerator.ComputeDensity(Vector3.zero);
        }
        public void Generate() { GenerateAllChunks(); }
        public void Generate(Chunk chunk) {
            _meshGenerator.GenerateChunk(chunk);
        }
        private int FindChunkIndexByPosition(Vector3 position) {
            // Normalizar la posición al rango de chunks
            Vector3 localPosition = position - _planetData.Center;
            float chunkSize = (_meshGenerator.boundsSize) / numChunks;
            int x = Mathf.FloorToInt((localPosition.x / chunkSize) + ((float) numChunks / 2));
            int y = Mathf.FloorToInt((localPosition.y / chunkSize) + ((float) numChunks / 2));
            int z = Mathf.FloorToInt((localPosition.z / chunkSize) + ((float) numChunks / 2));

            // Asegurarse de que las coordenadas están dentro del rango válido
            if (!IsPositionInPlanet(new Vector3Int(x,y,z))) {
                return -1;
            }
            return z + (x * numChunks) + (y * numChunks * numChunks);
        }
        private int GetChunkIndex(Vector3Int coords) {
            int x = coords.x;
            int y = coords.y;
            int z = coords.z;
            if (!IsPositionInPlanet(new Vector3Int(x, y, z)))
                return -1;
            return z + (x * numChunks) + (y * numChunks * numChunks);
        }
        public Chunk FindChunkAtPosition(Vector3 position) {
            int index = FindChunkIndexByPosition(position);
            if (index == -1 || chunks is null)
                return null;
            return chunks[index];
        }
        public Chunk GetChunkAtCoords(Vector3Int coords) {
            int index = GetChunkIndex(coords);
            if (index == -1 || chunks is null)
                return null;
            return chunks[index];
        }
        public Chunk GetClosestChunk(Vector3 position) {
            Vector3 localPosition = position - _planetData.Center;
            Vector3 lastChunkInBounds = new Vector3(ClampToBounds(localPosition.x), ClampToBounds(localPosition.y), ClampToBounds(localPosition.z));
            return FindChunkAtPosition(lastChunkInBounds);
        }
        private float ClampToBounds(float value) {
            float maxBoundsReach = (_meshGenerator.boundsSize / 2) - 1;
            return Mathf.Clamp(value, -maxBoundsReach, maxBoundsReach);
        }
        private bool IsPositionInPlanet(Vector3Int chunkPosition) => !(chunkPosition.x < 0 || chunkPosition.x >= numChunks ||
            chunkPosition.y < 0 || chunkPosition.y >= numChunks ||
            chunkPosition.z < 0 || chunkPosition.z >= numChunks);
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
                        Vector3 centre = relativeCentre + _planetData.Center; // Centro relativo del chunk

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

            // Create timers:
            timer_fetchVertexData = new Stopwatch();
            timer_processVertexData = new Stopwatch();

            totalVerts = 0;
            
            if(chunks is not null)
                foreach (Chunk t in chunks) {
                    _meshGenerator.GenerateChunk(t);
                }
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
        public float GetDensityAtPoint(Vector3 point) => _meshGenerator.GetDensityAtPoint(point);
        public float GetHeightMapValuesAtPoint(Vector3 point) {
            return _meshGenerator.GetHeightMapValuesAtPoint(point);
        }
        public Chunk[] GetChunks() => chunks;
        private void OnDestroy() {
            Delete();
        }
    }
}