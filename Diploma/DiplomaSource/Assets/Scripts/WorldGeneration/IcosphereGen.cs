using System.Collections.Generic;
using UnityEngine;

public static class IcosphereGen
{
    //Генерация треугольного меша
    public static DynamicMeshData GenerateTriangleMesh(int subdivision, Vector3 a, Vector3 b, Vector3 c)
    {
        DynamicMeshData data = new DynamicMeshData();

        Vector3 ab = (b - a) / (subdivision + 1);
        Vector3 bc = (c - b) / (subdivision + 1);

        int id = 0;

        Vector3 currPos = a;

        for (int i = 0; i < subdivision + 2; i++) {

            for (int j = 0; j < i + 1; j++) {
                data.AddVertex(currPos + bc * j);

                if (i < subdivision + 1) {
                    data.AddTriangle(id, id + i + 1, id + i + 2);
                    if (j < i)
                        data.AddTriangle(id, id + i + 2, id + 1);
                }

                id++;
            }

            currPos += ab;
        }

        return data;
    }

    //Генерация икосферы
    public static DynamicMeshData GenerateIcosphere(int subdivision, bool distort = false, float distortion = 0, int seed = 0)
    {
        System.Random random = new System.Random(seed);
        DynamicMeshData data = new DynamicMeshData();
        Vector2 perlinOffset = new Vector2((float)random.NextDouble() * 10000, (float)random.NextDouble() * 10000);

        //Золотое сечение
        float phi = (1 + Mathf.Sqrt(5)) / 2;

        //Расстановка углов начального икосаэдра
        Vector3[] anchors = new Vector3[]
        {
            new Vector3(-1, phi, 0),
            new Vector3(1, phi, 0),
            new Vector3(-1, -phi, 0),
            new Vector3(1, -phi, 0),

            new Vector3(0, -1, phi),
            new Vector3(0, 1, phi),
            new Vector3(0, -1, -phi),
            new Vector3(0, 1, -phi),

            new Vector3(phi, 0, -1),
            new Vector3(phi, 0, 1),
            new Vector3(-phi, 0, -1),
            new Vector3(-phi, 0, 1)
        };

        void AddBigTriangleToMesh(DynamicMeshData data, int subdivision, Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 ab = (b - a) / (subdivision + 1);
            Vector3 bc = (c - b) / (subdivision + 1);

            int id = data.Vertices.Count;

            Vector3 currPos = a;

            for (int i = 0; i < subdivision + 2; i++) {

                for (int j = 0; j < i + 1; j++) {
                    Vector3 pos = (currPos + bc * j).normalized;

                    if (distort) pos += pos * distortion * Mathf.PerlinNoise(perlinOffset.x + pos.x * pos.y, perlinOffset.y + pos.z * pos.y);
                    data.AddVertex(pos);

                    if (i < subdivision + 1) {
                        data.AddTriangle(id, id + i + 1, id + i + 2);
                        if (j < i)
                            data.AddTriangle(id, id + i + 2, id + 1);
                    }

                    id++;
                }

                currPos += ab;
            }
        }

        AddBigTriangleToMesh(data, subdivision, anchors[0], anchors[11], anchors[5]);
        AddBigTriangleToMesh(data, subdivision, anchors[0], anchors[5], anchors[1]);
        AddBigTriangleToMesh(data, subdivision, anchors[0], anchors[1], anchors[7]);
        AddBigTriangleToMesh(data, subdivision, anchors[0], anchors[7], anchors[10]);
        AddBigTriangleToMesh(data, subdivision, anchors[0], anchors[10], anchors[11]);

        AddBigTriangleToMesh(data, subdivision, anchors[1], anchors[5], anchors[9]);
        AddBigTriangleToMesh(data, subdivision, anchors[5], anchors[11], anchors[4]);
        AddBigTriangleToMesh(data, subdivision, anchors[11], anchors[10], anchors[2]);
        AddBigTriangleToMesh(data, subdivision, anchors[10], anchors[7], anchors[6]);
        AddBigTriangleToMesh(data, subdivision, anchors[7], anchors[1], anchors[8]);

        AddBigTriangleToMesh(data, subdivision, anchors[3], anchors[9], anchors[4]);
        AddBigTriangleToMesh(data, subdivision, anchors[3], anchors[4], anchors[2]);
        AddBigTriangleToMesh(data, subdivision, anchors[3], anchors[2], anchors[6]);
        AddBigTriangleToMesh(data, subdivision, anchors[3], anchors[6], anchors[8]);
        AddBigTriangleToMesh(data, subdivision, anchors[3], anchors[8], anchors[9]);

        AddBigTriangleToMesh(data, subdivision, anchors[4], anchors[9], anchors[5]);
        AddBigTriangleToMesh(data, subdivision, anchors[2], anchors[4], anchors[11]);
        AddBigTriangleToMesh(data, subdivision, anchors[6], anchors[2], anchors[10]);
        AddBigTriangleToMesh(data, subdivision, anchors[8], anchors[6], anchors[7]);
        AddBigTriangleToMesh(data, subdivision, anchors[9], anchors[8], anchors[1]);

        return data;
    }
}

public class DynamicMeshData
{
    public List<Vector3> Vertices { get; private set; }
    public List<int> Triangles { get; private set; }

    public DynamicMeshData()
    {
        Vertices = new List<Vector3>();
        Triangles = new List<int>();
    }

    public void AddTriangle(int a, int b, int c)
    {
        Triangles.AddRange(new int[] { a, b, c });
    }

    public void AddVertex(Vector3 pos)
    {
        Vertices.Add(pos);
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();

        mesh.vertices = Vertices.ToArray();
        mesh.triangles = Triangles.ToArray();

        mesh.RecalculateNormals();

        return mesh;
    }
}
