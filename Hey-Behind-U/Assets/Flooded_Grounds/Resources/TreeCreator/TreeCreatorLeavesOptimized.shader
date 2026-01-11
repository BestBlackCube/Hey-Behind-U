Shader "Universal Render Pipeline/Nature/Tree Creator Leaves Optimized"
{
    Properties
    {
        [MainColor] _Color("Main Color", Color) = (1,1,1,1)
        _TranslucencyColor("Translucency Color", Color) = (0.73,0.85,0.41,1)
        _Cutoff("Alpha Cutoff", Range(0,1)) = 0.3
        _TranslucencyViewDependency("View Dependency", Range(0,1)) = 0.7
        _ShadowStrength("Shadow Strength", Range(0,1)) = 0.8
        _ShadowOffsetScale("Shadow Offset Scale", Float) = 1
        
        [MainTexture] _MainTex("Base (RGB) Alpha (A)", 2D) = "white" {}
        _ShadowTex("Shadow (RGB)", 2D) = "white" {}
        _BumpSpecMap("Normalmap (GA) Spec (R)", 2D) = "bump" {}
        _TranslucencyMap("Translucency (B) Gloss(A)", 2D) = "white" {}

        [HideInInspector] _TreeInstanceColor("TreeInstanceColor", Vector) = (1,1,1,1)
        [HideInInspector] _TreeInstanceScale("TreeInstanceScale", Vector) = (1,1,1,1)
        [HideInInspector] _SquashAmount("Squash", Float) = 1
    }

    SubShader
    {
        Tags {
            "IgnoreProjector"="True"
            "RenderType"="TransparentCutout"
            "Queue"="AlphaTest"
            "RenderPipeline"="UniversalPipeline"
        }
        LOD 200
        Cull Off

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            
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
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float4 tangentWS : TEXCOORD3;
                float3 viewDirWS : TEXCOORD4;
                float fogCoord : TEXCOORD5;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_BumpSpecMap); SAMPLER(sampler_BumpSpecMap);
            TEXTURE2D(_TranslucencyMap); SAMPLER(sampler_TranslucencyMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _TranslucencyColor;
                float _Cutoff;
                float _TranslucencyViewDependency;
                float4 _MainTex_ST;
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
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color = IN.color * _TreeInstanceColor;
                OUT.normalWS = normalInputs.normalWS;
                OUT.tangentWS = float4(normalInputs.tangentWS, IN.tangentOS.w);
                OUT.viewDirWS = GetWorldSpaceViewDir(positionInputs.positionWS);
                OUT.fogCoord = ComputeFogFactor(OUT.positionCS.z);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);

                // 텍스처 샘플링
                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                half4 bumpSpec = SAMPLE_TEXTURE2D(_BumpSpecMap, sampler_BumpSpecMap, IN.uv);
                half4 translucencyMap = SAMPLE_TEXTURE2D(_TranslucencyMap, sampler_TranslucencyMap, IN.uv);

                // 알파 테스트
                half viewDot = pow(saturate(dot(normalize(IN.viewDirWS), float3(0,0,1))), 12);
                half alpha = mainTex.a * lerp(1.0, viewDot, _TranslucencyViewDependency);
                clip(alpha - _Cutoff);

                // 노말 매핑
                half3 normalTS;
                normalTS.xy = bumpSpec.ga * 2.0 - 1.0;
                normalTS.z = sqrt(1.0 - saturate(dot(normalTS.xy, normalTS.xy)));
                float3x3 TBN = float3x3(
                    normalize(IN.tangentWS.xyz),
                    normalize(cross(IN.normalWS, IN.tangentWS.xyz) * IN.tangentWS.w),
                    normalize(IN.normalWS)
                );
                half3 normalWS = normalize(mul(normalTS, TBN));

                // 조명 계산
                InputData lightingInput = (InputData)0;
                lightingInput.positionWS = IN.positionWS;
                lightingInput.normalWS = normalWS;
                lightingInput.viewDirectionWS = normalize(IN.viewDirWS);
                lightingInput.shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
                lightingInput.fogCoord = IN.fogCoord;

                SurfaceData surfaceData;
                surfaceData.albedo = mainTex.rgb * IN.color.rgb * _Color.rgb;
                surfaceData.metallic = 0.0;
                surfaceData.specular = _Color.rgb;
                surfaceData.smoothness = translucencyMap.a * _Color.r;
                surfaceData.normalTS = normalTS;
                surfaceData.occlusion = 1.0;
                surfaceData.emission = _TranslucencyColor.rgb * 0.02;
                surfaceData.alpha = 1.0;

                half4 color = UniversalFragmentBlinnPhong(lightingInput, surfaceData);
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

            TEXTURE2D(_ShadowTex); SAMPLER(sampler_ShadowTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _ShadowTex_ST;
                float _Cutoff;
                float4 _TreeInstanceScale;
                float _SquashAmount;
            CBUFFER_END

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

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);

                float3 positionOS = IN.positionOS.xyz * _TreeInstanceScale.xyz;
                positionOS.y *= _SquashAmount;
                OUT.positionCS = TransformObjectToHClip(positionOS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _ShadowTex);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half alpha = SAMPLE_TEXTURE2D(_ShadowTex, sampler_ShadowTex, IN.uv).r;
                clip(alpha - _Cutoff);
                return 0;
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Simple Lit"
    Dependency "OptimizedShader" = "Universal Render Pipeline/Nature/SpeedTree8"
}
