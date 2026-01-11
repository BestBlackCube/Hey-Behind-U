Shader "Flooded_Grounds/Triplanar_BumpSpec_URP"
{
    Properties
    {
        _TexScale ("Tex Scale", Range (0.1, 10.0))= 1.0
        _BlendPlateau ("BlendPlateau", Range (0.0, 1.0)) = 0.2       
        _MainTex ("Base 1 (RGB) Gloss(A)", 2D) = "white" {}
        _BumpMap1 ("NormalMap 1 (_Y_X)", 2D)  = "bump" {}   
        
        _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
   
    SubShader
    {
        Tags {
            "Queue"="Geometry" 
            "IgnoreProjector"="True" 
            "RenderType"="Opaque"
            "RenderPipeline"="UniversalPipeline"
        }
        ZWrite On
        LOD 400
 
        Pass
        {
            Name "ForwardLit"
            Tags {"LightMode" = "UniversalForward"}

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            // Properties
            CBUFFER_START(UnityPerMaterial)
                half _TexScale;
                half _BlendPlateau;
                half _Cutoff;
            CBUFFER_END
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_BumpMap1);
            SAMPLER(sampler_BumpMap1);
            
            // 구조체 정의
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float4 color : COLOR;
            };
            
            // 버텍스 함수 - 원래 셰이더의 vertLocal과 유사
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.color = input.color;
                
                return output;
            }
            
            // 프래그먼트 함수 - 원래 셰이더의 surf와 유사
            half4 frag(Varyings input) : SV_Target
            {
                // 노멀 정규화
                float3 normalWS = normalize(input.normalWS);
                
                // 3개의 평면 투영에 대한 블렌드 가중치 결정
                half3 blend_weights = abs(normalWS.xyz);
                
                // 블렌딩 영역 조정
                blend_weights = (blend_weights - _BlendPlateau);
                blend_weights = max(blend_weights, 0);
                // 가중치 합이 1이 되도록 강제(매우 중요)
                blend_weights /= (blend_weights.x + blend_weights.y + blend_weights.z).xxx;  
                       
                // 3개의 투영에 대한 색상값과 범프 벡터를 결정하고 블렌드
                half4 blended_color;
                half3 blended_bumpvec;
                         
                // 3개의 평면 투영에 대한 UV 좌표 계산
                half2 coord1 = input.positionWS.yz * _TexScale;  
                half2 coord2 = input.positionWS.zx * _TexScale;  
                half2 coord3 = input.positionWS.xy * _TexScale;  

                // 각 투영에 대한 색상 맵 샘플링
                half4 col1 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, coord1);
                half4 col2 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, coord2);
                half4 col3 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, coord3);

                // 범프 맵 샘플링 및 범프 벡터 생성
                half2 bumpVec1 = SAMPLE_TEXTURE2D(_BumpMap1, sampler_BumpMap1, coord1).wy * 2 - 1;  
                half2 bumpVec2 = SAMPLE_TEXTURE2D(_BumpMap1, sampler_BumpMap1, coord2).wy * 2 - 1;  
                half2 bumpVec3 = SAMPLE_TEXTURE2D(_BumpMap1, sampler_BumpMap1, coord3).wy * 2 - 1; 

                half3 bump1 = half3(0, bumpVec1.x, bumpVec1.y);  
                half3 bump2 = half3(bumpVec2.y, 0, bumpVec2.x);  
                half3 bump3 = half3(bumpVec3.x, bumpVec3.y, 0);

                // 3개의 평면 투영 결과 블렌딩
                blended_color = col1.xyzw * blend_weights.xxxx +  
                               col2.xyzw * blend_weights.yyyy +  
                               col3.xyzw * blend_weights.zzzz;  
                 
                blended_bumpvec = bump1.xyz * blend_weights.xxx +  
                                 bump2.xyz * blend_weights.yyy +  
                                 bump3.xyz * blend_weights.zzz;  
               
                half4 finalColor = blended_color;
                
                // 버텍스 컬러 적용
                finalColor.rgb *= input.color.rgb;
                
                // 알파 컷오프
                clip(finalColor.a - _Cutoff);
                
                // 최종 노멀 계산
                half3 finalNormal = normalize(half3(0,0,1) + blended_bumpvec.xyz);
                
                // 메인 라이트 가져오기
                Light mainLight = GetMainLight();
                
                // Lambert 디퓨즈 계산
                half NdotL = saturate(dot(finalNormal, mainLight.direction));
                half3 diffuse = mainLight.color * NdotL * finalColor.rgb;
                
                // 앰비언트 라이팅 계산 (구면 조화함수 사용)
                half3 ambient = SampleSH(finalNormal) * finalColor.rgb;
                
                // 라이팅 결합
                half3 finalLighting = ambient + diffuse;
                
                return half4(finalLighting, finalColor.a);
            }
            ENDHLSL
        }
        
        // 그림자 캐스팅 패스가 필요한 경우 여기에 추가
    }
    FallBack "Universal Render Pipeline/Lit"
}
