using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WorldTreesPlacer
{
    //Функция расстановки деревьев по сетке ячеек
    public static void PlaceTrees(Dictionary<Vector2Int, HexChunk> chunks, float heightMultiplier, int seed, int density, float offset, HexMapGenerator.TreeInfo info)
    {
        System.Random rand = new System.Random(seed);
        //В каждой ячейке
        foreach (HexChunk chunk in chunks.Values) {
            List<Vector3> positions = new List<Vector3>();
            //На каждой вершине, являющейся почвой
            foreach (HexVertex pos in chunk.Vertices) {
                if (pos.SubmeshID == 0) {
                    positions.Add(new Vector3(pos.LocalPos.x, pos.Height * heightMultiplier, pos.LocalPos.z) + chunk.Offset);
                }
            }
            //Пока не достигнуто максимальное количество деревьев в ячейке или хватает вершин
            for (int i = 0; i < density; i++) {
                if (positions.Count == 0) break;
                int index = VerticesContainer.Random.Next(0, positions.Count);


                float ratio = Mathf.Lerp(info.MinRatio, info.MaxRatio, (float)rand.NextDouble());
                float lengthScale = Mathf.Lerp(info.MinLengthScale, info.MaxLengthScale, (float)rand.NextDouble());

                RaycastHit hit;
                Physics.Raycast(positions[index] + new Vector3((float)VerticesContainer.Random.NextDouble() * offset, 10, (float)VerticesContainer.Random.NextDouble() * offset),
                    Vector3.down, out hit);
                Vector3 pos = hit.point;

                //Создать дерево в позиции вершины со случайным смещением и случайными параметрами в пределах допустимых
                InstantiaieTree(
                    pos - Vector3.up,
                    Mathf.Lerp(info.MinGrowSpeed, info.MaxGrowSpeed, (float)rand.NextDouble()),
                    Mathf.Lerp(info.MinFeed, info.MaxFeed, (float)rand.NextDouble()),
                    rand.Next(0, 10000),
                    info.RingSize,
                    Mathf.Lerp(info.MinRadiusScale, info.MaxRadiusScale, (float)rand.NextDouble()) * lengthScale,
                    lengthScale,
                    ratio,
                    Mathf.Lerp(info.MinSpread, info.MaxSpread, (float)rand.NextDouble()),
                    Mathf.Lerp(info.MinSplitSize, info.MaxSplitSize, (float)rand.NextDouble()),
                    Mathf.Lerp(info.MinSplitDecay, info.MaxSplitDecay, (float)rand.NextDouble()),
                    Mathf.Lerp(info.MinDirectedness, info.MaxDirectedness, (float)rand.NextDouble()),
                    ratio,
                    Mathf.Lerp(info.MinLeavesScale, info.MaxLeavesScale, (float)rand.NextDouble()),
                    Mathf.Lerp(info.MinInitLeavesRadius, info.MaxInitLeavesRadius, (float)rand.NextDouble())
                    );

                positions.RemoveAt(index);
            }
        }
    }

    //Функция создания дерева в определенной точке
    public static TreeModel InstantiaieTree(Vector3 position, float growSpeed, float feed, int seed, int ringSize, float radiusScale, float lengthScale, float ratio, 
        float spread, float splitSize, float splitDecay, float directedness, float taper, float leavesScale, float minLeavesRadius)
    {
        TreeModel model = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/WorldPrefabs/Tree"), position, Quaternion.identity).GetComponent<TreeModel>();
        
        model.GrowSpeed = growSpeed;
        model.Feed = feed;
        model.Seed = seed;
        model.RingSize = ringSize;
        model.LengthScale = lengthScale;
        model.RadiusScale = radiusScale;
        model.Ratio = ratio;
        model.Spread = spread;
        model.SplitSize = splitSize;
        model.SplitDecay = splitDecay;
        model.Directedness = directedness;
        model.Taper = taper;
        model.LeavesScale = leavesScale;
        model.MinLeavesRadius = minLeavesRadius;

        return model;
    }
}
