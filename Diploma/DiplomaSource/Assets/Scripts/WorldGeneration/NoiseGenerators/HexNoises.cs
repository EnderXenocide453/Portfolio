using System.Collections.Generic;
using UnityEngine;

public static class HexNoises
{
    //Применение искаженного шума Перлина к сетке ячеек
    public static void GenerateDistortedPerlinNoise(Dictionary<Vector2Int, HexChunk> chunks, Vector2 offset, float scale, float heightMultiplier, int octavesCount = 1, float H = 1)
    {
        foreach (HexChunk chunk in chunks.Values) {
            
            for (int i = 0; i < chunk.Vertices.Length; i++) {
                Vector3 pos = (chunk.Offset + chunk.Vertices[i].LocalPos) / VerticesContainer.GetContainer().ChunkRadius / scale;
                chunk.Vertices[i].Height += DistortedPerlin(new Vector2(pos.x, pos.z) + offset, H, octavesCount) * heightMultiplier;
            }
        }
    }
    //Применение шума Перлина к сетке ячеек
    public static void GeneratePerlinNoise(Vector2 offset, float scale, int octavesCount = 1, float persistance = 1, float lacunarity = 1)
    {
        offset += new Vector2(VerticesContainer.Random.Next(-100000, 100000), VerticesContainer.Random.Next(-100000, 100000));

        float minHeight = float.MaxValue, maxHeight = float.MinValue;
        Dictionary<Vector2Int, float> heightMap = new Dictionary<Vector2Int, float>();

        if (scale == 0) scale = 0.0001f;
        VerticesContainer container = VerticesContainer.GetContainer();

        foreach (Vector2Int pos in container.Vertices.Keys) {
            Vector3 vertPos = container.Vertices[pos][0].GetGlobalPosition();
            float amplitude = 1;
            float frequency = 1;
            float noiseHeight = 0;

            for (int j = 0; j < octavesCount; j++) {
                float perlinValue = Mathf.PerlinNoise(vertPos.x / scale * frequency + offset.x, vertPos.z / scale * frequency + offset.y);
                noiseHeight += perlinValue * amplitude;

                amplitude *= persistance;
                frequency *= lacunarity;
            }

            if (minHeight > noiseHeight) minHeight = noiseHeight;
            if (maxHeight < noiseHeight) maxHeight = noiseHeight;

            heightMap.Add(pos, noiseHeight);
            
        }

        foreach (Vector2Int pos in container.Vertices.Keys) {
            Mathf.Lerp(minHeight, maxHeight, heightMap[pos]);
            container.SetVertexHeight(pos, heightMap[pos]);
        }
    }

    //Генерация формы острова
    public static void GenerateLand(Vector2 offset, float landScale, float oceanMaxValue, float distanceInfluence)
    {
        //Определение максимального радиуса
        float radius = VerticesContainer.GetContainer().MapRadius * VerticesContainer.GetContainer().ChunkRadius * 1.5f;
        //Определение шага для вычисления наклона
        float step = distanceInfluence / radius;

        float minHeight = float.MaxValue, maxHeight = float.MinValue;
        Dictionary<Vector2Int, float> values = new Dictionary<Vector2Int, float>();
        VerticesContainer container = VerticesContainer.GetContainer();

        foreach (Vector2Int pos in container.Vertices.Keys) {
            Vector3 vertPos = container.Vertices[pos][0].GetGlobalPosition();
            Vector3 offsetPos = vertPos + new Vector3(offset.x, 0, offset.y);

            //Нахождение значения высоты умножением шума Перлина на уклон
            float value = Mathf.PerlinNoise(offsetPos.x / landScale, offsetPos.z / landScale) * (radius - vertPos.magnitude) * step * container.Vertices[pos][0].Height * 2 - 1;

            values.Add(pos, value);
            if (value > maxHeight) maxHeight = value;
            if (value < minHeight) minHeight = value;
        }

        foreach (Vector2Int pos in container.Vertices.Keys) {
            values[pos] = Mathf.InverseLerp(minHeight, maxHeight, values[pos]);
            container.SetVertexHeight(pos, values[pos], values[pos] < oceanMaxValue ? 1 : 0);
        }
    }

    public static void GenerateMountainsLayer(Dictionary<Vector2Int, HexChunk> chunks, Vector2 offset, float scale, float H, int octaves, float materialMinHeight, float heightMultiplier, float roughScale, float roughMultiplier, AnimationCurve mountainsCurve)
    {
        float radius = VerticesContainer.GetContainer().MapRadius * VerticesContainer.GetContainer().ChunkRadius;

        foreach (HexChunk chunk in chunks.Values) {
            Vector3 pos = chunk.Offset + new Vector3(offset.x, 0, offset.y);

            for (int i = 0; i < chunk.Vertices.Length; i++) {

                if (chunk.Vertices[i].Height > 0) {
                    Vector3 vertPos = (pos + chunk.Vertices[i].LocalPos) / VerticesContainer.GetContainer().ChunkRadius;
                    //Коэффициент снижения склона. чем ниже изначальная высота рельефа, тем ниже гора. Если значение выше 1 - materialMinHeight, то высота горы неизменна. Если ниже 0, то горы нет
                    
                    float value = (mountainsCurve.Evaluate(DistortedPerlin(new Vector2(vertPos.x, vertPos.z) / scale, H, octaves)));

                    if (value >= 0) {
                        chunk.Vertices[i].Height += value * heightMultiplier;
                    }
                }
            }
        }
    }

    //Дробное броуновское движение шума Перлина
    public static float FBM2D(Vector2 position, float scale, int octaves, float lacunarity, float gain)
    {
        float value = 0;
        float amplitude = 1;
        float frequency = 1;

        for (int i = 0; i < octaves; i++) {
            value += Mathf.PerlinNoise(position.x / scale * frequency, position.y / scale * frequency) * amplitude;
            amplitude *= gain;
            frequency *= lacunarity;
        }

        return value;
    }

    //Дробное броуновское движение
    public static float FBM(Vector2 pos, float H, int octaves) { 
        float G = Mathf.Exp(-H); 
        float f = 1; 
        float a = 1; 
        float t = 0; 

        for (int i = 0; i < octaves; i++) { 
            t += a * Mathf.PerlinNoise(pos.x * f, pos.y * f); 
            f *= 2; 
            a *= G; 
        } 

        return t; 
    }

    //Искаженный шум Перлина
    public static float DistortedPerlin(Vector2 position, float H, int octaves, float distortionPower = 1)
    {
        Vector2 q = new Vector2(FBM(position, H, octaves),
                                FBM(position + new Vector2(5.2f, 1.3f), H, octaves));

        return FBM(position + distortionPower * q, H, octaves);
    }
}
