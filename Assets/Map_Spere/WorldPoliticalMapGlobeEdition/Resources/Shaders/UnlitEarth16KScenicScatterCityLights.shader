Shader "World Political Map/Unlit Earth 16K Scenic Scatter City Lights" 
{
	Properties {
		_MainTex ("Main Tex", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1)
		_TexTL ("Tex TL", 2D) = "white" {}
		_TexTR ("Tex TR", 2D) = "white" {}
		_TexBL ("Tex BL", 2D) = "white" {}
		_TexBR ("Tex BR", 2D) = "white" {}
		[Normal] _NormalMap ("Normal Map", 2D) = "bump" {}
		_BumpAmount ("Bump Amount", Range(0, 1)) = 0.5
		_SpecularPower("Specular Power", Float) = 32.0
		_SpecularIntensity("Specular Intensity", Float) = 2.0
		_CloudMap ("Cloud Map", 2D) = "black" {}
		_CloudSpeed ("Cloud Speed", Range(-1, 1)) = -0.04
		_CloudAlpha ("Cloud Alpha", Range(0, 1)) = 1
		_CloudShadowStrength ("Cloud Shadow Strength", Range(0, 1)) = 0.2
		_CloudElevation ("Cloud Elevation", Range(0.001, 0.1)) = 0.003
		_CityLights ("City Lights (RGB)", 2D) = "black" {}
		_CityLightsBrightness("City Lights Brightness", Float) = 1
		_Brightness("Brightness", Range(1,3)) = 1.25
		_Contrast("Contrast", Range(0,2)) = 1.1
		_AmbientLight("Ambient Light", Range(0,1)) = 0.1
		_SunLightDirection("Sun Light Direction", Vector) = (0,0,1)
        _SpecularPower("Specular Power", Float) = 1
        _SpecularIntensity("Specular Intensity", Float) = 0
        _AtmosphereAlpha("Atmosphere Alpha", Float) = 1.0        
		[HideInInspector] _fOuterRadius ("Outer Radius", Float) = 1.25
		[HideInInspector] fOuterRadius2 ("Outer Radius Squared", Float) = 1.5625
		[HideInInspector] fInnerRadius ("Inner Radius", Float) = 1
		[HideInInspector] fInnerRadius2 ("Inner Radius Squared", Float) = 1
		[HideInInspector] fKrESun ("", Float) = 1.0
		[HideInInspector] fKmESun ("", Float) = 1.0
		[HideInInspector] fKr4PI ("", Float) = 1.0
		[HideInInspector] fKm4PI ("", Float) = 1.0
		[HideInInspector] fScale ("", Float) = 1.0
		[HideInInspector] fScaleDepth ("", Float) = 1.0
		[HideInInspector] fScaleOverScaleDepth ("", Float) = 1.0
		[HideInInspector] fHdrExposure ("", Float) = 1.0

	}
	SubShader 
	{
		Tags { 
			"RenderType" = "Opaque"
			"RenderPipeline" = "UniversalPipeline"
			"Queue" = "Geometry-20"
		}
		ZWrite Off

		Pass {
			PackageRequirements {
				"com.unity.render-pipelines.universal": ""
			}
			
			Name "ForwardLit"
			Tags { "LightMode" = "UniversalForwardOnly" }

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
			#pragma multi_compile _ _SHADOWS_SOFT
			#pragma multi_compile __ WPM_SPECULAR_ENABLED
			#pragma multi_compile __ WPM_BUMPMAP_ENABLED
			#pragma multi_compile __ WPM_CLOUDSHADOWS_ENABLED

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

			TEXTURE2D(_TexTL);
			TEXTURE2D(_TexTR);
			TEXTURE2D(_TexBL);
			TEXTURE2D(_TexBR);
			TEXTURE2D(_NormalMap);
			TEXTURE2D(_CloudMap);
			TEXTURE2D(_CityLights);
			SAMPLER(sampler_TexTL);
			SAMPLER(sampler_TexTR);
			SAMPLER(sampler_TexBL);
			SAMPLER(sampler_TexBR);
			SAMPLER(sampler_NormalMap);
			SAMPLER(sampler_CloudMap);
			SAMPLER(sampler_CityLights);

			CBUFFER_START(UnityPerMaterial)
				float3 v3InvWavelength;
				float fOuterRadius;
				float fOuterRadius2;
				float fInnerRadius;
				float fInnerRadius2;
				float fKrESun;
				float fKmESun;
				float fKr4PI;
				float fKm4PI;
				float fScale;
				float fScaleDepth;
				float fScaleOverScaleDepth;
				float fHdrExposure;
				float _BumpAmount;
				float _CloudSpeed;
				float _CloudAlpha;
				float _CloudShadowStrength;
				float _CloudElevation;
				float3 _SunLightDirection;
				float _Brightness;
				float _Contrast;
				float _AmbientLight;
				float _SpecularPower;
				float _SpecularIntensity;
				float _AtmosphereAlpha;
				float _CityLightsBrightness;
			CBUFFER_END

			struct Attributes {
				float4 positionOS : POSITION;
				float2 texcoord : TEXCOORD0;
				float4 tangentOS : TANGENT;
				float3 normalOS : NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct Varyings {
				float4 positionCS : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 c0 : COLOR0;
				float3 c1 : COLOR1;
				float3 viewDir: TEXCOORD1;
				float3 normal: NORMAL;
				float3 positionWS : TEXCOORD2;
				#if WPM_BUMPMAP_ENABLED
				float3 tspace0 : TEXCOORD3;
				float3 tspace1 : TEXCOORD4;
				float3 tspace2 : TEXCOORD5;
				#endif
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			float scale(float fCos) {
				float x = 1.0 - fCos;
				return fScaleDepth * exp(-0.00287 + x*(0.459 + x*(3.83 + x*(-6.80 + x*5.25))));
			}

			float3 projectOnPlane(float3 v, float3 n) {
				return v - dot(v, n) * n;
			}

			Varyings vert(Attributes input) {
				Varyings output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				float3 center = float3(UNITY_MATRIX_M[0][3], UNITY_MATRIX_M[1][3], UNITY_MATRIX_M[2][3]);
				float3 v3CameraPos = _WorldSpaceCameraPos - center;
				float fCameraHeight = length(v3CameraPos);
				float fCameraHeight2 = fCameraHeight*fCameraHeight;

				float3 v3Pos = TransformObjectToWorld(input.positionOS.xyz) - center;
				float3 v3Ray = v3Pos - v3CameraPos;
				float fFar = length(v3Ray);
				v3Ray /= fFar;

				float B = 2.0 * dot(v3CameraPos, v3Ray);
				float C = fCameraHeight2 - fOuterRadius2;
				float fDet = max(0.0, B*B - 4.0 * C);
				float fNear = 0.5 * (-B - sqrt(fDet));

				float3 v3Start = v3CameraPos + v3Ray * fNear;
				fFar -= fNear;
				float fDepth = exp((fInnerRadius - fOuterRadius) / fScaleDepth);
				float fCameraAngle = dot(-v3Ray, v3Pos) / length(v3Pos);
				float fLightAngle = dot(_SunLightDirection, v3Pos) / length(v3Pos);
				float fCameraScale = scale(fCameraAngle);
				float fLightScale = scale(fLightAngle);
				float fCameraOffset = fDepth*fCameraScale;
				float fTemp = (fLightScale + fCameraScale);

				const float fSamples = 2.0;

				float fSampleLength = fFar / fSamples;
				float fScaledLength = fSampleLength * fScale;
				float3 v3SampleRay = v3Ray * fSampleLength;
				float3 v3SamplePoint = v3Start + v3SampleRay * 0.5;

				float3 v3FrontColor = float3(0.0, 0.0, 0.0);
				float3 v3Attenuate = float3(0.0, 0.0, 0.0);
				for(int i=0; i<int(fSamples); i++) {
					float fHeight = length(v3SamplePoint);
					float fDepth = exp(fScaleOverScaleDepth * (fInnerRadius - fHeight));
					float fScatter = fDepth*fTemp - fCameraOffset;
					v3Attenuate = exp(-fScatter * (v3InvWavelength * fKr4PI + fKm4PI));
					v3FrontColor += v3Attenuate * (fDepth * fScaledLength);
					v3SamplePoint += v3SampleRay;
				}

				output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
				output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
				output.uv = input.texcoord;
				output.c0 = v3FrontColor * (v3InvWavelength * fKrESun + fKmESun);
				output.c1 = v3Attenuate;
				output.viewDir = normalize(GetWorldSpaceViewDir(output.positionWS));

				float3 wNormal = TransformObjectToWorldNormal(input.normalOS);
				output.normal = wNormal;

				#if WPM_BUMPMAP_ENABLED
				float3 wTangent = TransformObjectToWorldDir(input.tangentOS.xyz);
				float tangentSign = input.tangentOS.w * unity_WorldTransformParams.w;
				float3 wBitangent = cross(wNormal, wTangent) * tangentSign;
				output.tspace0 = float3(wTangent.x, wBitangent.x, wNormal.x);
				output.tspace1 = float3(wTangent.y, wBitangent.y, wNormal.y);
				output.tspace2 = float3(wTangent.z, wBitangent.z, wNormal.z);
				#endif

				return output;
			}

			float4 frag(Varyings input) : SV_Target {
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

				// Sample Earth texture
				float4 color;
				if (input.uv.x < 0.5) {
					if (input.uv.y > 0.5) {
						color = SAMPLE_TEXTURE2D_LOD(_TexTL, sampler_TexTL, 
							float2(input.uv.x * 2.0, (input.uv.y - 0.5) * 2.0), 0);
					} else {
						color = SAMPLE_TEXTURE2D_LOD(_TexBL, sampler_TexBL, 
							float2(input.uv.x * 2.0, input.uv.y * 2.0), 0);
					}
				} else {
					if (input.uv.y > 0.5) {
						color = SAMPLE_TEXTURE2D_LOD(_TexTR, sampler_TexTR, 
							float2((input.uv.x - 0.5) * 2.0, (input.uv.y - 0.5) * 2.0), 0);
					} else {
						color = SAMPLE_TEXTURE2D_LOD(_TexBR, sampler_TexBR, 
							float2((input.uv.x - 0.5) * 2.0, input.uv.y * 2.0), 0);
					}
				}

				float3 snormal = normalize(input.normal);

				#if WPM_SPECULAR_ENABLED
				float3 worldRefl = reflect(_SunLightDirection, snormal);
				float spec = pow(max(0.0, dot(-input.viewDir, worldRefl)), _SpecularPower);
				color.rgb += (spec * color.a * _SpecularIntensity);
				#endif

				#if WPM_BUMPMAP_ENABLED
				float3 tnormal = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, input.uv));
				float3 worldNormal;
				worldNormal.x = dot(input.tspace0, tnormal);
				worldNormal.y = dot(input.tspace1, tnormal);
				worldNormal.z = dot(input.tspace2, tnormal);
				float3 normal = normalize(lerp(snormal, worldNormal, _BumpAmount));
				#else
				float3 normal = snormal;
				#endif

			float2 t = float2(_Time.x * _CloudSpeed, 0);
			float2 disp = -input.viewDir.xy * _CloudElevation;
				float4 cloud = SAMPLE_TEXTURE2D(_CloudMap, sampler_CloudMap, input.uv + t - disp);
				float cloudReflectancy = 1.0 + input.c1.g;
				cloud.rgb *= cloudReflectancy * cloud.a * _CloudAlpha;

				#if WPM_CLOUDSHADOWS_ENABLED
				const float2 c = float2(0.998,0);
				float3 proj = projectOnPlane(_SunLightDirection, snormal);
				float3 up = projectOnPlane(float3(0,1,0), snormal);
				float3 right = projectOnPlane(float3(1,0,0), snormal);
				float x = dot(proj, right);
				float y = dot(proj, up);
				float2 persp = float2(x,y) * 0.01;
				float shadows = SAMPLE_TEXTURE2D(_CloudMap, sampler_CloudMap, input.uv + c + t + persp).a * 
							   _CloudAlpha * _CloudShadowStrength;
				#endif

				float LdotN = saturate(dot(_SunLightDirection, normal));
				float earthLighting = LdotN * input.c1.g;
				#if WPM_CLOUDSHADOWS_ENABLED
				earthLighting *= 1.0 - shadows;
				#endif
				earthLighting += _AmbientLight;
				color.rgb *= earthLighting;

				float3 cityLights = SAMPLE_TEXTURE2D(_CityLights, sampler_CityLights, input.uv).rgb * _CityLightsBrightness;
				color.rgb += cityLights * (1.0 - input.c1.g) * (1.0 - cloud.g);

				cloud.rgb += input.c0 * _AtmosphereAlpha;
				cloud.rgb *= input.c1.g + _AmbientLight;
				color.rgb += cloud.rgb;

				color.rgb = (color.rgb - 0.5.xxx) * _Contrast + 0.5.xxx;
				color.rgb = 1.0 - exp(color.rgb * -_Brightness);

				// Apply main light shadows
				float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
				float shadowAttenuation = MainLightRealtimeShadow(shadowCoord);
				color.rgb *= shadowAttenuation;

				#if !UNITY_COLORSPACE_GAMMA
				color.rgb = SRGBToLinear(color.rgb);
				#endif

				return color;
			}
			ENDHLSL
		}
	}

	SubShader 
	{
		Tags { "Queue"="Geometry-20" "RenderType"="Opaque" }
		ZWrite Off
    	Pass 
    	{
    		
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile __ WPM_SPECULAR_ENABLED
			#pragma multi_compile __ WPM_BUMPMAP_ENABLED
			#pragma multi_compile __ WPM_CLOUDSHADOWS_ENABLED

			sampler2D _TexTL;
			sampler2D _TexTR;
			sampler2D _TexBL;
			sampler2D _TexBR;
			sampler2D _NormalMap;
			sampler2D _CloudMap;
			sampler2D _CityLights;
		
			uniform float3 _SunLightDirection;		// The direction vector to the light source
			uniform float3 v3InvWavelength; // 1 / pow(wavelength, 4) for the red, green, and blue channels
			uniform float fOuterRadius;		// The outer (atmosphere) radius
			uniform float fOuterRadius2;	// fOuterRadius^2
			uniform float fInnerRadius;		// The inner (planetary) radius
			uniform float fInnerRadius2;	// fInnerRadius^2
			uniform float fKrESun;			// Kr * ESun
			uniform float fKmESun;			// Km * ESun
			uniform float fKr4PI;			// Kr * 4 * PI
			uniform float fKm4PI;			// Km * 4 * PI
			uniform float fScale;			// 1 / (fOuterRadius - fInnerRadius)
			uniform float fScaleDepth;		// The scale depth (i.e. the altitude at which the atmosphere's average density is found)
			uniform float fScaleOverScaleDepth;	// fScale / fScaleDepth
			uniform float fHdrExposure;		// HDR exposure
			uniform float _BumpAmount;		// Normal/Bump effect amount (0..1)
			uniform	float _CloudSpeed;
			uniform float _CloudAlpha;
			uniform float _CloudShadowStrength;
			uniform float _CloudElevation;
			uniform float _Brightness;
			uniform float _Contrast;
			uniform float _AmbientLight;
			uniform float _SpecularPower;
			uniform float _SpecularIntensity;
            uniform float _AtmosphereAlpha;
			uniform float _CityLightsBrightness;

			struct v2f 
			{
    			float4 pos : SV_POSITION;
    			float2 uv : TEXCOORD0;
    			float3 c0 : COLOR0;
    			float3 c1 : COLOR1;
    			float3 viewDir: TEXCOORD1;
    			float3 normal: NORMAL;
    			#if WPM_BUMPMAP_ENABLED
				float3 tspace0 : TEXCOORD2; // tangent.x, bitangent.x, normal.x
               	float3 tspace1 : TEXCOORD3; // tangent.y, bitangent.y, normal.y
               	float3 tspace2 : TEXCOORD4; // tangent.z, bitangent.z, normal.z
               	#endif
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
			};
			
			float scale(float fCos)
			{
				float x = 1.0 - fCos;
				return fScaleDepth * exp(-0.00287 + x*(0.459 + x*(3.83 + x*(-6.80 + x*5.25))));
			}

			v2f vert (appdata_tan v)
			{
				float3 center = float3(unity_ObjectToWorld[0][3], unity_ObjectToWorld[1][3], unity_ObjectToWorld[2][3]);
			    float3 v3CameraPos = _WorldSpaceCameraPos - center;	// The camera's current position
				float fCameraHeight = length(v3CameraPos);					// The camera's current height
				float fCameraHeight2 = fCameraHeight*fCameraHeight;			// fCameraHeight^2
				
				// Get the ray from the camera to the vertex and its length (which is the far point of the ray passing through the atmosphere)
				float3 v3Pos = mul(unity_ObjectToWorld, v.vertex).xyz - center;
				float3 v3Ray = v3Pos - v3CameraPos;
				float fFar = length(v3Ray);
				v3Ray /= fFar;
				
				// Calculate the closest intersection of the ray with the outer atmosphere (which is the near point of the ray passing through the atmosphere)
				float B = 2.0 * dot(v3CameraPos, v3Ray);
				float C = fCameraHeight2 - fOuterRadius2;
				float fDet = max(0.0, B*B - 4.0 * C);
				float fNear = 0.5 * (-B - sqrt(fDet));
				
				// Calculate the ray's starting position, then calculate its scattering offset
				float3 v3Start = v3CameraPos + v3Ray * fNear;
				fFar -= fNear;
				float fDepth = exp((fInnerRadius - fOuterRadius) / fScaleDepth);
				float fCameraAngle = dot(-v3Ray, v3Pos) / length(v3Pos);
				float v3PosLength = length(v3Pos);
				float fLightAngle = dot(_SunLightDirection, v3Pos) / v3PosLength;
				float fCameraScale = scale(fCameraAngle);
				float fLightScale = scale(fLightAngle);
				float fCameraOffset = fDepth*fCameraScale;
				float fTemp = (fLightScale + fCameraScale);
				
				const float fSamples = 2.0;
				
				// Initialize the scattering loop variables
				float fSampleLength = fFar / fSamples;
				float fScaledLength = fSampleLength * fScale;
				float3 v3SampleRay = v3Ray * fSampleLength;
				float3 v3SamplePoint = v3Start + v3SampleRay * 0.5;
				
				// Now loop through the sample rays
				float3 v3FrontColor = float3(0.0, 0.0, 0.0);
				float3 v3Attenuate;
				for(int i=0; i<int(fSamples); i++)
				{
					float fHeight = length(v3SamplePoint);
					float fDepth = exp(fScaleOverScaleDepth * (fInnerRadius - fHeight));
					float fScatter = fDepth*fTemp - fCameraOffset;
					v3Attenuate = exp(-fScatter * (v3InvWavelength * fKr4PI + fKm4PI));
					v3FrontColor += v3Attenuate * (fDepth * fScaledLength);
					v3SamplePoint += v3SampleRay;
				}
				
    			v2f o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_OUTPUT(v2f, o);
            UNITY_TRANSFER_INSTANCE_ID(v, o);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    			o.pos = UnityObjectToClipPos(v.vertex);
    			o.uv = v.texcoord;
    			o.c0 = v3FrontColor * (v3InvWavelength * fKrESun + fKmESun);
    			o.c1 = v3Attenuate;
				o.viewDir = normalize(WorldSpaceViewDir(v.vertex));

				// normal stuff
				float3 wNormal		= v3Pos / v3PosLength;
				o.normal = wNormal;
				#if WPM_BUMPMAP_ENABLED
                float3 wTangent = UnityObjectToWorldDir(v.tangent.xyz);
       	        float tangentSign = v.tangent.w * unity_WorldTransformParams.w;
           	    float3 wBitangent = cross(wNormal, wTangent) * tangentSign;
               	// output the tangent space matrix
                o.tspace0 = float3(wTangent.x, wBitangent.x, wNormal.x);
   	            o.tspace1 = float3(wTangent.y, wBitangent.y, wNormal.y);
       	        o.tspace2 = float3(wTangent.z, wBitangent.z, wNormal.z);
       	        #endif

    			return o;
			}
			
			float3 projectOnPlane(float3 v, float3 n) {
			 	return v - dot(v, n) * n;
			}

			float4 frag(v2f i) : SV_Target
			{
            UNITY_SETUP_INSTANCE_ID(i);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); 
				// get Earth texture texel
				float4 color;
				// compute Earth pixel color
				if (i.uv.x<0.5) {
					if (i.uv.y>0.5) {
						color = tex2Dlod(_TexTL, float4(i.uv.x * 2.0, (i.uv.y - 0.5) * 2.0, 0, 0));
					} else {
						color = tex2Dlod(_TexBL, float4(i.uv.x * 2.0, i.uv.y * 2.0, 0, 0));
					}
				} else {
					if (i.uv.y>0.5) {
						color = tex2Dlod(_TexTR, float4((i.uv.x - 0.5) * 2.0f, (i.uv.y - 0.5) * 2.0, 0, 0));
					} else {
						color = tex2Dlod(_TexBR, float4((i.uv.x - 0.5) * 2.0f, i.uv.y * 2.0, 0, 0));
					}
				}


                // surface normal
                float3 snormal = normalize(i.normal);

                // specular reflection (where Earth alpha determines water)
                #if WPM_SPECULAR_ENABLED
                float3 worldRefl = reflect(_SunLightDirection, snormal);
                float spec = pow(max(0.0, dot(-i.viewDir, worldRefl)), _SpecularPower);
                color.rgb += (spec * color.a * _SpecularIntensity);
                #endif

                // apply bump mapping
                #if WPM_BUMPMAP_ENABLED
                float3 tnormal = UnpackNormal(tex2D(_NormalMap, i.uv)); 
                float3 worldNormal;
                worldNormal.x = dot(i.tspace0, tnormal);
                worldNormal.y = dot(i.tspace1, tnormal);
                worldNormal.z = dot(i.tspace2, tnormal);
                float3 normal = normalize(lerp(snormal, worldNormal, _BumpAmount));
                #else
                float3 normal = snormal;
                #endif
                
                // Clouds
                float2 t = float2(_Time[0] * _CloudSpeed, 0);
                float2 disp = -i.viewDir * _CloudElevation;
                float4 cloud = tex2D (_CloudMap, i.uv + t - disp);
                float cloudReflectancy = 1.0 + i.c1.g;
                cloud.rgb *= cloudReflectancy * cloud.a * _CloudAlpha;

                // Cloud shadows
                #if WPM_CLOUDSHADOWS_ENABLED
                const float2 c = float2(0.998,0);
                float3 proj  = projectOnPlane(_SunLightDirection, snormal);
                float3 up    = projectOnPlane(float3(0,1,0), snormal);
                float3 right = projectOnPlane(float3(1,0,0), snormal);
                float  x     = dot(proj, right);
                float  y     = dot(proj, up);
                float2 persp = float2(x,y) * 0.01;
                float shadows = tex2D (_CloudMap, i.uv + c + t + persp).a * _CloudAlpha * _CloudShadowStrength;
                #endif
                
                // Earth component with bumpMap
                float LdotN = saturate(dot(_SunLightDirection, normal));
                float earthLighting = LdotN * i.c1.g;
                #if WPM_CLOUDSHADOWS_ENABLED
                earthLighting *= 1.0 - shadows;
                #endif
                earthLighting += _AmbientLight;
                color.rgb *= earthLighting;
                
                // Add city lights
                float3 cityLights = tex2D(_CityLights, i.uv).rgb * _CityLightsBrightness;
                color.rgb += cityLights * (1.0 - i.c1.g) * (1.0 - cloud.g);
                
                // apply atmosphere
                cloud.rgb += i.c0 * _AtmosphereAlpha;
                cloud.rgb *= i.c1.g + _AmbientLight;
                color.rgb += cloud.rgb;
                
                // adjust color from HDR
                color.rgb = (color.rgb - 0.5.xxx) * _Contrast + 0.5.xxx;
                color.rgb = 1.0 - exp(color.rgb * -_Brightness);
                
                #if !UNITY_COLORSPACE_GAMMA
                color.rgb = GammaToLinearSpace(color.rgb);
                #endif
                
                return color;
			}
			
			ENDCG

    	}
	}
}
