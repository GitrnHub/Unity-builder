Shader "AI/GodRayURP"
{
    Properties
    {
        _Color("Ray Color", Color) = (1, 0.86, 0.58, 0.07)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent+20" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha One
        ZWrite Off
        ZTest LEqual
        Cull Off

        Pass
        {
            Name "GodRay"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positions = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionHCS = positions.positionCS;
                output.positionWS = positions.positionWS;
                output.uv = input.uv;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float edge = saturate(1.0 - abs(input.uv.x * 2.0 - 1.0));
                edge = edge * edge * (3.0 - 2.0 * edge);
                float startFade = smoothstep(0.0, 0.08, input.uv.y);
                float endFade = 1.0 - smoothstep(0.72, 1.0, input.uv.y);
                float noise = 0.84 + 0.16 * sin(dot(input.positionWS.xz, float2(0.13, 0.19)) + _Time.y * 0.12);
                half alpha = (half)(edge * startFade * endFade * noise) * _Color.a;
                return half4(_Color.rgb, alpha);
            }
            ENDHLSL
        }
    }
}
