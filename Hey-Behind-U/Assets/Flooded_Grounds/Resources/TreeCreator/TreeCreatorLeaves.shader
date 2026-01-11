Shader "Universal Render Pipeline/Nature/Tree Creator Leaves"
{
    Properties
    {
        [MainColor] _BaseColor("Main Color", Color) = (1,1,1,1)
        [MainTexture] _BaseMap("Base (RGB) Alpha (A)", 2D) = "white" {}
        [Normal] _BumpMap("Normal Map", 2D) = "bump" {}
        _GlossMap("Gloss (A)", 2D) = "black" {}
        _TranslucencyMap("Translucency (A)", 2D) = "white" {}
        _Cutoff("Alpha Cutoff", Range(0,1)) = 0.3
        _Shininess("Smoothness", Range(0.01, 1)) = 0.078125
        
        [HideInInspector] _TreeInstanceColor("TreeInstanceColor", Vector) = (1,1,1,1)
        [HideInInspector] _TreeInstanceScale("TreeInstanceScale", Vector) = (1,1,1,1)
        [HideInInspector] _SquashAmount("Squash", Float) = 1
    }

    SubShader
    {
        Tags {
            "RenderType" = "TransparentCutout"
            "Queue" = "AlphaTest"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }
        LOD 300
        Cull Off

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local _NORMALMAP
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
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float4 tangentWS : TEXCOORD3;
                float fogCoord : TEXCOORD4;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_BumpMap); SAMPLER(sampler_BumpMap);
            TEXTURE2D(_GlossMap); SAMPLER(sampler_GlossMap);
            TEXTURE2D(_TranslucencyMap); SAMPLER(sampler_TranslucencyMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _BaseMap_ST;
                float _Cutoff;
                float _Shininess;
                float4 _TreeInstanceColor;
                float4 _TreeInstanceScale;
                float _SquashAmount;
            CBUFFER_END

            float3 TreeVertLeaf(float3 pos)
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

                float3 positionOS = TreeVertLeaf(IN.positionOS.xyz);
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

                // 텍스처 샘플링
                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                half4 glossMap = SAMPLE_TEXTURE2D(_GlossMap, sampler_GlossMap, IN.uv);
                half4 translucencyMap = SAMPLE_TEXTURE2D(_TranslucencyMap, sampler_TranslucencyMap, IN.uv);

                // 알파 테스트
                clip(baseMap.a - _Cutoff);

                // 알베도 계산
                half3 albedo = baseMap.rgb * IN.color.rgb * _BaseColor.rgb;

                // 노말 매핑 변수 선언
                half3 normalTS = half3(0, 0, 1); // 기본값 설정

                #ifdef _NORMALMAP
                    half4 normalTex = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, IN.uv);
                    normalTS = UnpackNormal(normalTex);
                    float3x3 TBN = float3x3(
                        normalize(IN.tangentWS.xyz),
                        normalize(cross(IN.normalWS, IN.tangentWS.xyz) * IN.tangentWS.w),
                        normalize(IN.normalWS)
                    );
                    half3 normalWS = normalize(mul(normalTS, TBN));
                #else
                    half3 normalWS = normalize(IN.normalWS);
                #endif

                // 조명 데이터 준비
                InputData lightingInput = (InputData)0;
                lightingInput.positionWS = IN.positionWS;
                lightingInput.normalWS = normalWS;
                lightingInput.viewDirectionWS = GetWorldSpaceViewDir(IN.positionWS);
                lightingInput.shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
                lightingInput.fogCoord = IN.fogCoord;

                // 표면 속성 설정
                SurfaceData surfaceData;
                surfaceData.albedo = albedo;
                surfaceData.metallic = 0.0;
                surfaceData.specular = _BaseColor.rgb;
                surfaceData.smoothness = _Shininess * glossMap.a;
                surfaceData.normalTS = normalTS; // 수정된 부분
                surfaceData.occlusion = 1.0;
                surfaceData.emission = 0;
                surfaceData.alpha = 1.0;

                // 투명도 효과
                half3 translucency = translucencyMap.rgb * _BaseColor.rgb;
                half transPower = 4.0;
                half3 transLight = _MainLightColor.rgb * translucency * transPower;
                half3 backLight = saturate(dot(normalWS, -_MainLightPosition.xyz)) * transLight;

                // 조명 계산
                half4 color = UniversalFragmentBlinnPhong(lightingInput, surfaceData);
                color.rgb += backLight;
                color.rgb = MixFog(color.rgb, IN.fogCoord);

                return color;
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float _Cutoff;
                float4 _TreeInstanceScale;
                float _SquashAmount;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);

                float3 positionOS = IN.positionOS.xyz * _TreeInstanceScale.xyz;
                positionOS.y *= _SquashAmount;
                OUT.positionCS = TransformObjectToHClip(positionOS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half alpha = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv).a;
                clip(alpha - _Cutoff);
                return 0;
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Simple Lit"
    Dependency "OptimizedShader" = "Hidden/Nature/Tree Creator Leaves Optimized"
}
