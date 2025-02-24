using System.Linq;
using _Project.Scripts.Utils;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

// This class contains some helper functions to make life a little easier working with compute shaders
// (Very work-in-progress!)

namespace _Project.Helper.Compute_Helper {
	public static class ComputeHelper
	{

		public const FilterMode defaultFilterMode = FilterMode.Bilinear;
		public const GraphicsFormat defaultGraphicsFormat = GraphicsFormat.R32G32B32A32_SFloat;

		static ComputeShader normalizeTextureCompute;
		static ComputeShader clearTextureCompute;
		static ComputeShader swizzleTextureCompute;
		static ComputeShader copy3DCompute;
		static ComputeShader transform3DTo2DCompute;
		static ComputeShader getColourFromTexture;
		static ComputeShader densityNormalizer;



		// Subscribe to this event to be notified when buffers created in edit mode should be released
		// (i.e before script compilation occurs, and when exitting edit mode)
		public static event System.Action shouldReleaseEditModeBuffers;

		/// Convenience method for dispatching a compute shader.
		/// It calculates the number of thread groups based on the number of iterations needed.
		public static void Dispatch(ComputeShader cs, int numIterationsX, int numIterationsY = 1, int numIterationsZ = 1, int kernelIndex = 0)
		{
			Vector3Int threadGroupSizes = GetThreadGroupSizes(cs, kernelIndex);
			int numGroupsX = Mathf.CeilToInt(numIterationsX / (float)threadGroupSizes.x);
			int numGroupsY = Mathf.CeilToInt(numIterationsY / (float)threadGroupSizes.y);
			int numGroupsZ = Mathf.CeilToInt(numIterationsZ / (float)threadGroupSizes.y);
			cs.Dispatch(kernelIndex, numGroupsX, numGroupsY, numGroupsZ);
		}

