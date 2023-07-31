using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PerlinNoise
{
    // Генерация шума посредством генерации шумов Перлина в несколько октав и их совмещения
    // persistance - стойкость. Определяет насколько сильно будет меняться влияние каждой октавы на конечную карту путём изменения амплитуды шума
    // lacunarity - мера неоднородности. Определяет изменение частоты каждой октавы
    public static float[,] GenerateNoiseMap(Vector2Int mapSize, float scale, int octaves, float persistance, float lacunarity, int seed, Vector2 offset, float distortion)
    {
        float[,] noiseMap = new float[mapSize.x, mapSize.y];

        System.Random noiseRandom = new System.Random(seed);
        Vector2[] octaveOffset = new Vector2[octaves];
        for (int i = 0; i < octaves; i++) {
            octaveOffset[i] = new Vector2(noiseRandom.Next(-100000, 100000), noiseRandom.Next(-100000, 100000));
        }

        if (scale <= 0) scale = 0.0001f;

        float minNoiseHeight = float.MaxValue; 
        float maxNoiseHeight = float.MinValue;

        Vector2Int halfSize = mapSize / 2;

        for (int x = 0; x < mapSize.x; x++) {
            for (int y = 0; y < mapSize.y; y++) {
                float noiseHeight  = HexNoises.DistortedPerlin(new Vector2(x, y) / scale + offset, persistance, octaves, distortion);

                noiseMap[x, y] = noiseHeight;
                if (noiseHeight < minNoiseHeight) minNoiseHeight = noiseHeight;
                else if (noiseHeight > maxNoiseHeight) maxNoiseHeight = noiseHeight;
            }
        }

        for (int x = 0; x < mapSize.x; x++)
            for (int y = 0; y < mapSize.y; y++)
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);

        return noiseMap;
    }
}
