Shader "World Political Map/Unlit Texture 16K" {
 
Properties {
	_MainTex ("Main Tex", 2D) = "white" {}
    _Color ("Color", Color) = (1,1,1)
	_TexTL ("Tex TL", 2D) = "white" {}
	_TexTR ("Tex TR", 2D) = "white" {}
	_TexBL ("Tex BL", 2D) = "white" {}
	_TexBR ("Tex BR", 2D) = "white" {}
}
 
SubShader {
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

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

        TEXTURE2D(_TexTL);
        TEXTURE2D(_TexTR);
        TEXTURE2D(_TexBL);
        TEXTURE2D(_TexBR);
        SAMPLER(sampler_TexTL);
        SAMPLER(sampler_TexTR);
        SAMPLER(sampler_TexBL);
        SAMPLER(sampler_TexBR);

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

SubShader {
  	Tags { "Queue"="Geometry-20" "RenderType"="Opaque" }
		Lighting Off
		ZWrite Off
		Pass {
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				
				#include "UnityCG.cginc"
				
				sampler2D _TexTL;
				sampler2D _TexTR;
				sampler2D _TexBL;
				sampler2D _TexBR;

				struct v2f {
					float4 pos : SV_POSITION;
					float2 uv : TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
				};
        
				v2f vert (appdata_base v) {
					v2f o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_OUTPUT(v2f, o);
            UNITY_TRANSFER_INSTANCE_ID(v, o);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					o.pos 				= UnityObjectToClipPos (v.vertex);
					o.uv				= v.texcoord.xy;
					return o;
				 }
				
				half4 frag (v2f i) : SV_Target {
            UNITY_SETUP_INSTANCE_ID(i);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); 
					half4 color;
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

		  		   return color;
				}
			
			ENDCG
		}
	}
}