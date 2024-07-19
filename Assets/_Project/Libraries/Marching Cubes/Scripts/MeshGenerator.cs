using System;
using System.Diagnostics;
using _Project.Helper.Compute_Helper;
using _Project.Scripts.Components;
using _Project.Scripts.Handlers;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

namespace _Project.Libraries.Marching_Cubes.Scripts {
	public class MeshGenerator : MonoBehaviour {

		[Header("Init Settings")]
		public int numPointsPerAxis = 10;
		public float boundsSize = 10;
		public float isoLevel = 0f;
		public float testValue = 0f;
		public bool useFlatShading;
		private float renderDistance = 0f;

		public float noiseScale;
		public float noiseHeightMultiplier;
		public bool blurMap;
		public int blurRadius = 3;

		public bool updateOnEditor = false;

		[Header("References")]
		public ComputeShader meshCompute;
		public ComputeShader densityCompute;
		public ComputeShader blurCompute;
		public ComputeShader editCompute;
		public ComputeShader colorFromTextureCompute;
		public Material material;


		// Private
		ComputeBuffer triangleBuffer;
		ComputeBuffer triCountBuffer;
		public RenderTexture rawDensityTexture;
		public RenderTexture processedDensityTexture;
		private Vector3 _reference;
		private Planet _planet;

		VertexData[] vertexDataArray;

		// Stopwatches
		Stopwatch timer_fetchVertexData;
		Stopwatch timer_processVertexData;
		Stopwatch timer_processDensityMap;
		RenderTexture originalMap;
		public RenderTexture originalMap2D;
		public RenderTexture continentalness;
		[SerializeField] private float depth;

		void Start() {
			Debug.Log("Graphics device version: " + SystemInfo.graphicsDeviceVersion);
			Debug.Log("Supports Shader Model 3.0: " + (SystemInfo.graphicsShaderLevel >= 30));
		}
		public void InitMesh(Planet planet) {
			_planet = planet;
			InitTextures();
			CreateBuffers();
			if (Application.isPlaying)
				renderDistance = GameManager.Singleton.gameConfiguration.renderDistance;
		}
		private void OnEnable() {
			ReleaseBuffers();
		}
		public void OnValidate() {
			if (updateOnEditor) {
				material.SetTexture("DensityTex", originalMap);
				//material.SetFloat("oceanRadius", FindObjectOfType<Water>().radius);
				material.SetFloat("planetBoundsSize", boundsSize);
			}
		}

		void InitTextures() {
			// Explanation of texture size:
			// Each pixel maps to one point.
			// Each chunk has "numPointsPerAxis" points along each axis
			// The last points of each chunk overlap in space with the first points of the next chunk
			// Therefore we need one fewer pixel than points for each added chunk
			float resolution = 50f;
			float radius = boundsSize / 2;
			int size = _planet.NumChunks * (numPointsPerAxis - 1) + 1;
			float circumference = 2.0f * Mathf.PI * radius;
			int textureWidth = Mathf.CeilToInt(circumference * testValue);
			int textureHeight = Mathf.CeilToInt(circumference / 2.0f * testValue);
			Debug.Log($"TextureHeight: {textureHeight} TextureWidth: {textureWidth}");
			
			Create3DTexture(ref rawDensityTexture, size, "Raw Density Texture");
			Create3DTexture(ref processedDensityTexture, size, "Processed Density Texture");
			Create2DTexture(ref originalMap2D, size, "Processed 2D Density Texture");
			Create2DTexture(ref continentalness, textureHeight, textureWidth, "Continentalness Values");
			//Create2DTexture(ref continentalness, size, "Continentalness Values");

			if (!blurMap) {
				processedDensityTexture = rawDensityTexture;
			}

			// Set textures on compute shaders
			densityCompute.SetTexture(0, "DensityTexture", rawDensityTexture);
			densityCompute.SetTexture(0, "ContinentalnessTexture", continentalness);
			editCompute.SetTexture(0, "EditTexture", rawDensityTexture);
			blurCompute.SetTexture(0, "Source", rawDensityTexture);
			blurCompute.SetTexture(0, "Result", processedDensityTexture);
			meshCompute.SetTexture(0, "DensityTexture", (blurCompute) ? processedDensityTexture : rawDensityTexture);
		}

		public void CreateRenderTextures() {
			ComputeHelper.CreateRenderTexture3D(ref originalMap, processedDensityTexture);
			ComputeHelper.CopyRenderTexture3D(processedDensityTexture, originalMap);
			ComputeHelper.TransformTexture3DTo2D(originalMap, originalMap2D, depth % originalMap.volumeDepth);
			material.EnableKeyword("_MAIN_LIGHT_SHADOWS");
		}

