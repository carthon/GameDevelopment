using System;
using System.Collections.Generic;
using System.Diagnostics;
using _Project.Helper.Compute_Helper;
using _Project.Scripts.Components;
using _Project.Scripts.Constants;
using _Project.Scripts.DataClasses;
using _Project.Scripts.Handlers;
using _Project.Scripts.Utils;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
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

		public NoiseData NoiseData;
		public float noiseHeightMultiplier;
		public bool blurMap;
		public int blurRadius = 3;

		public bool updateOnEditor = false;

		[Header("References")]
		public ComputeShader meshCompute;
		public ComputeShader densityCompute;
		public ComputeShader blurCompute;
		public ComputeShader editCompute;
		public Material material;


		// Private
		ComputeBuffer meshTriangleBuffer;
		ComputeBuffer waterTriangleBuffer;
		ComputeBuffer triangleCountBuffer;
		ComputeBuffer waterTriangleCountBuffer;
		ComputeBuffer noiseDataBuffer;
		ComputeBuffer densityMinMaxBuffer;
		private Vector3 _reference;
		private Planet _planet;

		VertexData[] vertexDataArray;

		// Stopwatches
		Stopwatch timer_fetchVertexData;
		Stopwatch timer_processVertexData;
		Stopwatch timer_processDensityMap;
		[SerializeField] RenderTexture densityMap;
		public RenderTexture originalMap2D;
		[FormerlySerializedAs("continentalness")] public RenderTexture debugHitTexture;
		public RenderTexture[] noiseTextures;
		[SerializeField] private float depthToSlice;

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
				UpdateMaterialData();
				CreateRenderTextures();
			}
		}

		void InitTextures() {
			// Explanation of texture size:
			// Each pixel maps to one point.
			// Each chunk has "numPointsPerAxis" points along each axis
			// The last points of each chunk overlap in space with the first points of the next chunk
			// Therefore we need one fewer pixel than points for each added chunk
			float resolution = 1f;
			float radius = boundsSize / 2;
			int size = _planet.NumChunks * (numPointsPerAxis - 1) + 1;
			float circumference = 2.0f * Mathf.PI * radius;
			int textureWidth = Mathf.CeilToInt(circumference * resolution);
			int textureHeight = Mathf.CeilToInt(circumference / 2.0f * resolution);
			
			Create2DTexture(ref originalMap2D, size, "Processed 2D Density Texture");
			Create2DTexture(ref debugHitTexture, textureHeight, textureWidth, 0, "Debug Value");
			Create3DTexture(ref densityMap, size, "Density Values");
			noiseTextures = new RenderTexture[NoiseData.noiseParams.Count];
			for (int i = 0; i < NoiseData.noiseParams.Count; i++) {
				NoiseParams noiseParams = NoiseData.noiseParams[i];
				if (noiseParams.noiseType == DensityEnum.HEIGHTMAP_NOISE)
					Create2DTexture(ref noiseTextures[i], textureHeight, textureWidth, 0, noiseParams.noiseName);
				else
					Create3DTexture(ref noiseTextures[i], size, noiseParams.noiseName);
			}

			// Set textures on compute shaders
			//densityCompute.SetTexture(0, "DensityTexture", rawDensityTexture);
			//editCompute.SetTexture(0, "EditTexture", rawDensityTexture);
			//blurCompute.SetTexture(0, "Source", rawDensityTexture);
			//blurCompute.SetTexture(0, "Result", processedDensityTexture);
			//meshCompute.SetTexture(0, "DensityTexture", (blurCompute) ? processedDensityTexture : rawDensityTexture);
		}

		public void CreateRenderTextures() {
			//ComputeHelper.CreateRenderTexture3D(ref densityMap, processedDensityTexture);
			//ComputeHelper.CopyRenderTexture3D(processedDensityTexture, densityMap);
			ComputeHelper.TransformTexture3DTo2D(densityMap, originalMap2D, depthToSlice % densityMap.volumeDepth);
		}

		public void ComputeDensity(Vector3 point) {
			// Get points (each point is a vector4: xyz = position, w = density)
			if (!ComputeHelper.CanRunEditModeCompute) {
				Debug.LogError("Compute Buffer could'nt run in editmode");
				return;
			}
			timer_processDensityMap = new Stopwatch();
			timer_processDensityMap.Start();
			
			densityCompute.SetInt("densityTextureSize", densityMap.width);
			
			uint[] initValues = new uint[2];
			initValues[0] = MathUtility.FloatToUint(3.4e+38f); // min lo inicializamos a +∞
			initValues[1] = MathUtility.FloatToUint(0); // max lo inicializamos a 0
			densityMinMaxBuffer.SetData(initValues);
			densityCompute.SetBuffer(0, "DensityMinMax", densityMinMaxBuffer);
			
			densityCompute.SetInt("sphereTextureHeight", debugHitTexture.height);
			densityCompute.SetInt("sphereTextureWidth", debugHitTexture.width);

			densityCompute.SetFloat("planetSize", boundsSize);
			densityCompute.SetVector("planetCenter", _planet.Center);
			densityCompute.SetFloat("isoLevel", isoLevel);
			densityCompute.SetFloat("testValue", testValue);
			densityCompute.SetFloat("noiseHeightMultiplier", noiseHeightMultiplier);

			List<NoiseParams> noiseParams = NoiseData.noiseParams;
			List<GPUNoiseParams> gpuNoiseParams = new List<GPUNoiseParams>();
			foreach (NoiseParams noiseParam in noiseParams) {
				GPUNoiseParams gpuNoiseParam = new GPUNoiseParams() {
					lacunarity = noiseParam.lacunarity,
					noiseScale = noiseParam.noiseScale,
					noiseType = noiseParam.noiseType,
					numLayers = noiseParam.numLayers,
					persistence = noiseParam.persistence,
				};
				gpuNoiseParams.Add(gpuNoiseParam);
			}
			noiseDataBuffer.SetData(gpuNoiseParams.ToArray());
			densityCompute.SetBuffer(0, "NoiseParamsBuffer", noiseDataBuffer);
			
			RenderTexture tempTexture = ComputeHelper.CombineRender2DTexturesToArray(noiseTextures);
			densityCompute.SetTexture(0, "NoiseTextures", tempTexture);
			densityCompute.SetTexture(0, "DebugHitsTexture", debugHitTexture);
			densityCompute.SetTexture(0, "OutputMap", densityMap);
			Debug.Log($"Generating map for 3D Texture: {densityMap.width} {densityMap.height} {densityMap.depth}");
			ComputeHelper.Dispatch(densityCompute, densityMap.width, densityMap.height, densityMap.volumeDepth);
			ComputeHelper.ExtractRender2DTextureToArray(tempTexture, ref noiseTextures);
			
			uint[] minMax = new uint[2];
			densityMinMaxBuffer.GetData(minMax);
			float minVal = MathUtility.UintToFloat(minMax[0]);
			float maxVal = MathUtility.UintToFloat(minMax[1]);
			Debug.Log($"MaxDensity: {maxVal} MinDensity: {minVal}");
			
			//ComputeHelper.Normalize3DTexture(ref densityMap, minVal, maxVal);
			UpdateMaterialData();
			//ProcessDensityMap();
		}

		void ProcessDensityMap() {
			//timer_processDensityMap.Start();
			if (blurMap) {
				if (!ComputeHelper.CanRunEditModeCompute) {
					Debug.LogError("Compute Buffer could'nt run in editmode");
					return;
				}
				//int size = rawDensityTexture.width;
				//blurCompute.SetInts("brushCentre", 0, 0, 0);
				//blurCompute.SetInt("blurRadius", blurRadius);
				//blurCompute.SetInt("textureSize", rawDensityTexture.width);
				//ComputeHelper.Dispatch(blurCompute, size, size, size);
			}
			timer_processDensityMap.Stop();
			Debug.Log($"Tiempo generación de densidad (ms): {timer_processDensityMap.ElapsedMilliseconds}");
		}

		public void UpdateMaterialData() {
			material.SetTexture("DensityTex", densityMap);
			//material.SetFloat("oceanRadius", FindObjectOfType<Water>().radius);
			material.SetFloat("planetBoundsSize", boundsSize);
			if (_planet is null) {
				TryGetComponent(out _planet);
			}
			material.SetVector("planetCenter", _planet.Center);
			material.EnableKeyword("_MAIN_LIGHT_SHADOWS");
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
			RenderTexture texture = noiseTextures[1];
			if (texture is null) return;
			//Revisar el cálculo de texture.height y width
			meshCompute.SetInt("densityTextureSize", densityMap.width);
			meshCompute.SetInt("sphereTextureHeight", texture.height);
			meshCompute.SetInt("sphereTextureWidth", texture.width);
			meshCompute.SetInt("numPointsPerAxis", numPointsPerAxis);
			meshCompute.SetFloat("isoLevel", isoLevel);
			meshCompute.SetFloat("planetSize", boundsSize);
			meshCompute.SetTexture(0, "DensityTexture", densityMap);
			meshTriangleBuffer.SetCounterValue(0);
			meshCompute.SetBuffer(marchKernel, "triangles", meshTriangleBuffer);

			Vector3 chunkCoord = (Vector3) chunk.GetCoords() * (numPointsPerAxis - 1);
			meshCompute.SetVector("chunkCoord", chunkCoord);
			meshCompute.SetVector("planetCenter", _planet.Center);
			
			ComputeHelper.Dispatch(meshCompute, numVoxelsPerAxis, numVoxelsPerAxis, numVoxelsPerAxis, marchKernel);

			// Create mesh
			int[] vertexCountData = new int[1];
			triangleCountBuffer.SetData(vertexCountData);
			ComputeBuffer.CopyCount(meshTriangleBuffer, triangleCountBuffer, 0);

			//timer_fetchVertexData.Start();
			triangleCountBuffer.GetData(vertexCountData);

			int numVertices = vertexCountData[0] * 3;

			// Fetch vertex data from GPU

			meshTriangleBuffer.GetData(vertexDataArray, 0, 0, numVertices);

			//timer_fetchVertexData.Stop();
			Debug.Log($"Tiempo creación de vértices(ms): {timer_fetchVertexData.ElapsedMilliseconds}");

			//CreateMesh(vertices);
			timer_processVertexData.Start();
			chunk.CreateMesh(vertexDataArray, numVertices, useFlatShading);
			timer_processVertexData.Stop();
			Debug.Log($"Tiempo creación de malla(ms): {timer_processVertexData.ElapsedMilliseconds}");
		}

		void Update() {
			CreateRenderTextures();
		}

		void CreateBuffers() {
			int numPoints = numPointsPerAxis * numPointsPerAxis * numPointsPerAxis;
			int numVoxelsPerAxis = numPointsPerAxis - 1;
			int numVoxels = numVoxelsPerAxis * numVoxelsPerAxis * numVoxelsPerAxis;
			int maxTriangleCount = numVoxels * 5;
			int maxVertexCount = maxTriangleCount * 3;
			ReleaseBuffers();
			triangleCountBuffer = new ComputeBuffer(1, sizeof (int), ComputeBufferType.Raw);
			meshTriangleBuffer = new ComputeBuffer(maxVertexCount, ComputeHelper.GetStride<VertexData>(), ComputeBufferType.Append);
			noiseDataBuffer = new ComputeBuffer(NoiseData.noiseParams.Count, ComputeHelper.GetStride<GPUNoiseParams>(), ComputeBufferType.Structured);
			densityMinMaxBuffer = new ComputeBuffer(2, sizeof(uint), ComputeBufferType.Raw);
			vertexDataArray = new VertexData[maxVertexCount];
		}

		public void ReleaseBuffers() {
			if (meshTriangleBuffer is not null && triangleCountBuffer is not null && noiseDataBuffer is not null &&
			    meshTriangleBuffer.IsValid() && triangleCountBuffer.IsValid() && noiseDataBuffer.IsValid())
				ComputeHelper.Release(meshTriangleBuffer, triangleCountBuffer, noiseDataBuffer);
		}
		
		public float GetDensityAtPoint(Vector3 point) {
			float[] result = ComputeHelper.GetColourFromTexture(densityMap, densityMap.width, densityMap.height, boundsSize, point);
			return result[0];
		}
		public float GetHeightMapValuesAtPoint(Vector3 point) {
			Assert.IsTrue(noiseTextures.Length > 0);
			int textureId = 0;
			foreach (NoiseParams dataNoiseParam in NoiseData.noiseParams) {
				if (dataNoiseParam.noiseType == DensityEnum.HEIGHTMAP_NOISE)
					break;
				textureId++;
			}
			float[] result = ComputeHelper.GetColourFromTexture(noiseTextures[textureId], noiseTextures[textureId].width, noiseTextures[textureId].height, boundsSize, point);
			return result[0];
		}
		
		void OnDestroy() {
			updateOnEditor = false;
		}
		public void Terraform(Vector3 point, float weight, float radius) {
			/*
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

			//ComputeHelper.CopyRenderTexture3D(densityMap, processedDensityTexture);
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

		void Create3DTexture(ref RenderTexture texture, int size, string name = "Empty") {
			Create3DTexture(ref texture, size, size, size, name);
		}
		void Create3DTexture(ref RenderTexture texture, int width, int height, int depth, string name = "Empty") {
			//
			var format = GraphicsFormat.R32_SFloat;
			if (texture == null || !texture.IsCreated() || texture.width != width || texture.height != height || texture.volumeDepth != depth || texture.graphicsFormat != format) {
				//Debug.Log ("Create tex: update noise: " + updateNoise);
				if (texture != null) {
					texture.Release();
				}
				const int numBitsInDepthBuffer = 0;
				texture = new RenderTexture(width, height, numBitsInDepthBuffer);
				texture.graphicsFormat = format;
				texture.volumeDepth = depth;
				texture.enableRandomWrite = true;
				texture.dimension = TextureDimension.Tex3D;
				
				Debug.Log($"TextureHeight: {height} TextureWidth: {width} TextureDepth: {depth}");
				texture.Create();
			}
			texture.wrapMode = TextureWrapMode.Repeat;
			texture.filterMode = FilterMode.Bilinear;
			texture.name = name;
		}
		void Create2DTexture(ref RenderTexture texture, int size, string name = "Empty") {
			Create2DTexture(ref texture, size, size, 0, name);
		}
		void Create2DTexture(ref RenderTexture texture, int height, int width, int depth = 0, string name = "Empty") {
			var format = GraphicsFormat.R32_SFloat;
			if (texture == null || !texture.IsCreated() || texture.width != width || texture.height != height || texture.volumeDepth != 0 || texture.graphicsFormat != format) {
				//Debug.Log ("Create tex: update noise: " + updateNoise);
				if (texture != null) {
					texture.Release();
				}
				const int numBitsInDepthBuffer = 0;
				texture = new RenderTexture(width, height, numBitsInDepthBuffer);
				texture.graphicsFormat = format;
				texture.volumeDepth = depth;
				texture.enableRandomWrite = true;
				texture.dimension = TextureDimension.Tex2D;


				texture.Create();
				Debug.Log($"TextureHeight: {height} TextureWidth: {width} TextureDepth: {depth}");
			}
			texture.wrapMode = TextureWrapMode.Repeat;
			texture.filterMode = FilterMode.Bilinear;
			texture.name = name;
		}
	}
}