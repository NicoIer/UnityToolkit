Shader "#NAME#"

{

    Properties
    {

        _MainTex ("Texture", 2D) = "white" {}

        _StencilComp ("Stencil Comparison", Float)=8
        _Stencil ("Stencil ID", Float)=0
        _StencilOp ("Stencil Operation", Float)=0
        _StencilWriteMask ("Stencil Write Mask", Float)=255
        _StencilReadMask ("Stencil Read Mask", Float)=255
        _ColorMask ("Color Mask", Float)=15

    }

    SubShader

    {

        Tags
        {
            "Queue"="Transparent"
        }

        Pass
        {
            Stencil
            {
                Ref [_Stencil]
                Comp [_StencilComp]
                Pass [_StencilOp]
                ReadMask [_StencilReadMask]
                WriteMask [_StencilWriteMask]
            }
            ColorMask [_ColorMask]

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex); // 声明纹理  
            SAMPLER(sampler_MainTex); // 声明纹理采样器


            CBUFFER_START(UnityPerMaterial)
                //声明变量
                float4 _MainTex_ST;
            CBUFFER_END

            #pragma vertex vert;
            #pragma fragment frag;

            struct a2v
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(a2v v)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 frag(v2f i) : SV_Target /* 注意在HLSL中，fixed4类型变成了half4类型*/
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                return col;
            }
            ENDHLSL


        }
    }
}