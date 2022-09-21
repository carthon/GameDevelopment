using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MeshGenerator : MonoBehaviour {

    private const int threadGroupSize = 8;
    private const string chunkHolderName = "Chunks Holder";

    [Header("General Settings")]
    public DensityGenerator densityGenerator;

    public bool fixedMapSize;
    [ConditionalHide(nameof (fixedMapSize), true)]
    public Vector3Int numChunks = Vector3Int.one;
    [ConditionalHide(nameof (fixedMapSize), false)]
    public Transform viewer;
    [ConditionalHide(nameof (fixedMapSize), false)]
    public float viewDistance = 30;
    public GameObject player;

    [Space]
    public bool autoUpdateInEditor = true;
    public bool autoUpdateInGame = true;
    public ComputeShader shader;
    public Material mat;
    public bool generateColliders;

    [Header("Voxel Settings")]
    public float isoLevel;
    public float boundsSize = 1;
    public Vector3 offset = Vector3.zero;

    [Range(2, 100)]
    public int numPointsPerAxis = 30;

    [Header("Gizmos")]
    public bool showBoundsGizmo = true;
    public Color boundsGizmoCol = Color.white;

    private GameObject chunkHolder;
    private List<Chunk> chunks;
    private Dictionary<Vector3Int, Chunk> existingChunks;
    private ComputeBuffer pointsBuffer;
    private Queue<Chunk> recycleableChunks;

    private bool settingsUpdated;

    // Buffers
    private ComputeBuffer triangleBuffer;
    private ComputeBuffer triCountBuffer;

    private void Awake() {
        if (Application.isPlaying && !fixedMapSize) {
            InitVariableChunkStructures();

            var oldChunks = FindObjectsOfType<Chunk>();
            for (var i = oldChunks.Length - 1; i >= 0; i--) Destroy(oldChunks[i].gameObject);
        }
    }

    private void Update() {
        // Update endless terrain
        if (Application.isPlaying && !fixedMapSize) Run();

        if (settingsUpdated) {
            RequestMeshUpdate();
            settingsUpdated = false;
        }
    }

    private void OnDestroy() {
        if (Application.isPlaying) ReleaseBuffers();
    }

    private void OnValidate() {
        settingsUpdated = true;
    }

    public void Run() {
        CreateBuffers();

        if (fixedMapSize) {
            InitChunks();
            UpdateAllChunks();

        }
        else {
            if (Application.isPlaying) InitVisibleChunks();
        }

        // Release buffers immediately in editor
        if (!Application.isPlaying) ReleaseBuffers();

    }

    public void RequestMeshUpdate() {
        if (Application.isPlaying && autoUpdateInGame || !Application.isPlaying && autoUpdateInEditor) Run();
    }

    private void InitVariableChunkStructures() {
        recycleableChunks = new Queue<Chunk>();
        chunks = new List<Chunk>();
        existingChunks = new Dictionary<Vector3Int, Chunk>();
    }

    private void InitVisibleChunks() {
        if (chunks == null) return;

        CreateChunkHolder();

        var p = viewer.position;
        var ps = p / boundsSize;
        var viewerCoord = new Vector3Int(Mathf.RoundToInt(ps.x), Mathf.RoundToInt(ps.y), Mathf.RoundToInt(ps.z));

        var maxChunksInView = Mathf.CeilToInt(viewDistance / boundsSize);
        var sqrViewDistance = viewDistance * viewDistance;

        // Go through all existing chunks and flag for recyling if outside of max view dst
        for (var i = chunks.Count - 1; i >= 0; i--) {
            var chunk = chunks[i];
            var centre = CentreFromCoord(chunk.coord);
            var viewerOffset = p - centre;
            var o = new Vector3(Mathf.Abs(viewerOffset.x), Mathf.Abs(viewerOffset.y), Mathf.Abs(viewerOffset.z)) - Vector3.one * boundsSize / 2;
            var sqrDst = new Vector3(Mathf.Max(o.x, 0), Mathf.Max(o.y, 0), Mathf.Max(o.z, 0)).sqrMagnitude;
            if (sqrDst > sqrViewDistance) {
                existingChunks.Remove(chunk.coord);
                recycleableChunks.Enqueue(chunk);
                chunks.RemoveAt(i);
            }
        }

        for (var x = -maxChunksInView; x <= maxChunksInView; x++)
        for (var y = -maxChunksInView; y <= maxChunksInView; y++)
        for (var z = -maxChunksInView; z <= maxChunksInView; z++) {
            var coord = new Vector3Int(x, y, z) + viewerCoord;

            if (existingChunks.ContainsKey(coord)) continue;

            var centre = CentreFromCoord(coord);
            var viewerOffset = p - centre;
            var o = new Vector3(Mathf.Abs(viewerOffset.x), Mathf.Abs(viewerOffset.y), Mathf.Abs(viewerOffset.z)) - Vector3.one * boundsSize / 2;
            var sqrDst = new Vector3(Mathf.Max(o.x, 0), Mathf.Max(o.y, 0), Mathf.Max(o.z, 0)).sqrMagnitude;

            // Chunk is within view distance and should be created (if it doesn't already exist)
            if (sqrDst <= sqrViewDistance) {

                var bounds = new Bounds(CentreFromCoord(coord), Vector3.one * boundsSize);
                if (IsVisibleFrom(bounds, Camera.main)) {
                    if (recycleableChunks.Count > 0) {
                        var chunk = recycleableChunks.Dequeue();
                        chunk.coord = coord;
                        existingChunks.Add(coord, chunk);
                        chunks.Add(chunk);
                        UpdateChunkMesh(chunk);
                        if (generateColliders)
                            chunk.UpdateCollider();
                    }
                    else {
                        var chunk = CreateChunk(coord);
                        chunk.coord = coord;
                        chunk.SetUp(mat, generateColliders);
                        existingChunks.Add(coord, chunk);
                        chunks.Add(chunk);
                        UpdateChunkMesh(chunk);
                        if (generateColliders)
                            chunk.UpdateCollider();
                    }
                }
            }

        }
    }

    public bool IsVisibleFrom(Bounds bounds, Camera camera) {
        var planes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(planes, bounds);
    }

    public void UpdateChunkMesh(Chunk chunk) {
        var numVoxelsPerAxis = numPointsPerAxis - 1;
        var numThreadsPerAxis = Mathf.CeilToInt(numVoxelsPerAxis / (float) threadGroupSize);
        var pointSpacing = boundsSize / (numPointsPerAxis - 1);

        var coord = chunk.coord;
        var centre = CentreFromCoord(coord);

        var worldBounds = new Vector3(numChunks.x, numChunks.y, numChunks.z) * boundsSize;

        densityGenerator.Generate(pointsBuffer, numPointsPerAxis, boundsSize, worldBounds, centre, offset, pointSpacing);

        triangleBuffer.SetCounterValue(0);
        shader.SetBuffer(0, "points", pointsBuffer);
        shader.SetBuffer(0, "triangles", triangleBuffer);
        shader.SetInt("numPointsPerAxis", numPointsPerAxis);
        shader.SetFloat("isoLevel", isoLevel);

        shader.Dispatch(0, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);

        // Get number of triangles in the triangle buffer
        ComputeBuffer.CopyCount(triangleBuffer, triCountBuffer, 0);
        int[] triCountArray = {0};
        triCountBuffer.GetData(triCountArray);
        var numTris = triCountArray[0];

        // Get triangle data from shader
        var tris = new Triangle[numTris];
        triangleBuffer.GetData(tris, 0, 0, numTris);

        var mesh = chunk.mesh;
        mesh.Clear();

        var vertices = new Vector3[numTris * 3];
        var meshTriangles = new int[numTris * 3];

        for (var i = 0; i < numTris; i++)
        for (var j = 0; j < 3; j++) {
            meshTriangles[i * 3 + j] = i * 3 + j;
            vertices[i * 3 + j] = tris[i][j];
        }
        mesh.vertices = vertices;
        mesh.triangles = meshTriangles;
        mesh.RecalculateNormals();
    }

    public void UpdateAllChunks() {

        // Create mesh for each chunk
        foreach (var chunk in chunks) UpdateChunkMesh(chunk);

    }

    private void CreateBuffers() {
        var numPoints = numPointsPerAxis * numPointsPerAxis * numPointsPerAxis;
        var numVoxelsPerAxis = numPointsPerAxis - 1;
        var numVoxels = numVoxelsPerAxis * numVoxelsPerAxis * numVoxelsPerAxis;
        var maxTriangleCount = numVoxels * 5;

        // Always create buffers in editor (since buffers are released immediately to prevent memory leak)
        // Otherwise, only create if null or if size has changed
        if (!Application.isPlaying || pointsBuffer == null || numPoints != pointsBuffer.count) {
            if (Application.isPlaying) ReleaseBuffers();
            triangleBuffer = new ComputeBuffer(maxTriangleCount, sizeof (float) * 3 * 3, ComputeBufferType.Append);
            pointsBuffer = new ComputeBuffer(numPoints, sizeof (float) * 4);
            triCountBuffer = new ComputeBuffer(1, sizeof (int), ComputeBufferType.Raw);

        }
    }

    private void ReleaseBuffers() {
        if (triangleBuffer != null) {
            triangleBuffer.Release();
            pointsBuffer.Release();
            triCountBuffer.Release();
        }
    }

    private Vector3 CentreFromCoord(Vector3Int coord) {
        // Centre entire map at origin
        if (fixedMapSize) {
            var totalBounds = (Vector3) numChunks * boundsSize;
            return -totalBounds / 2 + (Vector3) coord * boundsSize + Vector3.one * boundsSize / 2;
        }

        return new Vector3(coord.x, coord.y, coord.z) * boundsSize;
    }

    private void CreateChunkHolder() {
        // Create/find mesh holder object for organizing chunks under in the hierarchy
        if (chunkHolder == null) {
            if (GameObject.Find(chunkHolderName))
                chunkHolder = GameObject.Find(chunkHolderName);
            else
                chunkHolder = new GameObject(chunkHolderName);
        }
    }

    // Create/get references to all chunks
    private void InitChunks() {
        CreateChunkHolder();
        chunks = new List<Chunk>();
        var oldChunks = new List<Chunk>(FindObjectsOfType<Chunk>());

        // Go through all coords and create a chunk there if one doesn't already exist
        for (var x = 0; x < numChunks.x; x++)
        for (var y = 0; y < numChunks.y; y++)
        for (var z = 0; z < numChunks.z; z++) {
            var coord = new Vector3Int(x, y, z);
            var chunkAlreadyExists = false;

            // If chunk already exists, add it to the chunks list, and remove from the old list.
            for (var i = 0; i < oldChunks.Count; i++)
                if (oldChunks[i].coord == coord) {
                    chunks.Add(oldChunks[i]);
                    oldChunks.RemoveAt(i);
                    chunkAlreadyExists = true;
                    break;
                }

            // Create new chunk
            if (!chunkAlreadyExists) {
                var newChunk = CreateChunk(coord);
                chunks.Add(newChunk);
            }

            chunks[chunks.Count - 1].SetUp(mat, generateColliders);
        }

        // Delete all unused chunks
        for (var i = 0; i < oldChunks.Count; i++) oldChunks[i].DestroyOrDisable();
    }

    private Chunk CreateChunk(Vector3Int coord) {
        var chunk = new GameObject($"Chunk ({coord.x}, {coord.y}, {coord.z})");
        chunk.transform.parent = chunkHolder.transform;
        var newChunk = chunk.AddComponent<Chunk>();
        newChunk.coord = coord;
        return newChunk;
    }

    private struct Triangle {
#pragma warning disable 649 // disable unassigned variable warning
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;

        public Vector3 this[int i]
        {
            get
            {
                switch (i) {
                    case 0:
                        return a;
                    case 1:
                        return b;
                    default:
                        return c;
                }
            }
        }
    }

    private void OnDrawGizmos() {
        if (showBoundsGizmo) {
            Gizmos.color = boundsGizmoCol;

            var chunks = this.chunks == null ? new List<Chunk>(FindObjectsOfType<Chunk>()) : this.chunks;
            foreach (var chunk in chunks) {
                var bounds = new Bounds(CentreFromCoord(chunk.coord), Vector3.one * boundsSize);
                Gizmos.color = boundsGizmoCol;
                Gizmos.DrawWireCube(CentreFromCoord(chunk.coord), Vector3.one * boundsSize);
            }
        }
    }

}