using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureGenerator
{
    public static Texture2D TextureFromColorMap(Color[] colorMap, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        texture.SetPixels(colorMap);
        texture.Apply();

        return texture;
    }

    public static Texture2D TextureFromHeightMap(float[,] heightMap, Color a, Color b)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        Color[] colorMap = new Color[width * height];
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                colorMap[x + y * width] = Color.Lerp(a, b, heightMap[x, y]);
            }
        }

        return TextureFromColorMap(colorMap, width, height);
    }
}
