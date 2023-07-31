using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Генератор меша
public static class TerrainMeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve heightCurve, int detailsLevel)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        int simplificationIncrement = (int)Mathf.Pow(2, detailsLevel);
        int verticesPerLine = (width - 1) / simplificationIncrement + 1;
        int verticesPerColumn = (height - 1) / simplificationIncrement + 1;

        MeshData meshData = new MeshData(verticesPerLine, verticesPerColumn);
        int vertexIndex = 0;

        for (int y = 0; y < height; y += simplificationIncrement) {
            for (int x = 0; x < width; x += simplificationIncrement) {
                meshData.Vertices[vertexIndex] = new Vector3(topLeftX + x, heightCurve.Evaluate(heightMap[x, y]) * heightMultiplier, topLeftZ - y);
                meshData.UVs[vertexIndex] = new Vector2(x / (float)width, y / (float)height);

                if (x < width - 1 && y < height - 1) {
                    meshData.AddTriangle(vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);
                    meshData.AddTriangle(vertexIndex, vertexIndex + 1, vertexIndex + verticesPerLine + 1);
                }

                vertexIndex++;
            }
        }

        return meshData;
    }

    public static MeshData[,] GenerateMultiTerrainMesh(float[,] heightMap, float heightMultiplier, Vector2Int chunksCount, AnimationCurve heightCurve, int detailsLevel)
    {
        Vector2Int size = new Vector2Int(heightMap.GetLength(0), heightMap.GetLength(1));
        Vector2Int chunkSize = new Vector2Int(size.x / chunksCount.x, size.y / chunksCount.y);

        MeshData[,] chunks = new MeshData[chunksCount.x, chunksCount.y];

        for (int x = 0; x < chunksCount.x; x++) {
            for (int y = 0; y < chunksCount.y; y++) {
                float[,] chunkMap = new float[chunkSize.x, chunkSize.y];

                for (int dx = 0; dx < chunkSize.x; dx++)
                    for (int dy = 0; dy < chunkSize.y; dy++)
                        chunkMap[dx, dy] = heightMap[dx + x * (chunkSize.x - 1), dy + y * (chunkSize.y - 1)];

                chunks[x, y] = GenerateTerrainMesh(chunkMap, heightMultiplier, heightCurve, detailsLevel);
            }
        }

        return chunks;
    }
    //Функция генерации сетки ячеек
    public static Dictionary<Vector2Int, HexChunk> GenerateMultiHexMesh(float chunkRadius, int chunkCount, int subdivisionLevel, int submeshCount)
    {
        if (subdivisionLevel < 1) subdivisionLevel = 1;

        Vector3[] corners = VerticesContainer.GetContainer().HexOuterCorners;

        Dictionary<Vector2Int, HexChunk> chunks = new Dictionary<Vector2Int, HexChunk>();

        //Перебор возможных значений кубических координат в звдвнном радиусе
        for (int x = 1 - chunkCount; x < chunkCount; x++) {
            for (int y = 1 - chunkCount; y < chunkCount; y++) {
                if (Mathf.Abs(x + y) > chunkCount - 1) continue;
                int z = -(x + y);
                Vector3Int cubePos = new Vector3Int(x, y, z);
                Vector2Int displacePos = VerticesContainer.HexToColDisplace(cubePos);

                //создание и добавление в список ячейки сетки
                chunks.Add(VerticesContainer.HexToColDisplace(cubePos), new HexChunk(cubePos, displacePos, subdivisionLevel, submeshCount, chunkRadius));
                chunks[displacePos].Offset = (x * corners[0] + y * corners[1] + z * corners[2]);
                chunks[displacePos].GenerateMeshData();
            }
        }

        return chunks;
    }
}

public class MeshData
{
    public Vector3[] Vertices;
    public Vector2[] UVs;
    public List<int>[] Triangles;

    private int _submeshCount;

    //Конструктор для прямоугольной сетки
    public MeshData(int meshWidth, int meshHeight, int submeshCount)
    {
        _submeshCount = submeshCount;
        Vertices = new Vector3[meshWidth * meshHeight];
        UVs = new Vector2[meshWidth * meshHeight];
        Triangles = new List<int>[submeshCount];
        for (int i = 0; i < submeshCount; i++)
            Triangles[i] = new List<int>();
    }

    //Конструктор для треугольной или шестиугольной сетки
    public MeshData(int subdivLevel, int submeshCount)
    {
        _submeshCount = submeshCount;

        int n = subdivLevel + 1;
        int count = (1 + n) * n / 2 * 6 + 1; //Количество вершин шестиугольника - количество вершин внутренних треугольников + 1

        Vertices = new Vector3[count];
        UVs = new Vector2[count];
        Triangles = new List<int>[submeshCount];
        for (int i = 0; i < submeshCount; i++)
            Triangles[i] = new List<int>();
    }

