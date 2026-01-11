Shader "Universal Render Pipeline/Nature/Tree Creator Bark Optimized"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1,1,1,1)
        [MainTexture] _BaseMap("Base Map (RGB) Alpha (A)", 2D) = "white" {}
        _BumpSpecMap("Normal (GA) Spec (R)", 2D) = "bump" {}
        _TranslucencyMap("Translucency (RGB) Gloss (A)", 2D) = "white" {}
        
        // 숨김 프로퍼티
        [HideInInspector] _TreeInstanceColor("TreeInstanceColor", Vector) = (1,1,1,1)
        [HideInInspector] _TreeInstanceScale("TreeInstanceScale", Vector) = (1,1,1,1)
        [HideInInspector] _SquashAmount("Squash", Float) = 1
    }

    SubShader
    {
        Tags { 
            "RenderType" = "Opaque" 
            "Queue" = "Geometry" 
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
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
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float3 normalWS : TEXCOORD1;
                float4 tangentWS : TEXCOORD2;
                float3 positionWS : TEXCOORD3;
                float fogCoord : TEXCOORD4;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_BumpSpecMap); SAMPLER(sampler_BumpSpecMap);
            TEXTURE2D(_TranslucencyMap); SAMPLER(sampler_TranslucencyMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _BaseMap_ST;
                float4 _BumpSpecMap_ST;
                float4 _TranslucencyMap_ST;
                float _SquashAmount;
                float4 _TreeInstanceColor;
                float4 _TreeInstanceScale;
            CBUFFER_END

            // 트리 정점 변형 함수
            float3 TreeVertBark(float3 pos)
            {
                pos.xyz *= _TreeInstanceScale.xyz;
                pos.y *= _SquashAmount;
                return pos;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);

                // 트리 변형 적용
                float3 positionOS = TreeVertBark(IN.positionOS.xyz);
                
                VertexPositionInputs positionInputs = GetVertexPositionInputs(positionOS);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);

                OUT.positionCS = positionInputs.positionCS;
                OUT.positionWS = positionInputs.positionWS;
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.color = IN.color * _TreeInstanceColor;
                OUT.normalWS = normalInputs.normalWS;
                OUT.tangentWS = float4(normalInputs.tangentWS, IN.tangentOS.w);
                OUT.fogCoord = ComputeFogFactor(OUT.positionCS.z);
                
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);

                // 기본 텍스처 샘플링
                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                half4 bumpSpec = SAMPLE_TEXTURE2D(_BumpSpecMap, sampler_BumpSpecMap, IN.uv);
                half4 translucency = SAMPLE_TEXTURE2D(_TranslucencyMap, sampler_TranslucencyMap, IN.uv);

                // 알베도 계산
                half3 albedo = baseMap.rgb * IN.color.rgb * _BaseColor.rgb;

                // 노말 매핑 (DXT5nm 압축 해제)
                half3 normalTS;
                normalTS.xy = bumpSpec.ga * 2.0 - 1.0;
                normalTS.z = sqrt(1.0 - saturate(dot(normalTS.xy, normalTS.xy)));
                
                float3x3 TBN = float3x3(
                    normalize(IN.tangentWS.xyz),
                    normalize(cross(IN.normalWS, IN.tangentWS.xyz) * IN.tangentWS.w),
                    normalize(IN.normalWS)
                );
                half3 normalWS = normalize(mul(normalTS, TBN));

                // 표면 속성 설정
                InputData inputData = (InputData)0;
                inputData.positionWS = IN.positionWS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = GetWorldSpaceViewDir(IN.positionWS);
                inputData.shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
                inputData.fogCoord = IN.fogCoord;

                SurfaceData surfaceData;
                surfaceData.albedo = albedo;
                surfaceData.metallic = 0.0;
                surfaceData.specular = _BaseColor.rgb;
                surfaceData.smoothness = bumpSpec.r * translucency.a;
                surfaceData.normalTS = normalTS;
                surfaceData.occlusion = 1.0;
                surfaceData.emission = 0;
                surfaceData.alpha = baseMap.a;

                // 조명 계산
                half4 color = UniversalFragmentBlinnPhong(inputData, surfaceData);
                color.rgb = MixFog(color.rgb, IN.fogCoord);
                return color;
            }
            ENDHLSL
        }

        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }

    FallBack "Universal Render Pipeline/Simple Lit"
    Dependency "BillboardShader" = "Hidden/Nature/Tree Creator Bark Rendertex"
}
