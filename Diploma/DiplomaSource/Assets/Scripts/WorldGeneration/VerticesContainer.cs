using System.Collections.Generic;
using UnityEngine;

public class VerticesContainer
{
    public static System.Random Random;

    public float InnerRadius { get; private set; }
    public float InnerCathet { get; private set; }
    public Vector3[] HexInnerCorners { get; private set; }
    public Vector3[] HexOuterCorners { get; private set; }

    public AnimationCurve OceanCurve;

    public Dictionary<Vector2Int, List<HexVertex>> Vertices { get; private set; }
    public Dictionary<Vector3Int, Vector2Int> CubeToDisplaceCoords { get; private set; }

    //Переменные для генерации воды
    public Dictionary<Vector2Int, List<HexVertex>> WaterVertices { get; private set; }

    //Синглтон
    private static VerticesContainer _container;

    //Информация о сгенерированных мешах
    public Dictionary<Vector2Int, VertexInfo> DrawedVertices;

    //Индексы соседних точек
    public Vector2Int[] NeighboursIndices { get; private set; }

    //Данные настроек
    public int MapRadius;
    public float ChunkRadius;
    public int SubdivisionLevel;
    public float HeightScale;
    public float OceanHeight;

    //Конструктор ткласса
    private VerticesContainer()
    {
        Vertices = new Dictionary<Vector2Int, List<HexVertex>>();
        WaterVertices = new Dictionary<Vector2Int, List<HexVertex>>();
        CubeToDisplaceCoords = new Dictionary<Vector3Int, Vector2Int>();
        CalculateNeighbours();
    }

    //Функция генерации индексов ближайших вершин
    private void CalculateNeighbours()
    {
        NeighboursIndices = new Vector2Int[]
        {
            Vector2Int.right,
            Vector2Int.up,
            new Vector2Int(-1, 1),
            Vector2Int.left,
            new Vector2Int(-1, -1),
            Vector2Int.down
        };
    }

    //Функции для генерации суши
    //Функция вычисления направлений осей кубических координат
    public void CalculateCorners()
    {
        HexOuterCorners = new Vector3[3];
        HexInnerCorners = new Vector3[3];
        InnerRadius = ChunkRadius / Mathf.Pow(3, 0.5f);

        float a = 2 * Mathf.PI / 3;
        for (int i = 0; i < 3; i++) {
            HexOuterCorners[i] = new Vector3(ChunkRadius * Mathf.Cos(a * i), 0, ChunkRadius * Mathf.Sin(a * i));
            HexInnerCorners[i] = new Vector3(InnerRadius / SubdivisionLevel * Mathf.Sin(a * -i + a / 2), 0, InnerRadius / SubdivisionLevel * Mathf.Cos(a * -i + a / 2));
        }

        InnerCathet = (HexInnerCorners[0] + HexInnerCorners[1]).magnitude;
    }

    //Функция добавления вершины
    public void AddVertex(HexVertex vertex)
    {
        Vector2Int point = new Vector2Int(
            vertex.ParentChunk.DisplaceCoord.x * 3 * GetContainer().SubdivisionLevel / 2,
            (vertex.ParentChunk.DisplaceCoord.y * 2 - (vertex.ParentChunk.DisplaceCoord.x & 1)) * GetContainer().SubdivisionLevel)
            + vertex.DisplacementPos;

        if (!Vertices.ContainsKey(point)) {
            Vertices.Add(point, new List<HexVertex>());
            Vector3Int cube = DisplaceToCube(point);
            CubeToDisplaceCoords.Add(cube, point);

            Vertices[point].Add(vertex);
            Vertices[point][0].GlobalCubePos = cube;
            Vertices[point][0].GlobalDisplacePos = point;

            return;
        }

        //_cubeCoords.Add(vertex.ParentChunk.CubeVertices[vertex], point);
        Vertices[point].Add(vertex);
        Vertices[point][Vertices[point].Count - 1].GlobalCubePos = Vertices[point][0].GlobalCubePos;
        Vertices[point][Vertices[point].Count - 1].GlobalDisplacePos = point;
    }

    //Функция изменения высоты вершины
    public void AddVertexHeight(Vector2Int pos, float height, int submeshID = -1)
    {
        if (!Vertices.ContainsKey(pos)) return;

        List<HexVertex> vertices = Vertices[pos];
        for (int i = 0; i < vertices.Count; i++) {
            vertices[i].Height += height;
            //SetMeshVertexHeight(pos, vertices[i].Height);

            if (submeshID > -1) vertices[i].SubmeshID = submeshID;
        }
    }
    //Функция присваивания высоты вершине
    public void SetVertexHeight(Vector2Int pos, float height, int submeshID = -1)
    {
        if (!Vertices.ContainsKey(pos)) return;

        List<HexVertex> vertices = Vertices[pos];
        for (int i = 0; i < vertices.Count; i++) {
            vertices[i].Height = height;
            //SetMeshVertexHeight(pos, height);

            if (submeshID > -1) vertices[i].SubmeshID = submeshID;
        }
    }
    //Функция получения высоты вершины
    public float GetVertexHeight(Vector2Int point)
    {
        if (!Vertices.ContainsKey(point)) return 0;

        List<HexVertex> vertices = Vertices[point];
        return vertices[0].Height;
    }
    //Функция установки индекса сабмеша вершине
    public void SetVertexSubmesh(Vector2Int point, int submeshID)
    {
        if (!Vertices.ContainsKey(point)) return;

        List<HexVertex> vertices = Vertices[point];
        for (int i = 0; i < vertices.Count; i++)
            if (submeshID > -1) vertices[i].SubmeshID = submeshID;
    }