    public void AddTriangle(int a, int b, int c, int submeshID = 0)
    {
        Triangles[submeshID].AddRange(new int[] { a, b, c });
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();

        mesh.vertices = Vertices;
        mesh.uv = UVs;

        for (int i = 0; i < _submeshCount; i++) {
            mesh.SetTriangles(Triangles[i].ToArray(), i);
        }

        mesh.RecalculateNormals();

        return mesh;
    }
}

public class HexChunk
{
    //Свойства чанка
    public Vector3 Offset;

    public Vector3Int CubeCoord { get; private set; }
    public Vector2Int DisplaceCoord { get; private set; }

    public ChunkInfo ChunkInfo;

    //Переменные меша
    public Dictionary<Vector2Int, int> DisplaceVertices; //индексы, соответствующие координатам смещения
    public HexVertex[] Vertices; //Массив вершин
    public List<int>[] Triangles; //Список треугольников

    private int _submeshCount;
    private int _subdivLevel;
    private float _meshRadius;

    //Вода
    public Dictionary<Vector2Int, int> WDisplaceVertices;
    public List<HexVertex> WVertices;
    public List<int> WTriangles;

    //Конструктор класса
    public HexChunk(Vector3Int cubeCoord, Vector2Int displaceCoord, int subdivLevel, int submeshCount, float meshRadius)
    {
        CubeCoord = cubeCoord;
        DisplaceCoord = displaceCoord;

        _subdivLevel = subdivLevel;
        _submeshCount = submeshCount;
        _meshRadius = meshRadius;

        int count = (1 + subdivLevel) * subdivLevel / 2 * 6 + 1; //Количество вершин шестиугольника - количество вершин внутренних треугольников + 1

        DisplaceVertices = new Dictionary<Vector2Int, int>();
        Vertices = new HexVertex[count];

        Triangles = new List<int>[submeshCount];
        for (int i = 0; i < submeshCount; i++)
            Triangles[i] = new List<int>();

        WVertices = new List<HexVertex>();
        WDisplaceVertices = new Dictionary<Vector2Int, int>();
        WTriangles = new List<int>();
    }

    //Функция расположения вершин и их связывания в треугольники
    public void GenerateMeshData()
    {
        Vector3[] corners = VerticesContainer.GetContainer().HexInnerCorners;

        int id = 0;

        for (int x = -_subdivLevel; x <= _subdivLevel; x++) {
            for (int y = -_subdivLevel; y <= _subdivLevel; y++) {
                if (Mathf.Abs(x + y) > _subdivLevel) continue;
                int z = -(x + y);

                Vector3Int cube = new Vector3Int(x, y, z);
                Vector2Int displace = VerticesContainer.HexToOddRowDisplace(cube);

                Vertices[id] = new HexVertex(this, cube, displace, (x * corners[0] + y * corners[1] + z * corners[2]), id);
                Vertices[id].Height = 1;
                DisplaceVertices.Add(displace, id);

                VerticesContainer.GetContainer().AddVertex(Vertices[id]);

                id++;
            }
        }
    }
    //Функция установки номера типа поверхности
    public void SetVertexSubmeshID(int vertexID, int submesh)
    {
        if (vertexID >= Vertices.Length) return;
        Vertices[vertexID].SubmeshID = submesh;
    }
    //Функция добавления треугольника
    private void AddTriangle(int a, int b, int c)
    {
        int submeshID = Vertices[a].SubmeshID;

        if (Vertices[b].SubmeshID == Vertices[c].SubmeshID)
            submeshID = Vertices[b].SubmeshID;

        Triangles[submeshID].AddRange(new int[] { a, b, c });
    }
    //Функция генерации треугольников сетки
    private void GenerateTriangles()
    {
        int id = 0;
        int oppositeID = Vertices.Length - 1;
        int nextLineID = 0;

        for (int x = -_subdivLevel; x < 0; x++) {
            int inLine = (_subdivLevel * 2 + 1) - Mathf.Abs(x);
            nextLineID += inLine;

            for (int y = -_subdivLevel; y < _subdivLevel + 1; y++) {
                if (Mathf.Abs(x + y) > _subdivLevel) continue;
                int z = -(x + y);

                AddTriangle(id, id + inLine + 1, id + inLine);
                AddTriangle(oppositeID, oppositeID - inLine - 1, oppositeID - inLine);
                if (id < nextLineID - 1) {
                    AddTriangle(id, id + 1, id + inLine + 1);
                    AddTriangle(oppositeID, oppositeID - 1, oppositeID - inLine - 1);
                }

                id++;
                oppositeID--;
            }
        }
    }
    //Функция создания полигональной сетки на основе данных класса
    public Mesh CreateMesh()
    {
        GenerateTriangles();

        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[Vertices.Length];
        for (int i = 0; i < Vertices.Length; i++) {
            vertices[i] = Vertices[i].LocalPos + Vector3.up * Vertices[i].Height;
        }
        mesh.vertices = vertices;

        mesh.subMeshCount = _submeshCount;

        for (int i = 0; i < _submeshCount; i++) {
            if (Triangles[i].Count > 0)
                mesh.SetTriangles(Triangles[i].ToArray(), i);
        }
        mesh.RecalculateNormals();

        return mesh;
    }
    //Функция перерисовки сетки
    public void RedrawMaterials()
    {
        GenerateTriangles();

        Mesh mesh = ChunkInfo.Mesh;
        mesh.triangles = new int[0];
        mesh.subMeshCount = _submeshCount;

        for (int i = 0; i < _submeshCount; i++) {
            if (Triangles[i].Count > 0)
                mesh.SetTriangles(Triangles[i].ToArray(), i);
        }
    }

