using System;
using System.Collections.Generic;
using UnityEngine;

public static class BiomesGenerator
{
    public static Biome[] GenerateBiomesMap(Vector2Int size, float[,] heightMap, float[,] wetMap, float[,] tempMap)
    {
        int count = Convert.ToInt32(LandscapeDB.ExecuteQueryWithAnswer("SELECT COUNT(*) FROM Biomes;"));

        Biome[] biomes = new Biome[count];

        for (int i = 0; i < count; i++) {
            biomes[i] = new Biome();
            string[] limits = LandscapeDB.GetTable("SELECT TemperatureLimits FROM Biomes WHERE BiomID = " + i).Rows[0][0].ToString().Split('|');

            biomes[i].TempLimits = new Vector2((float)Convert.ToDouble(limits[0]), (float)Convert.ToDouble(limits[1]));

            limits = LandscapeDB.GetTable("SELECT WetLimits FROM Biomes WHERE BiomID = " + i).Rows[0][0].ToString().Split('|');

            biomes[i].WetLimits = new Vector2((float)Convert.ToDouble(limits[0]), (float)Convert.ToDouble(limits[1]));

            limits = LandscapeDB.GetTable("SELECT HeightLimits FROM Biomes WHERE BiomID = " + i).Rows[0][0].ToString().Split('|');

            biomes[i].HeightLimits = new Vector2((float)Convert.ToDouble(limits[0]), (float)Convert.ToDouble(limits[1]));

            Color surface;
            Color slope;

            ColorUtility.TryParseHtmlString(LandscapeDB.GetTable("SELECT SurfaceColor FROM Biomes WHERE BiomID = " + i).Rows[0][0].ToString(), out surface);
            ColorUtility.TryParseHtmlString(LandscapeDB.GetTable("SELECT SlopeColor FROM Biomes WHERE BiomID = " + i).Rows[0][0].ToString(), out slope);

            biomes[i].SurfaceColor = surface;
            biomes[i].SlopeColor = slope;
        }

        for (int x = 0; x < size.x; x++) 
            for (int y = 0; y < size.y; y++) 
                for (int i = 0; i < count; i++) 
                    if (heightMap[x, y] >= biomes[i].HeightLimits.x && heightMap[x, y] <= biomes[i].HeightLimits.y) {
                        biomes[i].Points.Add(new Vector2Int(x, y));
                        biomes[i].Colors.Add(biomes[i].SurfaceColor);
                        continue;
                    }

        return biomes;
    }
}

public class Biome
{
    public float Name;

    public Color SurfaceColor;
    public Color SlopeColor;

    public List<Color> Colors;
    public List<Vector2Int> Points;

    public Vector2 WetLimits;
    public Vector2 TempLimits;
    public Vector2 HeightLimits;

    public Biome()
    {
        Points = new List<Vector2Int>();
        Colors = new List<Color>();
    }
}