    //Функция предоставления доступа к экземпляру класса
    public static VerticesContainer GetContainer()
    {
        if (_container == null)
            _container = new VerticesContainer();
        return _container;
    }

    //Функция переопределения экземпляра класса
    public static void Clear()
    {
        _container = new VerticesContainer();
    }

    //Функции для генерации воды
    public void AddWaterVertex(Vector2Int pos, HexVertex vertex)
    {
        if (!WaterVertices.ContainsKey(pos)) WaterVertices.Add(pos, new List<HexVertex>());
        WaterVertices[pos].Add(vertex);
    }

    public void SetWaterVertexHeight(Vector2Int pos, float height)
    {
        if (!WaterVertices.ContainsKey(pos)) return;

        List<HexVertex> vertices = WaterVertices[pos];
        for (int i = 0; i < vertices.Count; i++) {
            vertices[i].Height = height;
        }
    }

    public void SetLocalWaterHeight(Vector2Int pos, float height)
    {
        if (!WaterVertices.ContainsKey(pos)) return;

        List<HexVertex> vertices = WaterVertices[pos];
        for (int i = 0; i < vertices.Count; i++) {
            vertices[i].Height = height + GetVertexHeight(pos);
            vertices[i].WaterHeight = height;
        }
    }

    public float GetLocalWaterHeight(Vector2Int pos) => WaterVertices.ContainsKey(pos) ? WaterVertices[pos][0].WaterHeight : float.MinValue;

    //Расчеты в кубических координатах и координатах смещения
    //Функция генерации случайной точки с кубическими координатами в указанных пределах
    public static Vector3Int GenerateRandomHexPoint(System.Random rand, int radius)
    {
        int x = rand.Next(-radius, radius);
        int y = rand.Next(-radius, radius - Mathf.Abs(x)) * System.Math.Sign(x);

        return new Vector3Int(x, y, -(x + y));
    }
    //Функция перевода кубических координат в декартовы
    public static Vector3 CubeToWorld(Vector3Int hexPos) => hexPos.x * _container.HexInnerCorners[0] + hexPos.y * _container.HexInnerCorners[1] + hexPos.z * _container.HexInnerCorners[2];
    //Функция генерации случайной точки на поверхности сетки
    public static Vector3 GenerateRandomPointOnTerrain(System.Random rand, int radius)
    {
        Vector3Int hexPos = GenerateRandomHexPoint(rand, radius);
        return hexPos.x * _container.HexInnerCorners[0] + hexPos.y * _container.HexInnerCorners[1] + hexPos.z * _container.HexInnerCorners[2];
    }

    public static Vector3 WorldToCube(Vector3 pos) //Нахождение кубических координат из декартовых. Используется теорема синусов (sinA/a=sinB/b=sinC/с). с=а*sinC/sinA. a - гипотенуза, поэтому sinA = 1
    {
        Vector3 localPos = new Vector3(pos.x, 0, pos.z) / GetContainer().InnerRadius * GetContainer().SubdivisionLevel / 1.5f;
        Vector3[] corners = GetContainer().HexInnerCorners;

        float x = localPos.magnitude * Mathf.Sin((90 - Vector3.Angle(corners[0], localPos)) * Mathf.PI / 180);
        float y = localPos.magnitude * Mathf.Sin((90 - Vector3.Angle(corners[1], localPos)) * Mathf.PI / 180);
        float z = localPos.magnitude * Mathf.Sin((90 - Vector3.Angle(corners[2], localPos)) * Mathf.PI / 180);
        return new Vector3(x, y, z);
    }
    public static Vector2Int HexToColDisplace(Vector3Int pos) => new Vector2Int(pos.x, pos.z + (pos.x + (pos.x & 1)) / 2); //преобразование кубических координат в чёт-q смещение
    public static Vector2Int HexToEvenRowDisplace(Vector3Int pos) => new Vector2Int(pos.x + (pos.z + (pos.z & 1)) / 2, pos.z); //преобразование кубических координат в чёт-r смещение
    public static Vector2Int HexToOddRowDisplace(Vector3Int pos) => new Vector2Int(pos.x + (pos.z - (pos.z & 1)) / 2, pos.z); //преобразование кубических координат в чёт-r смещение
    //Преобразование чёт-r смещения в кубические координаты
    public static Vector3Int DisplaceToCube(Vector2Int pos)
    {
        int x = pos.x - (pos.y - (pos.y & 1)) / 2,
            z = pos.y,
            y = -x - z;

        return new Vector3Int(x, y, z);
    }
    //Получение координат смещения ячейки, ближайшей к точке
    public static Vector2Int GetChunkPos(Vector3 pos)
    {
        int col = System.Convert.ToInt32(pos.x / (GetContainer().ChunkRadius * 1.5f));
        int row = System.Convert.ToInt32((float)(col & 1) / 2 - pos.z / 3 / GetContainer().InnerRadius);

        return new Vector2Int(col, row);
    }
    //Получение координат смещения вершины, ближайшей к точке
    public static Vector2Int GetVertexPos(Vector3 pos)
    {
        int row = -System.Convert.ToInt32(pos.z / (GetContainer().InnerRadius * 1.5f / GetContainer().SubdivisionLevel));
        int col = System.Convert.ToInt32((float)pos.x / GetContainer().ChunkRadius * GetContainer().SubdivisionLevel - (float)(row & 1) / 2);

        return new Vector2Int(col, row);
    }

}

public class VertexInfo
{
    public MeshFilter Filter;
    public MeshCollider Collider;
    public int ID;
}