    //Вода
    private void WAddTriangle(int a, int b, int c)
    {
        WTriangles.AddRange(new int[] { a, b, c });
    }

    public void WGenerateTriangles()
    {
        foreach (Vector2Int pos in WDisplaceVertices.Keys) {
            //Если напротив есть точка
            if (WDisplaceVertices.ContainsKey(pos + Vector2Int.up)) {
                //Если справа есть точка
                if (WDisplaceVertices.ContainsKey(pos + Vector2Int.right))
                    WAddTriangle(WDisplaceVertices[pos], WDisplaceVertices[pos + Vector2Int.right], WDisplaceVertices[pos + Vector2Int.up]);
                //Если слева снизу есть точка
                if (WDisplaceVertices.ContainsKey(pos + new Vector2Int(-1, 1)))
                    WAddTriangle(WDisplaceVertices[pos], WDisplaceVertices[pos + Vector2Int.up], WDisplaceVertices[pos + new Vector2Int(-1, 1)]);
            }
        }
    }

    public void WAddVertex(Vector2Int pos)
    {
        if (WDisplaceVertices.ContainsKey(pos)) return;

        WDisplaceVertices.Add(pos, WVertices.Count);
        WVertices.Add(Vertices[DisplaceVertices[pos]]);
        VerticesContainer.GetContainer().AddWaterVertex(Vertices[DisplaceVertices[pos]].GlobalDisplacePos, Vertices[DisplaceVertices[pos]]);
    }

    public Mesh WCreateMesh()
    {
        WGenerateTriangles();

        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[WVertices.Count];
        for (int i = 0; i < WVertices.Count; i++) {
            vertices[i] = WVertices[i].LocalPos + Vector3.up * (WVertices[i].Height + WVertices[i].WaterHeight);
        }
        mesh.vertices = vertices;
        mesh.triangles = WTriangles.ToArray();
        mesh.RecalculateNormals();

        return mesh;
    }
}

public enum HexSide
{
    NE, N, NW, SW, S, SE
}

public class HexVertex
{
    //Родительская ячейка
    public HexChunk ParentChunk { get; private set; }
    //Координаты точки в различных системах уоординат
    public Vector3Int GlobalCubePos;
    public Vector3Int CubePos { get; private set; }
    public Vector2Int GlobalDisplacePos;
    public Vector2Int DisplacementPos { get; private set; }
    public Vector3 LocalPos { get; private set; }
    public Vector3 GlobalPos { get; private set; }
    //Индекс вершины в списке вершин ячейки
    public int ID { get; private set; }

    public float Height;  //Высота ячейки
    public int SubmeshID; //Номер сабмеша

    //Высота воды над верщиной
    public float WaterHeight;

    //Конструктор класса на основе данных
    public HexVertex(HexChunk parent, Vector3Int cubePos, Vector2Int displacePos, Vector3 localPos, int id)
    {
        ParentChunk = parent;
        CubePos = cubePos;
        DisplacementPos = displacePos;
        LocalPos = localPos;
        ID = id;

        GlobalPos = LocalPos + ParentChunk.Offset;
    }
    //Конструктор на основе другой вершины
    public HexVertex(HexVertex vertex)
    {
        ParentChunk = vertex.ParentChunk;
        CubePos = vertex.CubePos;
        DisplacementPos = vertex.DisplacementPos;
        LocalPos = vertex.LocalPos;
        ID = vertex.ID;

        GlobalCubePos = vertex.GlobalCubePos;
        GlobalDisplacePos = vertex.GlobalDisplacePos;

        GlobalPos = vertex.GlobalPos;
    }

    //Метод установки высоты вершины
    public void SetHeight(float height) => Height = height;

    //Метод вычисления глобальной позиции вершины в декартовых координатах
    public Vector3 GetGlobalPosition() => GlobalPos + Vector3.up * Height;

    //Метод преобразования в строку
    public override string ToString()
    {
        return "Parent position: " + ParentChunk.Offset + "\n" +
            "\t" + ParentChunk.DisplaceCoord + "\n" +
            "vertex positions: " + "\n" +
            "\t" + LocalPos + "\n" +
            "\t" + DisplacementPos + "\n" +
            "\t" + CubePos;
    }
}