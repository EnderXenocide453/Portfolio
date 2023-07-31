using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public static class VoronoiDiagram
{
    public static SimpleVoronoiMap GenerateSimpleVoronoiDiagram(Vector2Int size, int regionsCount, int seed, float[] cellCoefficients)
    {
        System.Random noiseRandom = new System.Random(seed);

        SimpleVoronoiMap map = new SimpleVoronoiMap(size);
        SimpleTerrainRegion[] regions = new SimpleTerrainRegion[regionsCount];

        float maxHeight = float.MinValue;
        float minHeight = float.MaxValue;

        for (int i = 0; i < regionsCount; i++)
            regions[i] = new SimpleTerrainRegion((RegionType)noiseRandom.Next(0, 3), new Vector2(noiseRandom.Next(0, size.x), noiseRandom.Next(0, size.y)));

        for (int y = 0; y < size.y; y++) {
            for (int x = 0; x < size.x; x++) {
                List<float> distances = new List<float>();
                float dist = float.MaxValue;

                for (int i = 0; i < regionsCount; i++) {
                    distances.Add(Vector2.Distance(regions[i].Center, new Vector2(x, y)));

                    if (distances[i] < dist) {
                        dist = distances[i];
                        map.HeightMap[x, y] = regions[i].Height;
                    }
                }

                distances.Sort();
                for (int i = 0; i < cellCoefficients.Length; i++) {
                    map.InfluenceMap[x, y] += distances[i] * cellCoefficients[i];
                }

                if (map.InfluenceMap[x, y] > maxHeight) maxHeight = map.InfluenceMap[x, y];
                else if (map.InfluenceMap[x, y] < minHeight) minHeight = map.InfluenceMap[x, y];
            }
        }

        for (int x = 0; x < size.x; x++) {
            for (int y = 0; y < size.y; y++) {
                map.InfluenceMap[x, y] = Mathf.InverseLerp(minHeight, maxHeight, map.InfluenceMap[x, y]);
            }
        }

        return map;
    }

    public static RegionVoronoiMap GenerateRegionsVoronoiDiagram(Vector2Int size, int regionsCount, int seed, float regionBorderWidth, TerrainRegionType[] regionTypes)
    {
        System.Random noiseRandom = new System.Random(seed);
        RegionVoronoiMap voronoiMap = new RegionVoronoiMap(regionBorderWidth, seed, regionTypes);

        for (int i = 0; i < regionsCount; i++)
            voronoiMap.Regions.Add(new TerrainRegion(new Vector2((float)noiseRandom.NextDouble() * size.x, (float)noiseRandom.NextDouble() * size.y)));

        for (int x = 0; x < size.x; x++)
            for (int y = 0; y < size.y; y++)
                voronoiMap.AddPoint(new Vector2Int(x, y));

        //for (int i = 0; i < regionsCount; i++)
        //    voronoiMap.Regions[i].SetRegionType(regionTypes[noiseRandom.Next(0, regionTypes.Length)], regionTypes);

        return voronoiMap;
    }


}

public class SimpleVoronoiMap
{
    public float[,] InfluenceMap;
    public float[,] HeightMap;

    public Vector2Int size { get; private set; }

    public SimpleVoronoiMap(Vector2Int size)
    {
        this.size = size;

        HeightMap = new float[size.x, size.y];
        InfluenceMap = new float[size.x, size.y];
    }

    public float[,] CompareWithMap(float[,] map)
    {
        float[,] endMap = new float[size.x, size.y];

        float maxHeight = float.MinValue;
        float minHeight = float.MaxValue;

        for (int x = 0; x < size.x; x++)
            for (int y = 0; y < size.y; y++) {
                //поэкспериментировать надо
                endMap[x, y] = Mathf.Clamp(InfluenceMap[x, y] * HeightMap[x, y] * 2, 0, 1) + map[x, y];

                if (endMap[x, y] > maxHeight) maxHeight = endMap[x, y];
                else if (endMap[x, y] < minHeight) minHeight = endMap[x, y];
            }

        for (int x = 0; x < size.x; x++)
            for (int y = 0; y < size.y; y++)
                endMap[x, y] = Mathf.InverseLerp(minHeight, maxHeight, endMap[x, y]);

        return endMap;
    }
}

public class RegionVoronoiMap
{
    //Ширина границы перехода между регионами
    public float BorderWidth;
    public System.Random Rand;

    public float HeightModifier = 0;

    //Список регионов
    public List<TerrainRegion> Regions;

    //Массив всевозможных регионов
    private TerrainRegionType[] _regionTypes;

    public RegionVoronoiMap(float borderWidth, int seed, TerrainRegionType[] regionTypes)
    {
        BorderWidth = borderWidth;
        Rand = new System.Random(seed);

        Regions = new List<TerrainRegion>();
        _regionTypes = regionTypes;
    }

