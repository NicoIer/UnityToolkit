Shader "#NAME#"
{
    Properties
    {
        _BaseColor("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
        }
        Pass
        {
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag


            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"


            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };


            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
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
                return _BaseColor;
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Shader Graph/FallbackError"
}