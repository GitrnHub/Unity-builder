Shader "Hidden/AI/SunShafts"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            Name "SunShafts"

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            #pragma vertex Vert
            #pragma fragment Frag

            TEXTURE2D_X(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);

            float4 _SunViewport;
            half4 _SunColor;
            float _Intensity;
            float _Density;
            float _Decay;
            float _Weight;
            float _Exposure;

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                const int kSamples = 32;
                float2 uv = input.texcoord;
                half4 baseColor = SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv);

                float2 toSun = _SunViewport.xy - uv;
                float2 stepUv = toSun * (_Density / (float)kSamples);
                float2 sampleUv = uv;
                float illuminationDecay = 1.0;
                float scattering = 0.0;

                [unroll]
                for (int i = 0; i < kSamples; i++)
                {
                    sampleUv += stepUv;
                    bool inBounds = sampleUv.x >= 0.0 && sampleUv.x <= 1.0 && sampleUv.y >= 0.0 && sampleUv.y <= 1.0;
                    if (inBounds)
                    {
                        float rawDepth = SampleSceneDepth(sampleUv);
                        float linearDepth = Linear01Depth(rawDepth, _ZBufferParams);
                        // Only unobstructed sky contributes to the radial scattering.
                        // Terrain, trunks and leaf silhouettes therefore cut visible shafts.
                        float skyVisibility = smoothstep(0.982, 0.9995, linearDepth);
                        scattering += skyVisibility * illuminationDecay * _Weight;
                    }
                    illuminationDecay *= _Decay;
                }

                float sunDistance2 = dot(toSun, toSun);
                float halo = exp(-sunDistance2 * 24.0) * 0.16;
                float rays = (scattering + halo) * _Exposure * _Intensity;

                float currentRawDepth = SampleSceneDepth(uv);
                float currentLinearDepth = Linear01Depth(currentRawDepth, _ZBufferParams);
                float atmosphericPerspective = smoothstep(0.45, 0.995, currentLinearDepth) * 0.055 * _Intensity;
                half3 hazeColor = half3(0.48h, 0.61h, 0.74h);

                half3 result = lerp(baseColor.rgb, hazeColor, atmosphericPerspective);
                result += _SunColor.rgb * rays;
                return half4(result, baseColor.a);
            }
            ENDHLSL
        }
    }
}