    //Расчёт средних значений регионов
    private void CalculateAvarageHeight(float[,] map)
    {
        for (int i = 0; i < Regions.Count; i++) {
            Regions[i].SetAvarageHeight(0);
            foreach (Vector2Int point in Regions[i].GetPoints().Keys) {
                Regions[i].SetAvarageHeight(Regions[i].AvarageHeight + map[point.x, point.y]);
            }
            Regions[i].SetAvarageHeight(Regions[i].AvarageHeight / Regions[i].GetPoints().Count);
        }
    }

    public void AddPoint(Vector2Int point)
    {
        (float, int) currRegion = (float.MaxValue, 0);
        List<(float, int)> regionsDist = new List<(float, int)>();

        for (int i = 0; i < Regions.Count; i++) {
            float dist = Vector2.Distance(point, Regions[i].Center);
            regionsDist.Add((dist, i));

            if (dist < currRegion.Item1) {
                currRegion = (dist, i);
            }
        }

        regionsDist.RemoveAt(currRegion.Item2);
        (float, int) nearestDist = (float.MaxValue, 0);

        Regions[currRegion.Item2].AddPoint(point, 1);
        foreach ((float, int) dist in regionsDist) { 
            if (dist.Item1 - currRegion.Item1 < BorderWidth)
                Regions[dist.Item2].AddPoint(point, Mathf.InverseLerp(BorderWidth, 0, dist.Item1 - currRegion.Item1));

            if (dist.Item1 < nearestDist.Item1) nearestDist = dist;
        }

        Regions[currRegion.Item2].AddNeighbour(Regions[nearestDist.Item2]);
        Regions[nearestDist.Item2].AddNeighbour(Regions[currRegion.Item2]);
    }

    //Объединение с картой высот
    public float[,] CompareWithMap(float[,] map)
    {
        Vector2Int size = new Vector2Int(map.GetLength(0), map.GetLength(1));
        float[,] endMap = new float[size.x, size.y];
        float[,] influenceSum = new float[size.x, size.y];

        float maxHeight = float.MinValue;
        float minHeight = float.MaxValue;

        //Вычисление средних значений высоты регионов
        CalculateAvarageHeight(map);

        //Распределение типов регионов
        //Исходя из того, какие соседи, определяется тип
        //SetRegionType(Regions[0], _regionTypes[0]);
        foreach (var region in Regions) {
            List<TerrainRegionType> types = new List<TerrainRegionType>();
            float weight = 0;

            foreach (TerrainRegionType type in _regionTypes) {
                if (region.AvarageHeight > type.PlacementHeightLimits.x && region.AvarageHeight < type.PlacementHeightLimits.y) {
                    types.Add(type);
                    weight += type.Probability;
                }
            }

            if (types.Count == 0) {
                region.SetRegionType(_regionTypes[0]);
                break;
            }

            float randNum = (float)Rand.NextDouble() * weight;
            weight = 0;

            foreach (TerrainRegionType type in types) {
                weight += type.Probability;
                if (randNum < weight) {
                    region.SetRegionType(type);
                    break;
                }
            }
        }

        for (int i = 0; i < Regions.Count; i++) {
            var points = Regions[i].GetPoints();
            foreach (Vector2Int point in points.Keys) {
                endMap[point.x, point.y] += (Regions[i].AvarageHeight + 
                    (map[point.x, point.y] - Regions[i].AvarageHeight) * 
                    Regions[i].RegionType.AvarageHeightDeform + Regions[i].RegionType.AdditionalHeight) * 
                    Regions[i].GetInfluence(point);

                influenceSum[point.x, point.y] += Regions[i].GetInfluence(point);
            }
        }

        for (int x = 0; x < size.x; x++)
            for (int y = 0; y < size.y; y++) {
                endMap[x, y] /= influenceSum[x, y];

                if (endMap[x, y] > maxHeight) maxHeight = endMap[x, y];
                else if (endMap[x, y] < minHeight) minHeight = endMap[x, y];
            }

        for (int x = 0; x < size.x; x++)
            for (int y = 0; y < size.y; y++)
                endMap[x, y] = Mathf.InverseLerp(minHeight, maxHeight, endMap[x, y]);

        HeightModifier = maxHeight - minHeight;
        return endMap;
    }

    public void DestributeRegions()
    {
        foreach (var region in Regions) {
            List<TerrainRegionType> types = new List<TerrainRegionType>();
            float weight = 0;

            foreach (TerrainRegionType type in _regionTypes) {
                if (region.AvarageHeight > type.PlacementHeightLimits.x && region.AvarageHeight < type.PlacementHeightLimits.y) {
                    types.Add(type);
                    weight += type.Probability;
                }
            }

            if (types.Count == 0) {
                region.SetRegionType(_regionTypes[0]);
                return;
            }

            float randNum = (float)Rand.NextDouble() * weight;
            weight = 0;

            foreach (TerrainRegionType type in types) {
                weight += type.Probability;
                if (randNum < weight) {
                    region.SetRegionType(type);
                    return;
                }
            }
        }
    }

    //public void ChooseRegionType(TerrainRegion region)
    //{
    //    List<Neighbour> possibleRegions = new List<Neighbour>();