		public static int GetStride<T>()
		{
			return System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));
		}


		public static void CreateStructuredBuffer<T>(ref ComputeBuffer buffer, int count)
		{
			int stride = System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));
			bool createNewBuffer = buffer == null || !buffer.IsValid() || buffer.count != count || buffer.stride != stride;
			if (createNewBuffer)
			{
				Release(buffer);
				buffer = new ComputeBuffer(count, stride);
			}
		}

		public static void CreateStructuredBuffer<T>(ref ComputeBuffer buffer, T[] data)
		{
			CreateStructuredBuffer<T>(ref buffer, data.Length);
			buffer.SetData(data);
		}

		public static ComputeBuffer CreateAndSetBuffer<T>(T[] data, ComputeShader cs, string nameID, int kernelIndex = 0)
		{
			ComputeBuffer buffer = null;
			CreateAndSetBuffer<T>(ref buffer, data, cs, nameID, kernelIndex);
			return buffer;
		}

		public static void CreateAndSetBuffer<T>(ref ComputeBuffer buffer, T[] data, ComputeShader cs, string nameID, int kernelIndex = 0)
		{
			int stride = System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));
			CreateStructuredBuffer<T>(ref buffer, data.Length);
			buffer.SetData(data);
			cs.SetBuffer(kernelIndex, nameID, buffer);
		}

		public static ComputeBuffer CreateAndSetBuffer<T>(int length, ComputeShader cs, string nameID, int kernelIndex = 0)
		{
			ComputeBuffer buffer = null;
			CreateAndSetBuffer<T>(ref buffer, length, cs, nameID, kernelIndex);
			return buffer;
		}

		public static void CreateAndSetBuffer<T>(ref ComputeBuffer buffer, int length, ComputeShader cs, string nameID, int kernelIndex = 0)
		{
			CreateStructuredBuffer<T>(ref buffer, length);
			cs.SetBuffer(kernelIndex, nameID, buffer);
		}

		/// Releases supplied buffer/s if not null
		public static void Release(params ComputeBuffer[] buffers)
		{
			for (int i = 0; i < buffers.Length; i++)
			{
				if (buffers[i] != null)
				{
					buffers[i].Release();
				}
			}
		}

		/// Releases supplied render textures/s if not null
		public static void Release(params RenderTexture[] textures)
		{
			for (int i = 0; i < textures.Length; i++)
			{
				if (textures[i] != null)
				{
					textures[i].Release();
				}
			}
		}

		public static Vector3Int GetThreadGroupSizes(ComputeShader compute, int kernelIndex = 0)
		{
			uint x, y, z;
			compute.GetKernelThreadGroupSizes(kernelIndex, out x, out y, out z);
			return new Vector3Int((int)x, (int)y, (int)z);
		}

		// ------ Texture Helpers ------

		public static void CreateRenderTexture(ref RenderTexture texture, RenderTexture template)
		{
			if (texture != null)
			{
				texture.Release();
			}
			texture = new RenderTexture(template.descriptor);
			texture.enableRandomWrite = true;
			texture.Create();
		}

		public static void CreateRenderTexture(ref RenderTexture texture, int width, int height)
		{
			CreateRenderTexture(ref texture, width, height, defaultFilterMode, defaultGraphicsFormat);
		}


		public static void CreateRenderTexture(ref RenderTexture texture, int width, int height, FilterMode filterMode, GraphicsFormat format, string name = "Unnamed")
		{
			if (texture == null || !texture.IsCreated() || texture.width != width || texture.height != height || texture.graphicsFormat != format)
			{
				if (texture != null)
				{
					texture.Release();
				}
				texture = new RenderTexture(width, height, 0);
				texture.graphicsFormat = format;
				texture.enableRandomWrite = true;

				texture.autoGenerateMips = false;
				texture.Create();
			}
			texture.name = name;
			texture.wrapMode = TextureWrapMode.Clamp;
			texture.filterMode = filterMode;
		}

		public static void CreateRenderTexture3D(ref RenderTexture texture, RenderTexture template)
		{
			CreateRenderTexture(ref texture, template);
		}

		static void CreateRenderTexture3D(ref RenderTexture texture, int width, int height, int depth, string name = "Untitled")
		{
			var format = GraphicsFormat.R16_SFloat;
			if (texture == null || !texture.IsCreated() || texture.width != width || texture.height != height || texture.volumeDepth != depth || texture.graphicsFormat != format)
			{
				//Debug.Log ("Create tex: update noise: " + updateNoise);
				if (texture != null)
				{
					texture.Release();
				}
				const int numBitsInDepthBuffer = 0;
				texture = new RenderTexture(width, height, numBitsInDepthBuffer);
				texture.graphicsFormat = format;
				texture.volumeDepth = depth;
				texture.enableRandomWrite = true;
				texture.dimension = TextureDimension.Tex3D;
				texture.Create();
			}
			texture.wrapMode = TextureWrapMode.Repeat;
			texture.filterMode = FilterMode.Bilinear;
			texture.name = name;
		}

		/// Copy the contents of one render texture into another. Assumes textures are the same size.
		public static void CopyRenderTexture(Texture source, RenderTexture target)
		{
			Graphics.Blit(source, target);
		}

		/// Copy the contents of one render texture into another. Assumes textures are the same size.
		public static void CopyRenderTexture3D(Texture source, RenderTexture target)
		{
			LoadComputeShader(ref copy3DCompute, "Copy3D");
			copy3DCompute.SetInts("dimensions", target.width, target.height, target.volumeDepth);
			copy3DCompute.SetTexture(0, "Source", source);
			copy3DCompute.SetTexture(0, "Target", target);
			Dispatch(copy3DCompute, target.width, target.height, target.volumeDepth);//
		}
		public static void TransformTexture3DTo2D(RenderTexture source, RenderTexture target, float depth) {
			LoadComputeShader(ref transform3DTo2DCompute, "Transform3DTo2D");
			transform3DTo2DCompute.SetInts("dimensions", source.width, source.height, source.volumeDepth);
			transform3DTo2DCompute.SetTexture(0, "Source", source);
			transform3DTo2DCompute.SetTexture(0, "Target", target);
			transform3DTo2DCompute.SetFloat("Depth", depth);
			Dispatch(transform3DTo2DCompute, source.width, source.height, 1);
		}
		public static RenderTexture CombineRender2DTexturesToArray(RenderTexture[] textures) {
			Assert.IsTrue(textures.Length > 0);
			RenderTexture texture2D = textures.First(texture => texture.dimension == TextureDimension.Tex2D);
			int width = texture2D.width;
			int height = texture2D.height;
			int depth = textures.Count(texture => texture.dimension == TextureDimension.Tex2D);

			RenderTexture outTexture = new RenderTexture(width, height, 0) {
				dimension = TextureDimension.Tex2DArray,
				volumeDepth = depth,
				enableRandomWrite = true,
				format = texture2D.format
			};
			outTexture.Create();

			// Copiar texturas individuales al array
			int j = 0;
			for (int i = 0; i < textures.Length; i++) {
				if (textures[i].dimension == TextureDimension.Tex2D) {
					Graphics.CopyTexture(textures[i], 0, 0, outTexture, j, 0);
					j++;
				}
			}
			return outTexture;
		}
		public static void ExtractRender2DTextureToArray(RenderTexture texture, ref RenderTexture[] textures) {
			// Copiar texturas individuales al array
			int j = 0;
			for (int i = 0; i < textures.Length; i++) {
				if (textures[i].dimension == TextureDimension.Tex2D) {
					Graphics.CopyTexture(texture, j, 0, textures[i], 0, 0);
					j++;
				}
			}
		}
		public static float[] GetColourFromTexture(RenderTexture source, int textureSize, float planetSize, Vector3 point) => GetColourFromTexture(source, textureSize, textureSize, planetSize, point);
		public static float[] GetColourFromTexture(RenderTexture source, int width, int height, float planetSize, Vector3 point) {
			float[] result = new float[4];
			if (source is null || source.updateCount == 0) {
				Debug.LogError("Source texture is null");
				return new []{ 0f, 0f };
			}
			RenderTexture emptyTexture3D = null;
			CreateRenderTexture3D(ref emptyTexture3D, 2, 2, 2);
			RenderTexture emptyTexture2D = null;
			CreateRenderTexture(ref emptyTexture2D, 2, 2);
			LoadComputeShader(ref getColourFromTexture, "GetColorFromTexture");
			ComputeBuffer resultBuffer = new ComputeBuffer(1, sizeof(float) * 4);
			if (source.dimension == TextureDimension.Tex3D) {
				getColourFromTexture.SetTexture(0, "_RenderTexture3D", source);
				getColourFromTexture.SetTexture(0, "_RenderTexture2D", emptyTexture2D);
				getColourFromTexture.SetInt("dimension", 3);
			}
			if (source.dimension == TextureDimension.Tex2D) {
				getColourFromTexture.SetTexture(0, "_RenderTexture2D", source);
				getColourFromTexture.SetTexture(0, "_RenderTexture3D", emptyTexture3D);
				getColourFromTexture.SetInt("dimension", 2);
			}
			getColourFromTexture.SetInt( "textureWidth", width);
			getColourFromTexture.SetInt( "textureHeight", height);
			getColourFromTexture.SetFloat("planetSize", planetSize);
			getColourFromTexture.SetBuffer(0, "_Result", resultBuffer);
			getColourFromTexture.SetFloats("worldPos", point.x, point.y, point.z);
			Dispatch(getColourFromTexture, 1);
			resultBuffer.GetData(result);
			Release(emptyTexture2D, emptyTexture3D);
			Release(resultBuffer);
			return result;
		}

		public static void Normalize3DTexture(ref RenderTexture texture, float min, float max) {
			if (texture == null)
				return;
			ComputeBuffer minMaxBuffer = new ComputeBuffer(2, sizeof(uint), ComputeBufferType.Raw);
			LoadComputeShader(ref densityNormalizer, "DensityNormalizer");
			if (densityNormalizer == null)
				return;
			uint[] initValues = new uint[2];
			initValues[0] = MathUtility.FloatToUint(min);
			initValues[1] = MathUtility.FloatToUint(max);
			minMaxBuffer.SetData(initValues);
			densityNormalizer.SetTexture(0, "OutputMap", texture);
			densityNormalizer.SetBuffer(0, "DensityMinMax", minMaxBuffer);
			Dispatch(densityNormalizer, texture.width, texture.height, texture.volumeDepth);
			Release(minMaxBuffer);
		}

		/// Swap channels of texture, or set to zero. For example, if inputs are: (green, red, zero, zero)
		/// then red and green channels will be swapped, and blue and alpha channels will be set to zero.
		public static void SwizzleTexture(Texture texture, Channel x, Channel y, Channel z, Channel w)
		{
			if (swizzleTextureCompute == null)
			{
				swizzleTextureCompute = (ComputeShader)Resources.Load("Swizzle");
			}

			swizzleTextureCompute.SetInt("width", texture.width);
			swizzleTextureCompute.SetInt("height", texture.height);
			swizzleTextureCompute.SetTexture(0, "Source", texture);
			swizzleTextureCompute.SetVector("x", ChannelToMask(x));
			swizzleTextureCompute.SetVector("y", ChannelToMask(y));
			swizzleTextureCompute.SetVector("z", ChannelToMask(z));
			swizzleTextureCompute.SetVector("w", ChannelToMask(w));
			Dispatch(swizzleTextureCompute, texture.width, texture.height, 1, 0);
		}

		/// Sets all pixels of supplied texture to 0
		public static void ClearRenderTexture(RenderTexture source)
		{
			LoadComputeShader(ref clearTextureCompute, "ClearTexture");

			clearTextureCompute.SetInt("width", source.width);
			clearTextureCompute.SetInt("height", source.height);
			clearTextureCompute.SetTexture(0, "Source", source);
			Dispatch(clearTextureCompute, source.width, source.height, 1, 0);
		}

		/// Work in progress, currently only works with one channel and very slow
		public static void NormalizeRenderTexture(RenderTexture source)
		{
			LoadComputeShader(ref normalizeTextureCompute, "NormalizeTexture");

			normalizeTextureCompute.SetInt("width", source.width);
			normalizeTextureCompute.SetInt("height", source.height);
			normalizeTextureCompute.SetTexture(0, "Source", source);
			normalizeTextureCompute.SetTexture(1, "Source", source);

			ComputeBuffer minMaxBuffer = CreateAndSetBuffer<int>(new int[] { int.MaxValue, 0 }, normalizeTextureCompute, "minMaxBuffer", 0);
			normalizeTextureCompute.SetBuffer(1, "minMaxBuffer", minMaxBuffer);

			Dispatch(normalizeTextureCompute, source.width, source.height, 1, 0);
			Dispatch(normalizeTextureCompute, source.width, source.height, 1, 1);

			//int[] data = new int[2];
			//minMaxBuffer.GetData(data);
			//Debug.Log(data[0] + "   " + data[1]);

			Release(minMaxBuffer);
		}

		// https://cmwdexint.com/2017/12/04/computeshader-setfloats/
		public static float[] PackFloats(params float[] values)
		{
			float[] packed = new float[values.Length * 4];
			for (int i = 0; i < values.Length; i++)
			{
				packed[i * 4] = values[i];
			}
			return values;
		}

		// Only run compute shaders if this is true
		// This is only relevant for compute shaders that run outside of playmode
		public static bool CanRunEditModeCompute
		{
			get
			{
				return CheckIfCanRunInEditMode();
			}
		}

		// Set all values from settings object on the shader. Note, variable names must be an exact match in the shader.
		// Settings object can be any class/struct containing vectors/ints/floats/bools
		public static void SetParams(System.Object settings, ComputeShader shader, string variableNamePrefix = "", string variableNameSuffix = "")
		{
			var fields = settings.GetType().GetFields();
			foreach (var field in fields)
			{
				var fieldType = field.FieldType;
				string shaderVariableName = variableNamePrefix + field.Name + variableNameSuffix;

				if (fieldType == typeof(UnityEngine.Vector4) || fieldType == typeof(Vector3) || fieldType == typeof(Vector2))
				{
					shader.SetVector(shaderVariableName, (Vector4)field.GetValue(settings));
				}
				else if (fieldType == typeof(int))
				{
					shader.SetInt(shaderVariableName, (int)field.GetValue(settings));
				}
				else if (fieldType == typeof(float))
				{
					shader.SetFloat(shaderVariableName, (float)field.GetValue(settings));
				}
				else if (fieldType == typeof(bool))
				{
					shader.SetBool(shaderVariableName, (bool)field.GetValue(settings));
				}
				else
				{
					Debug.Log($"Type {fieldType} not implemented");
				}
			}
		}



		static Vector4 ChannelToMask(Channel channel)
		{
			switch (channel)
			{
				case Channel.Red:
					return new Vector4(1, 0, 0, 0);
				case Channel.Green:
					return new Vector4(0, 1, 0, 0);
				case Channel.Blue:
					return new Vector4(0, 0, 1, 0);
				case Channel.Alpha:
					return new Vector4(0, 0, 0, 1);
				case Channel.Zero:
					return new Vector4(0, 0, 0, 0);
				default:
					return Vector4.zero;
			}
		}

		static void LoadComputeShader(ref ComputeShader shader, string name)
		{
			if (shader == null)
			{
				shader = (ComputeShader)Resources.Load(name);
			}
		}


		// Editor helpers:

