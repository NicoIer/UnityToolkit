Shader "Hidden/UnityToolkit/UIDepthOccluder"
{
    Properties
    {
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "UIDepthOccluder"

            ColorMask 0
            ZWrite On
            ZTest Always
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };


            Varyings vert(Attributes input)
            {
                Varyings output;
                // UNITY_MATRIX_M = localToViewport matrix from cmd.DrawMesh
                // Transforms mesh [0,1] local space -> viewport [0,1] space
                float3 viewportPos = mul(UNITY_MATRIX_M, float4(input.positionOS.xyz, 1.0)).xyz;

                // Viewport [0,1] -> clip space [-1,1]
                float2 clipXY = viewportPos.xy * 2.0 - 1.0;

                #if UNITY_UV_STARTS_AT_TOP
                clipXY.y = -clipXY.y;
                #endif

                output.positionCS = float4(clipXY, UNITY_NEAR_CLIP_VALUE, 1.0);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return half4(1, 1, 1, 1);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