		public void ComputeDensity() {
			// Get points (each point is a vector4: xyz = position, w = density)
			if (!ComputeHelper.CanRunEditModeCompute) {
				Debug.LogError("Compute Buffer could'nt run in editmode");
				return;
			}
			int textureSize = rawDensityTexture.width;
			timer_processDensityMap = new Stopwatch();
			timer_processDensityMap.Start();
			
			//timer_processDensityMap.Start();
			densityCompute.SetInt("densityTextureSize", textureSize);
			densityCompute.SetInt("sphereTextureHeight", continentalness.height);
			densityCompute.SetInt("sphereTextureWidth", continentalness.width);

			densityCompute.SetFloat("planetSize", boundsSize);
			densityCompute.SetFloat("testValue", testValue);
			densityCompute.SetFloat("noiseHeightMultiplier", noiseHeightMultiplier);
			densityCompute.SetFloat("noiseScale", noiseScale);

			ComputeHelper.Dispatch(densityCompute, textureSize, textureSize, textureSize);

			ProcessDensityMap();
		}

		void ProcessDensityMap() {
			//timer_processDensityMap.Start();
			if (blurMap) {
				if (!ComputeHelper.CanRunEditModeCompute) {
					Debug.LogError("Compute Buffer could'nt run in editmode");
					return;
				}
				int size = rawDensityTexture.width;
				blurCompute.SetInts("brushCentre", 0, 0, 0);
				blurCompute.SetInt("blurRadius", blurRadius);
				blurCompute.SetInt("textureSize", rawDensityTexture.width);
				ComputeHelper.Dispatch(blurCompute, size, size, size);
			}
			timer_processDensityMap.Stop();
			Debug.Log($"Tiempo generación de densidad (ms): {timer_processDensityMap.ElapsedMilliseconds}");
		}

		public void GenerateChunk(Chunk chunk) {
			// Create timers:
			timer_fetchVertexData = new Stopwatch();
			timer_processVertexData = new Stopwatch();
			if (!ComputeHelper.CanRunEditModeCompute) {
				Debug.LogError("Compute Buffer could'nt run in editmode");
				return;
			}
			// Marching cubes
			int numVoxelsPerAxis = chunk.numPointsPerAxis - 1;
			int marchKernel = 0;
			
			meshCompute.SetInt("densityTextureSize", processedDensityTexture.width);
			meshCompute.SetInt("sphereTextureHeight", continentalness.height);
			meshCompute.SetInt("sphereTextureWidth", continentalness.width);
			meshCompute.SetInt("numPointsPerAxis", numPointsPerAxis);
			meshCompute.SetFloat("isoLevel", isoLevel);
			meshCompute.SetFloat("planetSize", boundsSize);
			meshCompute.SetTexture(0, "ContinentalnessTexture", continentalness);
			triangleBuffer.SetCounterValue(0);
			meshCompute.SetBuffer(marchKernel, "triangles", triangleBuffer);

			Vector3 chunkCoord = (Vector3) chunk.GetCoords() * (numPointsPerAxis - 1);
			meshCompute.SetVector("chunkCoord", chunkCoord);
			meshCompute.SetVector("planetCenter", _planet.Center);

			ComputeHelper.Dispatch(meshCompute, numVoxelsPerAxis, numVoxelsPerAxis, numVoxelsPerAxis, marchKernel);

			// Create mesh
			int[] vertexCountData = new int[1];
			triCountBuffer.SetData(vertexCountData);
			ComputeBuffer.CopyCount(triangleBuffer, triCountBuffer, 0);

			//timer_fetchVertexData.Start();
			triCountBuffer.GetData(vertexCountData);

			int numVertices = vertexCountData[0] * 3;

			// Fetch vertex data from GPU

			triangleBuffer.GetData(vertexDataArray, 0, 0, numVertices);

			//timer_fetchVertexData.Stop();
			Debug.Log($"Tiempo creación de vértices(ms): {timer_fetchVertexData.ElapsedMilliseconds}");

			//CreateMesh(vertices);
			timer_processVertexData.Start();
			chunk.CreateMesh(vertexDataArray, numVertices, useFlatShading);
			timer_processVertexData.Stop();
			Debug.Log($"Tiempo creación de malla(ms): {timer_processVertexData.ElapsedMilliseconds}");
		}

		void Update() {

			// TODO: move somewhere more sensible
			material.SetTexture("DensityTex", originalMap);
			//material.SetFloat("oceanRadius", FindObjectOfType<Water>().radius);
			material.SetFloat("planetBoundsSize", boundsSize);
			/*
		if (Input.GetKeyDown(KeyCode.G))
		{
			Debug.Log("Generate");
			GenerateAllChunks();
		}
		*/
		}

		void CreateBuffers() {
			int numPoints = numPointsPerAxis * numPointsPerAxis * numPointsPerAxis;
			int numVoxelsPerAxis = numPointsPerAxis - 1;
			int numVoxels = numVoxelsPerAxis * numVoxelsPerAxis * numVoxelsPerAxis;
			int maxTriangleCount = numVoxels * 5;
			int maxVertexCount = maxTriangleCount * 3;
			ReleaseBuffers();
			triCountBuffer = new ComputeBuffer(1, sizeof (int), ComputeBufferType.Raw);
			triangleBuffer = new ComputeBuffer(maxVertexCount, ComputeHelper.GetStride<VertexData>(), ComputeBufferType.Append);
			vertexDataArray = new VertexData[maxVertexCount];
		}

