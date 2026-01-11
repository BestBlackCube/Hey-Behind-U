Shader "Flooded_Grounds/PBR_Water_URP"
{
    Properties
    {
        [MainColor] _BaseColor("Main Color", Color) = (1,1,1,1)
        _Emis("Self-Ilumination", Range(0,1)) = 0.1
        _Smoothness("Smoothness", Range(0,1)) = 0.9
        _Parallax ("Height", Range (0.005, 0.08)) = 0.02

        [MainTexture] _MainTex("Base (RGB) Gloss (A)", 2D) = "white" {}
        _BumpMap("Normalmap", 2D) = "bump" {}
        _BumpMap2("Normalmap2", 2D) = "bump" {}
        _BumpLerp("Normalmap2 Blend", Range(0,1)) = 0.5
        _ParallaxMap("Heightmap", 2D) = "black" {}
        _WaveNoise("Wave Noise (for movement)", 2D) = "gray" {}

        _ScrollSpeed("Scroll Speed", Float) = 0.2
        _WaveFreq("Wave Frequency", Float) = 20
        _WaveHeight("Wave Height", Float) = 0.1

        _WaveSpeed("Wave Speed", Range(0,2)) = 1.0
        _WaveScale("Wave Scale", Range(0,0.5)) = 0.1
        _WaveDistortion("Wave Distortion", Range(0,5)) = 2.0
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Geometry"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // ParallaxOffset 수동 구현
            float2 ParallaxOffset(half h, half height, half3 viewDir)
            {
                h = h * height - height / 2.0;
                float3 v = normalize(viewDir);
                v.z += 0.42;
                return h * (v.xy / v.z);
            }

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float4 tangentOS    : TANGENT;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float3 viewDirTS    : TEXCOORD1;
                float3 normalWS     : TEXCOORD2;
                float4 tangentWS    : TEXCOORD3;
                float3 positionWS   : TEXCOORD4;
                float2 parallaxUV   : TEXCOORD5;
                float4 waveNoise    : TEXCOORD6;
            };

            TEXTURE2D(_MainTex);       SAMPLER(sampler_MainTex);
            TEXTURE2D(_BumpMap);       SAMPLER(sampler_BumpMap);
            TEXTURE2D(_BumpMap2);      SAMPLER(sampler_BumpMap2);
            TEXTURE2D(_ParallaxMap);   SAMPLER(sampler_ParallaxMap);
            TEXTURE2D(_WaveNoise);     SAMPLER(sampler_WaveNoise);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _BaseColor;
                float _ScrollSpeed;
                float _WaveFreq;
                float _WaveHeight;
                float _Parallax;
                float _Smoothness;
                float _Emis;
                float _BumpLerp;
                float _WaveSpeed;
                float _WaveScale;
                float _WaveDistortion;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                // 노이즈 기반 웨이브 계산
                float2 noiseUV = IN.uv * 4.0 + _TimeParameters.y * _WaveSpeed * 0.2;
                float4 noise = SAMPLE_TEXTURE2D_LOD(_WaveNoise, sampler_WaveNoise, noiseUV, 0);

                // 다층 웨이브
                float wave1 = sin((IN.positionOS.x * 0.5 + _TimeParameters.y * _WaveSpeed) * _WaveFreq);
                float wave2 = cos((IN.positionOS.z * 0.3 + _TimeParameters.y * _WaveSpeed * 0.8) * (_WaveFreq * 0.7));
                float wave3 = noise.r * 2.0 - 1.0;

                // 웨이브 합성 및 정점 왜곡
                IN.positionOS.y += (wave1 * 0.4 + wave2 * 0.3 + wave3 * 0.3) * _WaveHeight;
                IN.positionOS.xz += noise.gb * _WaveDistortion * _WaveScale;

                VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);

                float3x3 tangentToWorld = float3x3(
                    normalInputs.tangentWS,
                    normalInputs.bitangentWS,
                    normalInputs.normalWS
                );
                float3 viewDirWS = GetWorldSpaceViewDir(positionInputs.positionWS);
                OUT.viewDirTS = mul(transpose(tangentToWorld), viewDirWS);

                OUT.positionHCS = positionInputs.positionCS;
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.normalWS = normalInputs.normalWS;
                OUT.tangentWS = float4(normalInputs.tangentWS, IN.tangentOS.w);
                OUT.positionWS = positionInputs.positionWS;
                OUT.parallaxUV = IN.uv;
                OUT.waveNoise = noise;

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // 파라랙스 매핑
                float h = SAMPLE_TEXTURE2D(_ParallaxMap, sampler_ParallaxMap, IN.parallaxUV).r;
                float2 offset = ParallaxOffset(h, _Parallax, normalize(IN.viewDirTS));

                // 스크롤 UV 계산
                float2 timeOffset = float2(_TimeParameters.y * _ScrollSpeed, _TimeParameters.y * _ScrollSpeed * 0.5);
                float2 mainUV = IN.uv + offset + timeOffset;
                float2 bumpUV = IN.uv + offset;

                // 노멀맵 왜곡
                float2 distortion = (IN.waveNoise.gb * 0.2 + timeOffset) * _WaveScale;

                float3 normal1 = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, bumpUV + distortion), 1.0);
                float3 normal2 = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap2, sampler_BumpMap2, bumpUV - distortion), 1.0);

                float dynamicLerp = saturate(sin(_TimeParameters.y * 0.5) * 0.5 + _BumpLerp);
                float3 finalNormal = normalize(lerp(normal1, normal2, dynamicLerp));
                finalNormal.xy *= 1.0 + (IN.waveNoise.r * 0.3);
                finalNormal = normalize(finalNormal);

                float4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, mainUV);
                float4 albedo = tex * _BaseColor;

                InputData lightingInput = (InputData)0;
                lightingInput.positionWS = IN.positionWS;
                lightingInput.normalWS = TransformTangentToWorld(
                    finalNormal,
                    float3x3(IN.tangentWS.xyz, cross(IN.normalWS, IN.tangentWS.xyz) * IN.tangentWS.w, IN.normalWS)
                );
                lightingInput.viewDirectionWS = normalize(GetWorldSpaceViewDir(IN.positionWS));
                lightingInput.shadowCoord = TransformWorldToShadowCoord(IN.positionWS);

                SurfaceData surfaceInput = (SurfaceData)0;
                surfaceInput.albedo = albedo.rgb;
                surfaceInput.specular = 0.0;
                surfaceInput.metallic = 0.0;
                surfaceInput.smoothness = _Smoothness;
                surfaceInput.normalTS = finalNormal;
                surfaceInput.emission = albedo.rgb * _Emis;
                surfaceInput.alpha = albedo.a;

                return UniversalFragmentPBR(lightingInput, surfaceInput);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}
