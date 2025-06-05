using System.Collections.Generic;
using _Project.Scripts.DataClasses;
using _Project.Scripts.DataClasses.WorldData;
using _Project.Scripts.Entities;
using _Project.Scripts.Utils.Helper.Compute_Helper;
using Unity.Mathematics;
using UnityEngine;

namespace _Project.Libraries.Marching_Cubes.Scripts {
	public class Chunk {
		private ChunkData chunkData;
		public Mesh mesh;

		public ComputeBuffer pointsBuffer;
		public int numPointsPerAxis;
		public MeshFilter filter;
		MeshRenderer renderer;
		MeshCollider collider;
		public bool terra;

		// Mesh processing
		Dictionary<int2, int> vertexIndexMap;
		List<Vector3> processedVertices;
		List<Vector3> processedNormals;
		List<int> processedTriangles;
		private bool _isLoaded = false;
		private bool _isActive = false;

		private List<IEntity> _chunkEntities = new List<IEntity>();

		public float GetSize() => chunkData.size;
		public Vector3Int GetCoords() => chunkData.id;
		public Vector3 GetCenter() => chunkData.centre;

		public bool IsLoaded {
			set => _isLoaded = true;
			get => _isLoaded;
		}
		public bool IsActive
		{
			get => _isActive;
			set {
				_isActive = value;
				foreach (IEntity entity in _chunkEntities) {
					GameObject gameObject = entity.GetGameObject();
					if (gameObject is not null) gameObject.SetActive(value);
				}
			}
		}
		public GameObject GetGameObject() => renderer.gameObject;
		public Chunk(Vector3Int coord, Vector3 centre, float size, int numPointsPerAxis, GameObject meshHolder) {
			this.chunkData = new ChunkData(coord, centre, size);
			this.numPointsPerAxis = numPointsPerAxis;

			mesh = new Mesh();
			mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

			int numPointsTotal = numPointsPerAxis * numPointsPerAxis * numPointsPerAxis;
			ComputeHelper.CreateStructuredBuffer<PointData>(ref pointsBuffer, numPointsTotal);

			// Mesh rendering and collision components
			filter = meshHolder.AddComponent<MeshFilter>();
			renderer = meshHolder.AddComponent<MeshRenderer>();


			filter.mesh = mesh;
			collider = renderer.gameObject.AddComponent<MeshCollider>();

			vertexIndexMap = new Dictionary<int2, int>();
			processedVertices = new List<Vector3>();
			processedNormals = new List<Vector3>();
			processedTriangles = new List<int>();
		}
		public void CreateMesh(VertexData[] vertexData, int numVertices, bool useFlatShading)
		{

			vertexIndexMap.Clear();
			processedVertices.Clear();
			processedNormals.Clear();
			processedTriangles.Clear();

			int triangleIndex = 0;

			for (int i = 0; i < numVertices; i++)
			{
				VertexData data = vertexData[i];

				int sharedVertexIndex;
				if (!useFlatShading && vertexIndexMap.TryGetValue(data.id, out sharedVertexIndex))
				{
					processedTriangles.Add(sharedVertexIndex);
				}
				else
				{
					if (!useFlatShading)
					{
						vertexIndexMap.Add(data.id, triangleIndex);
					}
					processedVertices.Add(data.position);
					processedNormals.Add(data.normal);
					processedTriangles.Add(triangleIndex);
					triangleIndex++;
				}
			}

			if(collider)
				collider.sharedMesh = null;

			mesh.Clear();
			mesh.SetVertices(processedVertices);
			mesh.SetTriangles(processedTriangles, 0, true);

			if (useFlatShading)
			{
				mesh.RecalculateNormals();
			}
			else
			{
				mesh.SetNormals(processedNormals);
			}
			if(collider && Application.isPlaying) {
				collider.sharedMesh = mesh;
			}
			IsLoaded = true;
			IsActive = true;
		}
		public void AddEntity(IEntity entity) {
			_chunkEntities.Add(entity);
			if (IsActive) {
				entity.GetGameObject().SetActive(true);
			}
		}
		public void RemoveEntity(IEntity entity) {
			_chunkEntities.Remove(entity);
		}
		public bool IsInBounds(Vector3 position) {
			// Calcular la mitad del tamaño del chunk
			float halfSize = chunkData.size / 2;
			// Calcular los límites del chunk
			float minX = GetCenter().x - halfSize;
			float maxX = GetCenter().x + halfSize;
			float minY = GetCenter().y - halfSize;
			float maxY = GetCenter().y + halfSize;
			float minZ = GetCenter().z - halfSize;
			float maxZ = GetCenter().z + halfSize;
			// Comprobar si el punto está dentro de los límites del chunk
			return (position.x >= minX && position.x <= maxX &&
				position.y >= minY && position.y <= maxY &&
				position.z >= minZ && position.z <= maxZ);
		}
		
		public struct PointData
		{
			public Vector3 position;
			public Vector3 normal;
			public float density;
		}

		public void AddCollider()
		{
			collider = renderer.gameObject.AddComponent<MeshCollider>();
		}

		public void SetMaterial(Material material)
		{
			renderer.material = material;
		}

		public void Release()
		{
			ComputeHelper.Release(pointsBuffer);
		}

		public void DrawBoundsGizmo(Color col)
		{
			Gizmos.color = col;
			Gizmos.DrawWireCube(GetCenter(), Vector3.one * GetSize());
		}
	}
}