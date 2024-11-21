Shader "Hidden/CustomRP/StencilBuffer"
{
    Properties
    {
        _StencilRef("Stencil Ref", Range(0, 255)) = 2
        _ColorValue("Color Value", Range(0, 1)) = 1
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
        }
        Pass
        {
            Name "DrawStencilBuffer"

            // Render State
            Cull Off
            Blend Off
            ZTest Off
            ZWrite Off
            Stencil
            {
                Ref [_StencilRef]
                Comp Equal
                Pass Keep
            }


            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag


            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"


            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };


            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
                int _StencilRef;
                half _ColorValue;
            CBUFFER_END


            Varyings vert(Attributes input)
            {
                Varyings output;
                float2 uv = float2((input.vertexID << 1) & 2, input.vertexID & 2);
                output.positionCS = float4(uv * 2.0 - 1.0, UNITY_NEAR_CLIP_VALUE, 1.0);
                return output;
            }

            half4 frag(Varyings packedInput) : SV_Target
            {
                return half4(_ColorValue, 0, 0, 0);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Shader Graph/FallbackError"
}