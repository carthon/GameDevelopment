Shader "Custom/Terrain"
{
    Properties
    {
        DensityTex ("DensityTex", 3D) = "white" {}
        _MainTex ("MainTex", 2D) = "white" {}
		_RockInnerShallow ("Rock Inner Shallow", Color) = (1,1,1,1)
		_RockInnerDeep ("Rock Inner Deep", Color) = (1,1,1,1)
		_RockLight ("Rock Light", Color) = (1,1,1,1)
		_RockLightTex ("Rock Light Texture", 2D) = "White" {}
		_RockDark ("Rock Dark", Color) = (1,1,1,1)
		_RockDarkTex ("Rock Dark Texture", 2D) = "White" {}
		_GrassLight ("Grass Light", Color) = (1,1,1,1)
		_GrassLightTex ("Grass Light Texture", 2D) = "White" {}
		_GrassDark ("Grass Dark", Color) = (1,1,1,1)
		_GrassDarkTex ("Grass Dark Texture", 2D) = "White" {}


		_Color ("Color", Color) = (1,1,1,1)
		_Specular ("Specular", Color) = (1,1,1,1)
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Test ("Test", Float) = 0.0
		_TextureScale ("TextureScale", Float) = 0.0
		_RockThreshold ("Rock Threshold", Float) = 0.0

		_NoiseTex("Noise Texture", 2D) = "White" {}
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
        	Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            
            #define _SPECULAR_COLOR
            #if UNITY_VERSION >= 202120
				#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #else
				#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
				#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #endif
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            
            #pragma vertex Vertex
            #pragma fragment Fragment
            // make fog work
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
		    TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
		    TEXTURE2D(_NoiseTex); SAMPLER(sampler_NoiseTex);
		    TEXTURE2D(_RockDarkTex); SAMPLER(sampler_RockDarkTex);
		    TEXTURE2D(_GrassLightTex); SAMPLER(sampler_GrassLightTex);
		    TEXTURE2D(_GrassDarkTex); SAMPLER(sampler_GrassDarkTex);
		    TEXTURE3D(DensityTex); SAMPLER(sampler_DensityTex);

            float4 _ColorMap_ST; // Automatically set by unity used in transform_tex
            float4 _NoiseTex_ST;
            float4 DensityTex_ST;
            float4 _MainTex_ST;
            float4 _RockDarkTex_ST; 
			float4 _GrassLightTex_ST;
			float4 _GrassDarkTex_ST;
            float planetBoundsSize;
            float planetCenter;
            
			half _Glossiness;
			half _Specular;
			float4 _Color;
			float4 _RockInnerShallow;
			float4 _RockInnerDeep;
			float4 _RockLight;
			float4 _RockDark;
			float4 _GrassLight;
			float4 _GrassDark;
			float _Test;
			float _TextureScale;
			float _RockThreshold;

            struct Attributes
            {
                float4 positionOS : POSITION; //Position in obj space
            	float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0; //Material texture uvs
            };

            //WS - WorldSpace | VS - ViewSpace | CS - Homogeneous ClipSpace position | NDC - Homogeneous normalized device coords
            struct Interpolators
            {
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
            	float4 positionCS : SV_POSITION;
            	float3 normalOS : NORMAL;
            };
            
            float3 _LightDirection;

            float4 GetShadowCasterPositionCS(float3 positionWS, float3 normalWS)
            {
	            float3 lightDirectionWS = _LightDirection;
            	float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
            	#if UNITY_REVERSED_Z
            	positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
            	#else
            	positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE)
            	#endif
            	return positionCS;
            }
            
			float4 triplanarOffset(float3 vertPos, float3 normal, float3 scale, Texture2D _texture, SamplerState _sampler, float2 offset) {
				float3 scaledPos = vertPos / scale;
				float4 colX = SAMPLE_TEXTURE2D (_texture, _sampler, scaledPos.zy + offset);
				float4 colY = SAMPLE_TEXTURE2D (_texture, _sampler, scaledPos.xz + offset);
				float4 colZ = SAMPLE_TEXTURE2D (_texture, _sampler, scaledPos.xy + offset);
				
				// Square normal to make all values positive + increase blend sharpness
				float3 blendWeight = normal * normal;
				// Divide blend weight by the sum of its components. This will make x + y + z = 1
				blendWeight /= dot(blendWeight, 1);
				return colX * blendWeight.x + colY * blendWeight.y + colZ * blendWeight.z;
			}
	            
			float3 worldToTexPos(float3 worldPos) {
            	float3 local = worldPos - planetCenter;
				return (local / planetBoundsSize / 2) + 0.5;
			}
            
            Interpolators Vertex (Attributes v) //Transform normal to world space
            {
                Interpolators output;
				VertexPositionInputs posnInputs = GetVertexPositionInputs(v.positionOS);
				VertexNormalInputs normInputs = GetVertexNormalInputs(v.normalOS);
            	output.normalWS = normInputs.normalWS;
            	output.normalOS = v.normalOS;
            	output.positionCS = GetShadowCasterPositionCS(posnInputs.positionWS, normInputs.normalWS);
            	output.positionWS = posnInputs.positionWS;
                //output.uv = TRANSFORM_TEX(v.uv, _GrassLightTex);
                output.uv = v.uv;
                return output;
            }

            float4 Fragment (Interpolators i) : SV_Target
            {
                // sample the texture
				float3 t = saturate( worldToTexPos(i.positionWS) );
				float density = SAMPLE_TEXTURE3D(DensityTex, sampler_DensityTex, t);
				//0 = flat, 0.5 = vertical, 1 = flat (but upside down)
				float steepness = 1 - (dot(normalize(i.positionWS), i.normalOS) * 0.5 + 0.5);
				float dstFromCentre = length(i.positionWS - planetCenter);
            	//density = (density + 1) / 2;
				// Normalización:
				density = (density - 290) / (350 - 290);
			    // Escala a [0..1] => (densidad + 1)/2
            	//return float4(density, density, density, 1.0f);
				float4 noise = triplanarOffset(i.positionWS, i.normalOS, 30, _NoiseTex, sampler_NoiseTex, 0);
				float4 noise2 = triplanarOffset(i.positionWS, i.normalOS, 50, _NoiseTex, sampler_NoiseTex, 0);
				

				float metallic = 0;
				float rockMetalStrength = 0.4;
            	
                float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
            	InputData lightingInput = (InputData)0;
            	lightingInput.positionWS = i.positionWS;
            	lightingInput.normalWS = normalize(i.normalWS);
            	lightingInput.viewDirectionWS = GetWorldSpaceNormalizeViewDir(i.positionWS);
				lightingInput.shadowCoord = TransformWorldToShadowCoord(i.positionWS);
            	
            	SurfaceData surfaceInput = (SurfaceData)0;
            	//surfaceInput.albedo = col.rbg * _Color.rgb;
            	surfaceInput.alpha = col.a * _Color.a;
            	surfaceInput.smoothness = _Glossiness;
            	surfaceInput.specular = _Specular;
            	
				float threshold = 0.005;
				if (density >= -_RockThreshold) {
					
					float rockDepthT = saturate(abs(density + _RockThreshold) * 20);
					/*
					surfaceInput.albedo = lerp(_RockInnerShallow, _RockInnerDeep, rockDepthT);
					metallic = lerp(rockMetalStrength, 1, rockDepthT);
					*/
					float4 rockTextureColor = triplanarOffset(i.positionWS, i.normalOS, _TextureScale, _RockDarkTex, sampler_RockDarkTex, 0);
					//surfaceInput.albedo = lerp(rockTextureColor, _RockInnerDeep, rockDepthT);
					surfaceInput.albedo = rockTextureColor * _RockDark;
				}
				else {
			        float4 grassTextureColor = triplanarOffset(i.positionWS, i.normalOS, _TextureScale, _GrassLightTex, sampler_GrassLightTex, 0);
			        float4 grassRockColor = triplanarOffset(i.positionWS, i.normalOS, _TextureScale, _GrassDarkTex, sampler_GrassDarkTex, 0);
					int r = 10;
					float4 rockCol = lerp(grassRockColor, grassTextureColor, (int)(noise2.r*r) / float(r));
					float n = (noise.r-0.4) * _Test;

					float rockWeight = smoothstep(0.24 + n, 0.24 + 0.001 + n, steepness);
					surfaceInput.albedo = lerp(grassTextureColor, rockCol, rockWeight);
					//o.Albedo = steepness > _Test;
					metallic = lerp(0, rockMetalStrength, rockWeight);
				}
                return UniversalFragmentBlinnPhong(lightingInput, surfaceInput);
            }
            ENDHLSL
        }
    
		Pass
		{
			Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            
            ColorMask 0
            
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            struct Attributes {
	            float3 positionOS : POSITION;
            	float3 normalOS : NORMAL;
            };

            struct Interpolators {
	            float4 positionCS : SV_POSITION;
            };

            float3 _LightDirection;
            
            float4 GetShadowCasterPositionCS(float3 positionWS, float3 normalWS)
            {
	            float3 lightDirectionWS = _LightDirection;
            	float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
            	#if UNITY_REVERSED_Z
            	positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
            	#else
            	positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
            	#endif
            	return positionCS;
            }
            
            Interpolators Vertex(Attributes input)
            {
	            Interpolators output;

            	VertexPositionInputs posnInputs = GetVertexPositionInputs(input.positionOS);
            	VertexNormalInputs normInputs = GetVertexNormalInputs(input.normalOS);
            	
            	output.positionCS = GetShadowCasterPositionCS(posnInputs.positionWS, normInputs.normalWS);
            	return output;
            }

            float4 Fragment(Interpolators input) : SV_TARGET {
				return 0;
            }
            ENDHLSL
		}
	}
}
