Shader "#NAME#"
{
    /* Properties区域
     * 
     * Shaderlab 提供的一种用于在Inspector中显示Shader属性的机制
     * 通过Properties区域定义的属性，可以在Inspector中显示，并且可以在Shader中使用
     * 支持的类型查看Unity文档 https://docs.unity3d.com/Manual/SL-Properties.html
     * 
     * 如何定义
     * 属性名("Inspector显示名", 类型) = "默认值" { }
     * 
     * 如何与HLSL关联 
     * 需要在HLSL中声明一个同名的变量，这样Unity会自动将Inspector中的属性值赋值给HLSL中的变量
     */

    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}// 主贴图
    }
    /* SubShader是Shader的主要部分，包含了一系列的Pass
     * Pass是渲染管线的一个阶段，包含了一个顶点着色器和一个片元着色器
     * 可能存在多个SubShader，Unity会选择当前环境可用的第一个SubShader
     */
    SubShader
    {
        /* Tag 是一个键值对，它的作用是告诉渲染引擎，应该 什么时候 怎么样 去渲染 
         * https://docs.unity.cn/cn/2022.3/Manual/SL-SubShaderTags.html
         */
        Tags
        {
            "RenderType"="Opaque"
            "RenderPipeLine"="UniversalPipeline" //用于指明使用URP来渲染
        }
        /* 通用区域 这里内容可以在多个Pass中共享 */

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"


        // 支持SRP Batcher
        CBUFFER_START(UnityPerMaterial)
            //声明变量
            float4 _MainTex_ST;
        CBUFFER_END

        TEXTURE2D(_MainTex); // 声明纹理  
        SAMPLER(sampler_MainTex); // 声明纹理采样器

        /* 渲染管线整个流水线都有数据的输入和输出
         * 这些数据是什么? 从哪里输入? 输出到哪里? Shader中的变量如何与这些数据交互?
         */

        /* 常用种类
         * POSITION: 顶点位置
         * NORMAL: 顶点法线
         * TANENT: 顶点切线
         * COLOR: 顶点颜色
         * TXECOORD0: 纹理坐标 UV0 -(UV是一张图 可以有多个通道 UV0表示第一个通道)
         * TEXCOORD1: 纹理坐标 UV1
         * TEXCOORD2: 纹理坐标 UV2
         * TEXCOORD3: 纹理坐标 UV3
         *
         *
         * 输入输出
         * 数据存放在寄存器中，输入从寄存器中读取，输出写入寄存器
         *
         *
         * Shader中的变量如何与这些数据交互
         * 语义绑定
         * void xxFunc(xxxType xxName : XXX) -(作函数参数标记)
         * xxxType xxFunc() : XXX -(作函数返回值标记)
         * struct xxStruct{ 
         *   xxxType xxName : XXX; -(作结构体成员标记)
         * }
         * 输入输出怎么表示
         * in -> 输入
         * out -> 输出
         * inout -> 输入输出
         */

        // a2v -> attribute to vertex
        struct a2v
        {
            /* POSITION是一个系统值语义，它表示一个顶点的对象空间位置。
             * 在3D图形中，对象空间是一个局部坐标系，它是相对于一个特定的对象的坐标系。
             * 对象空间的原点通常位于对象的中心，坐标轴沿着对象的主要方向。
             * 因此，对象空间的位置是相对于对象的原点的位置。
             */
            float4 positionOS: POSITION;
            /* NORMAL是一个系统值语义，它表示一个顶点的对象空间法线。
             * 法线是一个垂直于表面的向量，它指示表面的朝向。
             * 法线通常用于计算光照，因为光照取决于表面的朝向。
             */

            /* TANGENT是一个系统值语义，它表示一个顶点的对象空间切线。
             * 切线是一个沿着表面的向量，它指示表面的切线方向。
             * 切线通常用于计算法线贴图，因为法线贴图取决于表面的切线方向。
             */
            
            /* COLOR是一个系统值语义，它表示一个顶点的颜色。
             * 颜色是一个RGBA值，它表示顶点的颜色。
             * 颜色通常用于在顶点着色器中对顶点进行着色。
             */
            half4 vertexColor: COLOR;
            /* TEXCOORD0是一个系统值语义，它表示一个顶点的纹理坐标。
             * 纹理坐标是一个UV值，它表示纹理的坐标。
             * 纹理坐标通常用于在顶点着色器中对纹理进行采样。
             */
            float2 uv : TEXCOORD0;
        };

        // v2f -> vertex to fragment
        struct v2f
        {
            /* SV -> System Value
             * 
             */
            /* 在HLSL（High-Level Shader Language）中，SV_POSITION是一个系统值语义，
             * 它表示一个顶点的裁剪空间位置。这是一个特殊的语义，因为它是唯一一个可以用于顶点着色器输出和像素着色器输入的系统值语义。
             * 在3D图形中，裁剪空间是一个中间步骤，用于将3D世界坐标转换为2D屏幕坐标。
             * 在裁剪空间中，所有可见的几何体都位于一个立方体内，该立方体的范围在所有坐标轴上都是[-1, 1]。
             * 然后，这个立方体被映射到视口，即最终的2D屏幕空间。
             * 因此，SV_POSITION通常用于顶点着色器的输出结构和像素着色器的输入结构，以传递顶点的裁剪空间位置。
             */
            float4 positionCS: SV_POSITION;
            /* TEXCOORD0是一个系统值语义，它表示一个顶点的纹理坐标。
             * 纹理坐标是一个UV值，它表示纹理的坐标。
             * 纹理坐标通常用于在顶点着色器中对纹理进行采样。
             */
            float2 uv: TEXCOORD0;
            /* COLOR是一个系统值语义，它表示一个顶点的颜色。
             * 颜色是一个RGBA值，它表示顶点的颜色。
             * 颜色通常用于在顶点着色器中对顶点进行着色。
             */
            half4 vertexColor: COLOR;
        };
        ENDHLSL

        Pass
        {
            // Pass的标签
            Tags {}
            HLSLPROGRAM
            // 编译指令 
            #pragma vertex vert // vertex shader 的函数 是 vert
            #pragma fragment frag // fragment shader 的函数 是 frag


            v2f vert(a2v v)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.vertexColor = v.vertexColor;
                return o;
            }

            half4 frag(v2f i) : SV_Target /* 注意在HLSL中，fixed4类型变成了half4类型*/
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                half res = lerp(i.vertexColor, col, i.vertexColor.g).x;
                return half4(res, res, res, 1.0);
            }
            ENDHLSL
        }
    }

    /* Fallback是一个备用Shader，当当前Shader不支持时，会使用Fallback指定的Shader
     * 通常是Unity内置的Shader
     */
//    Fallback "Universal Render Pipeline/Unlit"
}