		public void ReleaseBuffers() {
			if (triangleBuffer is not null && triCountBuffer is not null && triangleBuffer.IsValid() && triCountBuffer.IsValid())
				ComputeHelper.Release(triangleBuffer, triCountBuffer);
		}
		
		public float GetDensityAtPoint(Vector3 point) {
			float[] result = ComputeHelper.GetColourFromTexture(originalMap, rawDensityTexture.width, boundsSize, point);
			return result[0];
		}
		public float GetContinentalnessAtPoint(Vector3 point) {
			float[] result = ComputeHelper.GetColourFromTexture(continentalness, continentalness.width, continentalness.height, boundsSize, point);
			return result[0];
		}
		
		void OnDestroy() {
			updateOnEditor = false;
		}

		public void Terraform(Vector3 point, float weight, float radius) {

			int editTextureSize = rawDensityTexture.width;
			float editPixelWorldSize = boundsSize / editTextureSize;
			int editRadius = Mathf.CeilToInt(radius / editPixelWorldSize);
			//Debug.Log(editPixelWorldSize + "  " + editRadius);

			float tx = Mathf.Clamp01((point.x + boundsSize / 2) / boundsSize);
			float ty = Mathf.Clamp01((point.y + boundsSize / 2) / boundsSize);
			float tz = Mathf.Clamp01((point.z + boundsSize / 2) / boundsSize);

			int editX = Mathf.RoundToInt(tx * (editTextureSize - 1));
			int editY = Mathf.RoundToInt(ty * (editTextureSize - 1));
			int editZ = Mathf.RoundToInt(tz * (editTextureSize - 1));

			editCompute.SetFloat("weight", weight);
			editCompute.SetFloat("deltaTime", Time.deltaTime);
			editCompute.SetInts("brushCentre", editX, editY, editZ);
			editCompute.SetInt("brushRadius", editRadius);

			editCompute.SetInt("size", editTextureSize);
			ComputeHelper.Dispatch(editCompute, editTextureSize, editTextureSize, editTextureSize);

			//ProcessDensityMap();
			int size = rawDensityTexture.width;

			if (blurMap) {
				blurCompute.SetInt("textureSize", rawDensityTexture.width);
				blurCompute.SetInts("brushCentre", editX - blurRadius - editRadius, editY - blurRadius - editRadius, editZ - blurRadius - editRadius);
				blurCompute.SetInt("blurRadius", blurRadius);
				blurCompute.SetInt("brushRadius", editRadius);
				int k = (editRadius + blurRadius) * 2;
				ComputeHelper.Dispatch(blurCompute, k, k, k);
			}

			//ComputeHelper.CopyRenderTexture3D(originalMap, processedDensityTexture);
/*
			float worldRadius = (editRadius + 1 + ((blurMap) ? blurRadius : 0)) * editPixelWorldSize;
			for (int i = 0; i < chunks.Length; i++) {
				Chunk chunk = chunks[i];
				if (MathUtility.SphereIntersectsBox(point, worldRadius, chunk.GetCenter(), Vector3.one * chunk.GetSize())) {

					chunk.terra = true;
					GenerateChunk(chunk);

				}
			}*/
		}

		void Create3DTexture(ref RenderTexture texture, int size, string name) {
			//
			var format = GraphicsFormat.R32_SFloat;
			if (texture == null || !texture.IsCreated() || texture.width != size || texture.height != size || texture.volumeDepth != size || texture.graphicsFormat != format) {
				//Debug.Log ("Create tex: update noise: " + updateNoise);
				if (texture != null) {
					texture.Release();
				}
				const int numBitsInDepthBuffer = 0;
				texture = new RenderTexture(size, size, numBitsInDepthBuffer);
				texture.graphicsFormat = format;
				texture.volumeDepth = size;
				texture.enableRandomWrite = true;
				texture.dimension = TextureDimension.Tex3D;


				texture.Create();
			}
			texture.wrapMode = TextureWrapMode.Repeat;
			texture.filterMode = FilterMode.Bilinear;
			texture.name = name;
		}
		void Create2DTexture(ref RenderTexture texture, int size, string name) {
			Create2DTexture(ref texture, size, size, name);
		}
		void Create2DTexture(ref RenderTexture texture, int height, int width, string name) {
			var format = GraphicsFormat.R32_SFloat;
			if (texture == null || !texture.IsCreated() || texture.width != width || texture.height != height || texture.volumeDepth != 0 || texture.graphicsFormat != format) {
				//Debug.Log ("Create tex: update noise: " + updateNoise);
				if (texture != null) {
					texture.Release();
				}
				const int numBitsInDepthBuffer = 0;
				texture = new RenderTexture(width, height, numBitsInDepthBuffer);
				texture.graphicsFormat = format;
				texture.volumeDepth = 0;
				texture.enableRandomWrite = true;
				texture.dimension = TextureDimension.Tex2D;


				texture.Create();
			}
			texture.wrapMode = TextureWrapMode.Repeat;
			texture.filterMode = FilterMode.Bilinear;
			texture.name = name;
		}
	}
}