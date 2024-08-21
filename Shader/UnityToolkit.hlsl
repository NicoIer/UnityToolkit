#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityInput.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
// URP下的环境光颜色
// half3 AambientColor()
// {
//     return _GlossyEnvironmentColor.rgb;
//     // return half3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);
// }

// 兰伯特漫反射光照模型
half LambertDiffuse(float3 normal, float3 lightDir)
{
    // 点乘的意义是计算光线和法线的夹角
    // dot(A, B) = |A| * |B| * cosθ
    // saturate函数的作用是将值限制在0-1之间
    // cosθ的值域是[-1, 1]，所以需要使用saturate函数将值限制在[0, 1]之间
    // 负数不满足光照模型，所以需要将负数转换为0
    return saturate(dot(normal, lightDir));
}

half HalfLambdaDiffuse(float3 normal, float3 lightDir)
{
    return saturate(dot(normal, lightDir) * 0.5 + 0.5);
}

// // 讲一个物体空间的法线转换为世界空间的法线
// half3 ObjectToWorldNormal(float3 normalOS)
// {
//     // unity_WorldToObject是一个float4x4矩阵，表示从世界坐标系到物体坐标系的转换矩阵
//     // 为什么是右乘? 1*3 * 3*3 = 1*3 只有右乘才能保证矩阵乘法的正确性
//     return normalize(mul(normalOS, (float3x3)unity_WorldToObject));
// }