using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public float Distortion = 4;

    public enum DrawMode { NoiseMap, ColorMap, Mesh }
    public DrawMode drawMode;

    public enum NoiseMode { Perlin, DiamondSquare, Flat, Voronoi }
    public NoiseMode noiseMode;

    public bool Voronoi;
    public bool Temperature;

    public float HeightTempMultiplier;
    public float TempScale;
    public float HeightTempLowerBound;

    public Vector2Int ChunksCount = Vector2Int.one;

    const int MapChunkSize = 129;
    [Range(0, 6)]
    public int DetailsLevel;
    public float NoiseScale;

    public int Octaves;
    [Range(0, 1)]
    public float Persistance;
    public float Lacunarity;

    public int Seed;
    public Vector2 Offset;

    public float MeshHeightMultiplier;
    public AnimationCurve MeshHeightCurve;

    public ColorTerrainType[] RegionColors;
    public float[] CellCoefficients;
    public int VoronoiRegionsCount;
    public float VoronoiBorderWidth;
    public TerrainRegionType[] TerrainRegionTypes;

    public bool AutoUpdate;

    public Transform Player;

    private float _heightMultiplier;

    private void Start()
    {
        GenerateMap(true);
    }

    public void GenerateMap(bool useOtherSeed = false)
    {
        if (useOtherSeed) Seed = WorldLoader.Seed;

        float[,] noiseMap;

        print(Mathf.Exp(-Persistance));

        if (noiseMode == NoiseMode.Perlin)
            noiseMap = PerlinNoise.GenerateNoiseMap(new Vector2Int(MapChunkSize * ChunksCount.x, MapChunkSize * ChunksCount.y), NoiseScale, Octaves, Persistance, Lacunarity, Seed, Offset, Distortion);
        //else if (noiseMode == NoiseMode.DiamondSquare)
        //    noiseMap = DiamondSquare.GenerateDSHeightMap(new Vector2Int(MapChunkSize, MapChunkSize), Seed);
        //else if (noiseMode == NoiseMode.Voronoi)
        //    noiseMap = VoronoiDiagram.GenerateSimpleVoronoiDiagram(new Vector2Int(MapChunkSize * ChunksCount.x, MapChunkSize * ChunksCount.y), VoronoiRegionsCount, Seed, CellCoefficients).InfluenceMap;
        else
            noiseMap = new float[MapChunkSize * ChunksCount.x, MapChunkSize * ChunksCount.y];

        if (Voronoi) {

            RegionVoronoiMap voronoiMap = VoronoiDiagram.GenerateRegionsVoronoiDiagram(new Vector2Int(MapChunkSize * ChunksCount.x, MapChunkSize * ChunksCount.y),
                                                                                            VoronoiRegionsCount * ChunksCount.x * ChunksCount.y, Seed, VoronoiBorderWidth, TerrainRegionTypes);
            noiseMap = voronoiMap.CompareWithMap(noiseMap);
            MeshHeightMultiplier = voronoiMap.HeightModifier * NoiseScale / 2;
            //noiseMap = VoronoiDiagram.GenerateSimpleVoronoiDiagram(new Vector2Int(MapChunkSize, MapChunkSize), VoronoiRegionsCount, Seed, CellCoefficients).CompareWithMap(noiseMap);
        }

        Color[] colorMap = new Color[MapChunkSize * ChunksCount.x * MapChunkSize * ChunksCount.y];

        for (int y = 0; y < MapChunkSize * ChunksCount.y; y++) {
            for (int x = 0; x < MapChunkSize * ChunksCount.x; x++) {
                float currHeight = noiseMap[x, y];
                for (int i = 0; i < RegionColors.Length; i++) {
                    if (currHeight <= RegionColors[i].Height) {
                        colorMap[y * MapChunkSize * ChunksCount.x + x] = RegionColors[i].Colour;

                        break;
                    }
                }
            }
        }

        MapDisplay display = FindObjectOfType<MapDisplay>();

        if (Temperature)
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(AdditionalNoises.GenerateTemperatureMap(noiseMap, Seed, TempScale, HeightTempMultiplier, HeightTempLowerBound), Color.blue, Color.red));

        else if (drawMode == DrawMode.NoiseMap)
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap, Color.white, Color.black));
        else if (drawMode == DrawMode.ColorMap)
            display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, MapChunkSize * ChunksCount.x, MapChunkSize * ChunksCount.y));
        else if (drawMode == DrawMode.Mesh) {
            display.DrawMesh(TerrainMeshGenerator.GenerateMultiTerrainMesh(noiseMap, MeshHeightMultiplier, ChunksCount, MeshHeightCurve, DetailsLevel), new Vector2Int(MapChunkSize, MapChunkSize), TextureGenerator.TextureFromColorMap(colorMap, MapChunkSize * ChunksCount.x, MapChunkSize * ChunksCount.y));
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap, Color.white, Color.black));
        }
    }

    private void OnValidate()
    {
        if (Octaves < 0) Octaves = 0;
        if (Lacunarity < 1) Lacunarity = 1;
        if (CellCoefficients.Length == 0) CellCoefficients = new float[] { 1 };
        if (ChunksCount.x < 1) ChunksCount.x = 1;
        if (ChunksCount.y < 1) ChunksCount.y = 1;
    }
}

[System.Serializable]
public struct ColorTerrainType
{
    public string Name;
    public float Height;
    public Color Colour;
}