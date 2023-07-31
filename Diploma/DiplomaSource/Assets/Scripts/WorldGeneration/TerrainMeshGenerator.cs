using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ��������� ����
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
    //������� ��������� ����� �����
    public static Dictionary<Vector2Int, HexChunk> GenerateMultiHexMesh(float chunkRadius, int chunkCount, int subdivisionLevel, int submeshCount)
    {
        if (subdivisionLevel < 1) subdivisionLevel = 1;

        Vector3[] corners = VerticesContainer.GetContainer().HexOuterCorners;

        Dictionary<Vector2Int, HexChunk> chunks = new Dictionary<Vector2Int, HexChunk>();

        //������� ��������� �������� ���������� ��������� � �������� �������
        for (int x = 1 - chunkCount; x < chunkCount; x++) {
            for (int y = 1 - chunkCount; y < chunkCount; y++) {
                if (Mathf.Abs(x + y) > chunkCount - 1) continue;
                int z = -(x + y);
                Vector3Int cubePos = new Vector3Int(x, y, z);
                Vector2Int displacePos = VerticesContainer.HexToColDisplace(cubePos);

                //�������� � ���������� � ������ ������ �����
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

    //����������� ��� ������������� �����
    public MeshData(int meshWidth, int meshHeight, int submeshCount)
    {
        _submeshCount = submeshCount;
        Vertices = new Vector3[meshWidth * meshHeight];
        UVs = new Vector2[meshWidth * meshHeight];
        Triangles = new List<int>[submeshCount];
        for (int i = 0; i < submeshCount; i++)
            Triangles[i] = new List<int>();
    }

    //����������� ��� ����������� ��� ������������� �����
    public MeshData(int subdivLevel, int submeshCount)
    {
        _submeshCount = submeshCount;

        int n = subdivLevel + 1;
        int count = (1 + n) * n / 2 * 6 + 1; //���������� ������ �������������� - ���������� ������ ���������� ������������� + 1

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
    //�������� �����
    public Vector3 Offset;

    public Vector3Int CubeCoord { get; private set; }
    public Vector2Int DisplaceCoord { get; private set; }

    public ChunkInfo ChunkInfo;

    //���������� ����
    public Dictionary<Vector2Int, int> DisplaceVertices; //�������, ��������������� ����������� ��������
    public HexVertex[] Vertices; //������ ������
    public List<int>[] Triangles; //������ �������������

    private int _submeshCount;
    private int _subdivLevel;
    private float _meshRadius;

    //����
    public Dictionary<Vector2Int, int> WDisplaceVertices;
    public List<HexVertex> WVertices;
    public List<int> WTriangles;

    //����������� ������
    public HexChunk(Vector3Int cubeCoord, Vector2Int displaceCoord, int subdivLevel, int submeshCount, float meshRadius)
    {
        CubeCoord = cubeCoord;
        DisplaceCoord = displaceCoord;

        _subdivLevel = subdivLevel;
        _submeshCount = submeshCount;
        _meshRadius = meshRadius;

        int count = (1 + subdivLevel) * subdivLevel / 2 * 6 + 1; //���������� ������ �������������� - ���������� ������ ���������� ������������� + 1

        DisplaceVertices = new Dictionary<Vector2Int, int>();
        Vertices = new HexVertex[count];

        Triangles = new List<int>[submeshCount];
        for (int i = 0; i < submeshCount; i++)
            Triangles[i] = new List<int>();

        WVertices = new List<HexVertex>();
        WDisplaceVertices = new Dictionary<Vector2Int, int>();
        WTriangles = new List<int>();
    }

    //������� ������������ ������ � �� ���������� � ������������
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
    //������� ��������� ������ ���� �����������
    public void SetVertexSubmeshID(int vertexID, int submesh)
    {
        if (vertexID >= Vertices.Length) return;
        Vertices[vertexID].SubmeshID = submesh;
    }
    //������� ���������� ������������
    private void AddTriangle(int a, int b, int c)
    {
        int submeshID = Vertices[a].SubmeshID;

        if (Vertices[b].SubmeshID == Vertices[c].SubmeshID)
            submeshID = Vertices[b].SubmeshID;

        Triangles[submeshID].AddRange(new int[] { a, b, c });
    }
    //������� ��������� ������������� �����
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
    //������� �������� ������������� ����� �� ������ ������ ������
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
    //������� ����������� �����
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

    //����
    private void WAddTriangle(int a, int b, int c)
    {
        WTriangles.AddRange(new int[] { a, b, c });
    }

    public void WGenerateTriangles()
    {
        foreach (Vector2Int pos in WDisplaceVertices.Keys) {
            //���� �������� ���� �����
            if (WDisplaceVertices.ContainsKey(pos + Vector2Int.up)) {
                //���� ������ ���� �����
                if (WDisplaceVertices.ContainsKey(pos + Vector2Int.right))
                    WAddTriangle(WDisplaceVertices[pos], WDisplaceVertices[pos + Vector2Int.right], WDisplaceVertices[pos + Vector2Int.up]);
                //���� ����� ����� ���� �����
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
    //������������ ������
    public HexChunk ParentChunk { get; private set; }
    //���������� ����� � ��������� �������� ���������
    public Vector3Int GlobalCubePos;
    public Vector3Int CubePos { get; private set; }
    public Vector2Int GlobalDisplacePos;
    public Vector2Int DisplacementPos { get; private set; }
    public Vector3 LocalPos { get; private set; }
    public Vector3 GlobalPos { get; private set; }
    //������ ������� � ������ ������ ������
    public int ID { get; private set; }

    public float Height;  //������ ������
    public int SubmeshID; //����� �������

    //������ ���� ��� ��������
    public float WaterHeight;

    //����������� ������ �� ������ ������
    public HexVertex(HexChunk parent, Vector3Int cubePos, Vector2Int displacePos, Vector3 localPos, int id)
    {
        ParentChunk = parent;
        CubePos = cubePos;
        DisplacementPos = displacePos;
        LocalPos = localPos;
        ID = id;

        GlobalPos = LocalPos + ParentChunk.Offset;
    }
    //����������� �� ������ ������ �������
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

    //����� ��������� ������ �������
    public void SetHeight(float height) => Height = height;

    //����� ���������� ���������� ������� ������� � ���������� �����������
    public Vector3 GetGlobalPosition() => GlobalPos + Vector3.up * Height;

    //����� �������������� � ������
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