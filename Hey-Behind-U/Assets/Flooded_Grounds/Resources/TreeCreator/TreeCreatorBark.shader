Shader "Universal Render Pipeline/Nature/Tree Creator Bark"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _Shininess ("Smoothness", Range(0.01, 1)) = 0.078125
        _BaseMap ("Base Map (RGB) Alpha (A)", 2D) = "white" {}
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _GlossMap ("Gloss Map (A)", 2D) = "black" {}
        _SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
        [HideInInspector] _TreeInstanceColor ("TreeInstanceColor", Vector) = (1,1,1,1)
        [HideInInspector] _TreeInstanceScale ("TreeInstanceScale", Vector) = (1,1,1,1)
        [HideInInspector] _SquashAmount ("Squash", Float) = 1
    }

    SubShader
    {
        Tags { 
            "RenderType"="Opaque" 
            "Queue"="Geometry" 
            "RenderPipeline"="UniversalPipeline"
        }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : NORMAL;
                float4 tangentWS : TANGENT;
                float4 color : COLOR;
                float fogCoord : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float _Shininess;
                float4 _SpecColor;
                float4 _BaseMap_ST;
            CBUFFER_END

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_BumpMap); SAMPLER(sampler_BumpMap);
            TEXTURE2D(_GlossMap); SAMPLER(sampler_GlossMap);

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);
                
                OUT.positionCS = positionInputs.positionCS;
                OUT.positionWS = positionInputs.positionWS;
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.normalWS = normalInputs.normalWS;
                OUT.tangentWS = float4(normalInputs.tangentWS, IN.tangentOS.w);
                OUT.color = IN.color;
                OUT.fogCoord = ComputeFogFactor(OUT.positionCS.z);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // 텍스처 샘플링
                float4 baseCol = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                float gloss = SAMPLE_TEXTURE2D(_GlossMap, sampler_GlossMap, IN.uv).a;
                float4 normalTex = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, IN.uv);

                // 노말 매핑 계산 (URP 표준 방식)
                float3 normalTS = UnpackNormal(normalTex);
                float3x3 TBN = float3x3(
                    normalize(IN.tangentWS.xyz),
                    normalize(cross(IN.normalWS, IN.tangentWS.xyz) * IN.tangentWS.w),
                    normalize(IN.normalWS)
                );
                float3 normalWS = normalize(mul(normalTS, TBN));

                // 알베도 계산
                float3 albedo = baseCol.rgb * IN.color.rgb * IN.color.a * _BaseColor.rgb;

                // 조명 데이터 준비
                InputData lightingInput = (InputData)0;
                lightingInput.positionWS = IN.positionWS;
                lightingInput.normalWS = normalWS;
                lightingInput.viewDirectionWS = GetWorldSpaceViewDir(IN.positionWS);
                lightingInput.shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
                lightingInput.fogCoord = IN.fogCoord;

                SurfaceData surfaceData;
                surfaceData.albedo = albedo;
                surfaceData.metallic = 0.0;
                surfaceData.specular = _SpecColor.rgb;
                surfaceData.smoothness = _Shininess * gloss;
                surfaceData.normalTS = normalTS;
                surfaceData.occlusion = 1.0;
                surfaceData.emission = 0;
                surfaceData.alpha = baseCol.a;

                // 최종 조명 계산
                half4 color = UniversalFragmentBlinnPhong(lightingInput, surfaceData);
                color.rgb = MixFog(color.rgb, IN.fogCoord);
                return color;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
 
}
