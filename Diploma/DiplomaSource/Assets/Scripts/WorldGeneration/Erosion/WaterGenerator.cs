using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//Генерация водоёмов 
public static class WaterGenerator
{
    public static float smoothRadius { get; private set; }

    private static List<Vector2Int> _smoothIndices;

    private static VerticesContainer _container;

    //Посещенные точки
    private static List<Vector2Int> _descendVisited;
    private static List<Vector2Int> _poolVisited;

    public static void GenerateWater(HexMapGenerator generator, int iterations, Dictionary<Vector2Int, HexChunk> chunks)
    {
        //Когда вода идёт к точке выше прежней, высота поверхности не меняется, если ниже, то понижается

        _container = VerticesContainer.GetContainer();
        //GenerateSmoothIndices(generator.WaterSmoothRadius);

        //Алгоритм спуска воды
        static void DescendFill(Vector2Int pos, float waterHeight)
        {
            //Проверка выхода за пределы карты
            if (!_container.Vertices.ContainsKey(pos)) return;

            //Если вершина не является частью водоёма, она создаётся
            float newHeight = waterHeight > _container.GetLocalWaterHeight(pos) ? waterHeight : _container.GetLocalWaterHeight(pos);
            foreach (HexVertex vertex in _container.Vertices[pos]) {
                if (!vertex.ParentChunk.WDisplaceVertices.ContainsKey(vertex.DisplacementPos)) vertex.ParentChunk.WAddVertex(vertex.DisplacementPos);
                vertex.WaterHeight = newHeight;
            }
            _descendVisited.Add(pos);

            //Если уровен воды равен или меньше нуля, то соседние точки не заполняются
            if (waterHeight <= 0) return;

            //Переменная для определения возможности течь дальше
            bool hasDrain = false;

            foreach (Vector2Int currPos in _container.NeighboursIndices) {
                if (_descendVisited.Contains(pos + currPos)) continue;
                float currHeight = waterHeight;

                if (_container.GetVertexHeight(pos) < _container.GetVertexHeight(pos + currPos))
                    currHeight = _container.GetVertexHeight(pos) + waterHeight - _container.GetVertexHeight(pos + currPos);

                DescendFill(pos + currPos, currHeight);
            }
        }

        //for (int i = 0; i < 10; i++) {
        //    chunks[Vector2Int.zero].WAddVertex(Vector2Int.right * i);
        //    chunks[Vector2Int.zero].WAddVertex(Vector2Int.right * i + Vector2Int.up);
        //}
        //chunks[Vector2Int.zero].WGenerateTriangles();

        _descendVisited = new List<Vector2Int>();
        DescendFill(Vector2Int.up * 20, 0.01f);
        foreach(HexChunk chunk in chunks.Values) {
            if (chunk.WVertices.Count < 3) continue;
            chunk.WGenerateTriangles();
        }
    }

    //Алгоритм образования статичных вод

    //Рассчет параметров сглаживания водной глади (рассчёт индексов вокруг точки, влияющих на неё)
    private static void GenerateSmoothIndices(float sRadius)
    {
        VerticesContainer container = VerticesContainer.GetContainer();

        //Если радиус меньше нуля, то он обнуляется
        if (sRadius < 0) sRadius = 0;
        //Если радиус больше радиуса карты, он приравнивается радиусу карты
        else if (sRadius > container.ChunkRadius * container.MapRadius) sRadius = container.InnerRadius * container.MapRadius;

        smoothRadius = sRadius;

        _smoothIndices = new List<Vector2Int>();

        //Если радиус равен нулю, эрозии подвергается только центральная вершина
        if (sRadius == 0) {
            _smoothIndices.Add(Vector2Int.zero);
            return;
        }

        //Радиус для получения кубических координат вершин
        int radius = (int)(sRadius / container.ChunkRadius * container.SubdivisionLevel);

        //Помещение вершин, лежащих внутри радиуса действия эрозии, в список. Степень влияния эрозии на вершину равняется квадрату расстояния от границы окружности до вершины
        int id = 0;
        for (int x = -radius; x <= radius; x++) {
            for (int y = -radius; y <= radius; y++) {
                int z = -(x + y);
                Vector3Int pos = new Vector3Int(x, y, z);

                if (!container.CubeToDisplaceCoords.ContainsKey(pos)) continue;

                HexVertex vertex = container.Vertices[container.CubeToDisplaceCoords[pos]][0];

                if (vertex.GlobalPos.magnitude > smoothRadius) continue;

                _smoothIndices.Add(vertex.GlobalDisplacePos);

                id++;
            }
        }
    }

    private static void SmoothAt(Vector2Int pos)
    {
        float height = 0;
        for (int j = 0; j < _smoothIndices.Count; j++) {
            if (!_container.Vertices.ContainsKey(_smoothIndices[j] + pos)) continue;
            height += _container.Vertices[_smoothIndices[j] + pos][0].Height;
        }

        _container.SetWaterVertexHeight(pos, height / _smoothIndices.Count);
    }
}