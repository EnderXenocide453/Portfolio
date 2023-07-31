using System;
using UnityEngine;

public static class AdditionalNoises
{
    const float MinTemp = 0;
    const float MaxTemp = 100;
    const float MinWet = 0;
    const float MaxWet = 100;

    public static float[,] GenerateFalloffNoise(Vector2Int size, float flatnessRadius)
    {
        if (flatnessRadius >= 1) flatnessRadius = 1;
        float[,] map = new float[size.x, size.y];

        for (int i = 0; i < size.x; i++) {
            for (int j = 0; j < size.y; j++) {
                float x = i / (float)size.x * 2 - 1;
                float y = j / (float)size.y * 2 - 1;

                map[i, j] = Mathf.InverseLerp(flatnessRadius, 1, Mathf.Clamp(Mathf.Max(Mathf.Abs(x), Mathf.Abs(y)), flatnessRadius, 1));
            }
        }

        return map;
    }

    public static float[,] GenerateTemperatureMap(float[,] heightMap, int seed, float scale, float heightTempMultiplier, float heightTempLowerBound)
    {
        System.Random random = new System.Random(seed);

        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        float[,] tempMap = new float[width, height];

        float min = float.MaxValue;
        float max = float.MinValue;

        Vector2 offset = new Vector2(random.Next(-100000, 100000), random.Next(-100000, 100000));

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++) {
                tempMap[x, y] = ((heightMap[x, y] >= heightTempLowerBound) ? (heightMap[x, y] - heightTempLowerBound) * heightTempMultiplier : 0) + Mathf.PerlinNoise((float)x / scale + offset.x, (float)y / scale + offset.y);

                if (tempMap[x, y] < min) min = tempMap[x, y];
                if (tempMap[x, y] > max) max = tempMap[x, y];
            }

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                tempMap[x, y] = Mathf.InverseLerp(min, max, tempMap[x, y]);

        return tempMap;
    }

    public static float[,] GenerateWetMap(Vector2Int size, int seed, float scale)
    {
        System.Random random = new System.Random(seed + 1);

        float[,] wetMap = new float[size.x, size.y];
        Vector2 offset = new Vector2(random.Next(-100000, 100000), random.Next(-100000, 100000));

        for (int x = 0; x < size.x; x++)
            for (int y = 0; y < size.y; y++)
                wetMap[x, y] = Mathf.PerlinNoise(offset.x + (float)x / scale, offset.y + (float)y / scale);

        return wetMap;
    }
}