    //    float weightSum = 0;

    //    foreach (TerrainRegion neighbour in region.GetNeighbours()) {
    //        if (neighbour.RegionType == null) continue;

    //        for (int i = 0; i < neighbour.RegionType.NeighbourRegionTypes.Length; i++) {
    //            if (!possibleRegions.Contains(neighbour.RegionType.NeighbourRegionTypes[i])) {
    //                possibleRegions.Add(neighbour.RegionType.NeighbourRegionTypes[i]);
    //                weightSum += neighbour.RegionType.NeighbourRegionTypes[i].Weight;
    //            }
    //        }
    //    }

    //    if (weightSum == 0)
    //        return;

    //    float randNum = (float)Rand.NextDouble() * weightSum;
    //    weightSum = 0;

    //    foreach (Neighbour weight in possibleRegions) {
    //        weightSum += weight.Weight;
    //        if (randNum <= weightSum) {
    //            SetRegionType(region, _regionTypes[weight.RegionID]);
    //            break;
    //        }
    //    }
    //}

    //public void SetRegionType(TerrainRegion region, TerrainRegionType type)
    //{
    //    region.SetRegionType(type);

    //    foreach (TerrainRegion neighbour in region.GetNeighbours()) {
    //        if (neighbour.RegionType != null) continue;
    //        ChooseRegionType(neighbour);
    //    }
    //}
}

//Структура региона для построения простой диаграммы
public struct SimpleTerrainRegion
{
    public float Height;
    public Vector2 Center;

    private RegionType _regionType;

    public SimpleTerrainRegion(RegionType regionType, Vector2 center)
    {
        _regionType = regionType;
        Height = (float)(regionType + 1) / 3f;

        Center = center;
    }
}

//Структура региона для построения диаграммы с учётом типов рельефа и их соседей 
public class TerrainRegion : IEquatable<TerrainRegion>
{
    public Vector2 Center;

    public TerrainRegionType RegionType { get; private set; }
    public float AvarageHeight { get; private set; }

    private Dictionary<Vector2Int, float> _points;
    private List<TerrainRegion> _neighbourRegions;

    public TerrainRegion(Vector2 center)
    {
        RegionType = null;

        _points = new Dictionary<Vector2Int, float>();
        _neighbourRegions = new List<TerrainRegion>();

        Center = center;
        AvarageHeight = 0;
    }

    public void SetRegionType(TerrainRegionType regionType)
    {
        RegionType = regionType;
    }

    public void SetAvarageHeight(float height) => AvarageHeight = height;

    //public void SetRegionType(TerrainRegionType regionType, TerrainRegionType[] types)
    //{
    //    RegionType = regionType;
    //    System.Random random = new System.Random(1);

    //    foreach (TerrainRegion neighbour in _neighbourRegions) {
    //        if (neighbour.RegionType != null) continue;

    //        float weightSum = 0;
    //        for (int i = 0; i < regionType.NeighboorRegionTypes.Length; i++)
    //            weightSum += regionType.NeighboorRegionTypes[i].Weight;

    //        float randNum = (float)random.NextDouble() * weightSum;
    //        weightSum = 0;
    //        for (int i = 0; i < regionType.NeighboorRegionTypes.Length; i++) {
    //            if (regionType.NeighboorRegionTypes[i].Weight + weightSum > randNum)
    //                neighbour.SetRegionType(types[regionType.NeighboorRegionTypes[i].RegionID], types);

    //            weightSum += regionType.NeighboorRegionTypes[i].Weight;
    //        }
    //    }
    //}

    public void AddPoint(Vector2Int point, float influence)
    {
        if (_points.ContainsKey(point))
            return;
        
        _points.Add(point, influence);
    }

    public float GetInfluence(Vector2Int point) => _points[point];

    public Dictionary<Vector2Int, float> GetPoints() => new Dictionary<Vector2Int, float>(_points);

    public void AddNeighbour(TerrainRegion region)
    {
        if (!_neighbourRegions.Contains(region))
            _neighbourRegions.Add(region);
    }

    public TerrainRegion[] GetNeighbours() => _neighbourRegions.ToArray();

    //Необходимо для работы Contains
    public bool Equals(TerrainRegion other)
    {
        return other.Center == Center;
    }
}

[System.Serializable]
public class TerrainRegionType
{
    public string Name;
    //Значение усреднения высот в регионе
    public float AvarageHeightDeform = 1;
    //Добавочная высота
    public float AdditionalHeight;

    //Частота генерации
    public float Probability = 1;

    //Границы высоты размещения
    public Vector2 PlacementHeightLimits = new Vector2(float.MinValue, float.MaxValue);
    public Neighbour[] NeighbourRegionTypes;
}

//Типы регионов, которые могут быть рядом с текущим, и вероятность их размещения
[System.Serializable]
public struct Neighbour { public int RegionID; public float Weight; }

public enum RegionType
{
    flat,
    mountain,
    ravine
}