using System;
using UnityEngine;
using Random = System.Random;

namespace Nico.Algorithm
{
    public enum NoiseType
    {
        PerlinNoise
    }

    public static class Noise
    {
        public static float[,] Generator(NoiseType noiseType, int mapWidth, int mapHeight, int seed, float scale,
            int octaves,
            float persistance, float lacunarity, Vector2 offset)
        {
            switch (noiseType)
            {
                case NoiseType.PerlinNoise:
                    return GeneratePerlinMap(mapWidth, mapHeight, seed, scale, octaves, persistance, lacunarity,
                        offset);
                default:
                    throw new ArgumentOutOfRangeException(nameof(noiseType), noiseType, null);
            }
        }

        /// <summary>
        /// 标准柏林噪声地图
        /// </summary>
        /// <param name="mapWidth">地图宽</param>
        /// <param name="mapHeight">地图高</param>
        /// <param name="seed">随机种子</param>
        /// <param name="scale">噪声图大小</param>
        /// <param name="octaves">噪声八音度</param>
        /// <param name="persistance">噪声清晰度</param>
        /// <param name="lacunarity">噪声图间隙</param>
        /// <param name="offset">补偿修正</param>
        /// <returns>返回噪声图</returns>
        public static float[,] GeneratePerlinMap(int mapWidth, int mapHeight, int seed, float scale, int octaves,
            float persistance, float lacunarity, Vector2 offset)
        {
            float[,] noiseMap = new float[mapWidth, mapHeight];

            Random prng = new Random(seed);
            Vector2[] octaveOffsets = new Vector2[octaves];
            for (int i = 0; i < octaves; i++)
            {
                float offsetX = prng.Next(-100000, 100000) + offset.x;
                float offsetY = prng.Next(-100000, 100000) + offset.y;
                octaveOffsets[i] = new Vector2(offsetX, offsetY);
            }

            if (scale <= 0)
            {
                scale = 0.0001f; //约束scale最小值
            }

            float maxNoiseHeight = float.MinValue;
            float minNoiseHeight = float.MaxValue;

            float halfWidth = mapWidth / 2f;
            float halfHeight = mapHeight / 2f;

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    float amplitude = 1;
                    float frequency = 1;
                    float noiseHeight = 0;
                    for (int i = 0; i < octaves; i++)
                    {
                        float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
                        float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;

                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                        noiseHeight += perlinValue * amplitude;
                        amplitude *= persistance;
                        frequency *= lacunarity;
                    }

                    if (noiseHeight > maxNoiseHeight)
                    {
                        maxNoiseHeight = noiseHeight;
                    }
                    else if (noiseHeight < minNoiseHeight)
                    {
                        minNoiseHeight = noiseHeight;
                    }

                    noiseMap[x, y] = noiseHeight;
                }
            }

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
                }
            }

            return noiseMap;
        }

        public static float[,] GeneratorNoise(float noiseScale, NoiseType type, int width, int height,
            Vector2 offset,
            int seed)
        {
            if (type == NoiseType.PerlinNoise)
            {
                float[,] noiseMap = new float[width, height];
                var prng = new Random(seed);
                float xOffset = prng.Next(-100000, 100000) + offset.x;
                float yOffset = prng.Next(-100000, 100000) + offset.y;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        float noiseValue = Mathf.PerlinNoise(x * noiseScale + xOffset, y * noiseScale + yOffset);
                        noiseMap[x, y] = noiseValue;
                    }
                }

                return noiseMap;
            }

            throw new NotImplementedException($"noise type:{type} has not been imple");
        }

        public static Texture2D TextureFromColorMap(Color[] colorMap, int width, int height)
        {
            Texture2D texture = new Texture2D(width, height)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixels(colorMap);
            texture.Apply();
            return texture;
        }
    }
}