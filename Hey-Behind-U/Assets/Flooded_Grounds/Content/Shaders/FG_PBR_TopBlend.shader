Shader "Flooded_Grounds/PBR_TopBlend_URP" {
    Properties{
        _MainTex("Base Albedo (RGB)", 2D) = "white" {}
        _Spc("Base Metalness(R) Smoothness(A)", 2D) = "black" {}
        _BumpMap("Base Normal", 2D) = "bump" {}
        _AO("Base AO", 2D) = "white" {}
        _layer1Tex("Layer1 Albedo (RGB) Smoothness (A)", 2D) = "white" {}
        _layer1Metal("Layer1 Metalness", Range(0,1)) = 0
        _layer1Norm("Layer 1 Normal", 2D) = "bump" {}
        _layer1Breakup("Layer1 Breakup (R)", 2D) = "white" {}
        _layer1BreakupAmnt("Layer1 Breakup Amount", Range(0,1)) = 0.5
        _layer1Tiling("Layer1 Tiling", float) = 10
        _Power("Layer1 Blend Amount", float) = 1
        _Shift("Layer1 Blend Height", float) = 1
        _DetailBump("Detail Normal", 2D) = "bump" {}
        _DetailInt("DetailNormal Intensity", Range(0,1)) = 0.4
        _DetailTiling("DetailNormal Tiling", float) = 2
    }
        SubShader{
            Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
            Pass {
                Name "ForwardLit"
                Tags { "LightMode" = "UniversalForward" }
                HLSLPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_fog

            // Include URP shader libraries
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // Textures & samplers
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_Spc); SAMPLER(sampler_Spc);
            TEXTURE2D(_BumpMap); SAMPLER(sampler_BumpMap);
            TEXTURE2D(_AO); SAMPLER(sampler_AO);
            TEXTURE2D(_layer1Tex); SAMPLER(sampler_layer1Tex);
            TEXTURE2D(_layer1Norm); SAMPLER(sampler_layer1Norm);
            TEXTURE2D(_layer1Breakup); SAMPLER(sampler_layer1Breakup);
            TEXTURE2D(_DetailBump); SAMPLER(sampler_DetailBump);

            float _layer1Metal;
            float _layer1BreakupAmnt;
            float _layer1Tiling;
            float _Power;
            float _Shift;
            float _DetailInt;
            float _DetailTiling;

            // Uniform a főfény irányához (URP által átadva)
            float4 _MainLightDirection;
            // _MainLightColor is provided by URP

            struct Attributes {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 uvLayer1 : TEXCOORD1;
                float2 uvDetail : TEXCOORD2;
            };

            Varyings vert(Attributes IN) {
                Varyings OUT;
                OUT.pos = TransformObjectToHClip(IN.vertex);
                OUT.uv = IN.uv;
                OUT.uvLayer1 = IN.uv * _layer1Tiling;
                OUT.uvDetail = IN.uv * _DetailTiling;
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target {
                // Mintázd a base textúrákat
                float3 baseAlbedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv).rgb;
                float4 baseSpec = SAMPLE_TEXTURE2D(_Spc, sampler_Spc, IN.uv);
                float3 baseNormal = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, IN.uv));
                float ao = SAMPLE_TEXTURE2D(_AO, sampler_AO, IN.uv).r;

                // Layer1 textúrák (tile-elt UV)
                float4 layer1Albedo = SAMPLE_TEXTURE2D(_layer1Tex, sampler_layer1Tex, IN.uvLayer1);
                float3 layer1Normal = UnpackNormal(SAMPLE_TEXTURE2D(_layer1Norm, sampler_layer1Norm, IN.uvLayer1));
                float layer1Breakup = SAMPLE_TEXTURE2D(_layer1Breakup, sampler_layer1Breakup, IN.uvLayer1).r;

                // Detail normál
                float3 detailNormal = UnpackNormal(SAMPLE_TEXTURE2D(_DetailBump, sampler_DetailBump, IN.uvDetail));

                // Blend factor számítása (normál keverés alapján)
                float3 modNormal = baseNormal + float3(layer1Normal.r * 0.6, layer1Normal.g * 0.6, 0);
                float3 layer1Dir = float3(0, 1, 0);
                float blend = dot(normalize(modNormal), layer1Dir);
                float blendFactor = (blend * _Power + _Shift) * lerp(1.0, layer1Breakup, _layer1BreakupAmnt);
                blendFactor = saturate(pow(blendFactor, 3.0));

                // Normálok keverése
                float3 blendedNormal = lerp(baseNormal, layer1Normal, blendFactor);
                blendedNormal += float3(detailNormal.xy * _DetailInt, 0);
                blendedNormal = normalize(blendedNormal);

                // Albedo rétegek keverése
                float3 blendedAlbedo = lerp(baseAlbedo, layer1Albedo.rgb, blendFactor);

                // Smoothness és metallic értékek interpolálása (további PBR feldolgozáshoz)
                float smoothness = lerp(baseSpec.a, layer1Albedo.a, blendFactor);
                float metallic = lerp(baseSpec.r, _layer1Metal, blendFactor);

                // Egyszerű megvilágítás:
                float3 lightDir = normalize(_MainLightDirection.xyz);
                float NdotL = saturate(dot(blendedNormal, lightDir));
                float3 diffuse = blendedAlbedo * _MainLightColor.xyz * NdotL;
                float3 ambient = blendedAlbedo * 0.2; // fixált ambient
                float3 colorLit = (diffuse + ambient) * ao;

                return float4(colorLit, 1.0);
            }
            ENDHLSL
        }
        }
            FallBack "Universal Forward"
}
