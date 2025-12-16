Shader "World Political Map/Unlit Single Texture" {

Properties {
	_MainTex ("Texture", 2D) = "white"  {}
}

// URP Subshader
SubShader {
	Tags { 
		"Queue"="Geometry-20" 
		"RenderType"="Opaque"
		"RenderPipeline" = "UniversalPipeline"
	}
	ZWrite Off

	Pass {
		PackageRequirements {
			"com.unity.render-pipelines.universal":""
		}            
		Name "ForwardLit"
		Tags { "LightMode" = "UniversalForwardOnly" }

		HLSLPROGRAM
		#pragma vertex vert	
		#pragma fragment frag
		#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
		#pragma multi_compile _ _SHADOWS_SOFT

		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

		TEXTURE2D(_MainTex);
		SAMPLER(sampler_MainTex);

		struct Attributes {
			float4 positionOS : POSITION;
			float2 texcoord : TEXCOORD0;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		struct Varyings {
			float4 positionCS : SV_POSITION;
			float2 uv : TEXCOORD0;
			float3 positionWS : TEXCOORD1;
			UNITY_VERTEX_INPUT_INSTANCE_ID
			UNITY_VERTEX_OUTPUT_STEREO
		};
		
		Varyings vert(Attributes input) {
			Varyings output;
			UNITY_SETUP_INSTANCE_ID(input);
			UNITY_TRANSFER_INSTANCE_ID(input, output);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

			output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
			output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
			output.uv = input.texcoord;
			return output;
		}
		
		float4 frag(Varyings input) : SV_Target {
			UNITY_SETUP_INSTANCE_ID(input);
			UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

			float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

			// Apply main light shadows
			float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
			float atten = MainLightRealtimeShadow(shadowCoord);
			color.rgb *= atten;

			return color;
		}
			
		ENDHLSL
	}
}

// Built-in Pipeline Subshader
SubShader {
	Tags { "Queue"="Geometry-20" "RenderType"="Opaque" }
	ZWrite Off
	Pass {
    	CGPROGRAM
		#pragma vertex vert	
		#pragma fragment frag
        #pragma fragmentoption ARB_precision_hint_fastest
        #include "UnityCG.cginc"

		sampler2D _MainTex;
		
		struct appdata {
			float4 vertex : POSITION;
			float2 texcoord: TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		struct v2f {
			float4 pos : SV_POSITION;
			float2 uv: TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
		};
		
		v2f vert(appdata v) {
            v2f o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_OUTPUT(v2f, o);
            UNITY_TRANSFER_INSTANCE_ID(v, o);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
			o.pos = UnityObjectToClipPos(v.vertex);
			o.uv = v.texcoord;
			return o;
		}
		
		fixed4 frag(v2f i) : SV_Target {
			UNITY_SETUP_INSTANCE_ID(i);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); 
			return tex2D(_MainTex, i.uv);
		}
			
		ENDCG
	}
}  

}
