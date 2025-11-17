Shader "Hidden/CustomRP/StencilOnly"
{
    Properties
    {
        [IntRange] _StencilID ("Stencil ID", Range(0, 255)) = 2
    }

    SubShader
    {
        // Tags { "RenderType"="Opaque" "Queue"="Geometry-1" "RenderPipeline" = "UniversalPipeline"}

        Pass 
        {
            Blend Zero One
            ZWrite Off

            Stencil
            {
                Ref [_StencilID]
                Comp Always
                Pass Replace
            }
        }
    }
}