#if UNITY_EDITOR
		static UnityEditor.PlayModeStateChange playModeState;

		static ComputeHelper()
		{
			// Monitor play mode state
			UnityEditor.EditorApplication.playModeStateChanged -= MonitorPlayModeState;
			UnityEditor.EditorApplication.playModeStateChanged += MonitorPlayModeState;
			// Monitor script compilation
			UnityEditor.Compilation.CompilationPipeline.compilationStarted -= OnCompilationStarted;
			UnityEditor.Compilation.CompilationPipeline.compilationStarted += OnCompilationStarted;
		}

		static void MonitorPlayModeState(UnityEditor.PlayModeStateChange state)
		{
			playModeState = state;
			if (state == UnityEditor.PlayModeStateChange.ExitingEditMode)
			{
				if (shouldReleaseEditModeBuffers != null)
				{
					shouldReleaseEditModeBuffers(); //
				}
			}
		}

		static void OnCompilationStarted(System.Object obj)
		{
			if (shouldReleaseEditModeBuffers != null)
			{
				shouldReleaseEditModeBuffers();
			}
		}
#endif

		static bool CheckIfCanRunInEditMode()
		{
			bool isCompilingOrExitingEditMode = false;
#if UNITY_EDITOR
			isCompilingOrExitingEditMode |= UnityEditor.EditorApplication.isCompiling;
			isCompilingOrExitingEditMode |= playModeState == UnityEditor.PlayModeStateChange.ExitingEditMode;
#endif
			bool canRun = !isCompilingOrExitingEditMode;
			return canRun;
		}
	}
}
