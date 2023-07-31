using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
public class ReliefGenerator : MonoBehaviour
{
    public int SizePow = 5;
    public float WidthStep = 0.5f;
    public float HeightStep = 2f;
    
    private int _size;
    private float[,] _heightMap;

    private MeshFilter _meshFilter;
    private MeshCollider _meshCollider;

    void Start()
    {
        _size = (int)Mathf.Pow(2, SizePow) + 1;
        _meshFilter = GetComponent<MeshFilter>();
        _meshCollider = GetComponent<MeshCollider>();

        _heightMap = GenerateDSHeightMap();

        _meshFilter.mesh = MapToMesh(_heightMap, WidthStep);
        _meshCollider.sharedMesh = _meshFilter.mesh;
    }

    private float[,] GenerateDSHeightMap()
    {
        float[,] heightMap = new float[_size, _size];

        //установка значений высоты на крайних точках
        heightMap[0, 0] = Random.Range(-2, 2);
        heightMap[0, _size - 1] = Random.Range(-2, 2);
        heightMap[_size - 1, 0] = Random.Range(-2, 2);
        heightMap[_size - 1, _size - 1] = Random.Range(-2, 2);

        //размер стороны квадрата между точками
        int squareSide = _size - 1;
        float heightStep = HeightStep;

        while (squareSide > 1) {
            int halfSide = squareSide / 2;

            //алгоритм diamond
            for (int x = 0; x < _size - 1; x += squareSide) {
                for (int y = 0; y < _size - 1; y += squareSide) {
                    float height =
                        (heightMap[x, y] +
                        heightMap[x + squareSide, y] +
                        heightMap[x, y + squareSide] +
                        heightMap[x + squareSide, y + squareSide]) / 4 + 
                        Random.Range(-heightStep, heightStep);

                    heightMap[x + halfSide, y + halfSide] = height;
                }
            }

            //алгоритм square
            for (int x = 0; x < _size - 1; x += halfSide) {
                for (int y = (x + halfSide) % squareSide; y < _size - 1; y += squareSide) {
                    float height = (heightMap[(x - halfSide + _size - 1) % (_size - 1), y] +
                        heightMap[(x + halfSide) % (_size - 1), y] +
                        heightMap[x, (y + halfSide) % (_size - 1)] +
                        heightMap[x, (y - halfSide + _size - 1) % (_size - 1)]) / 4 +
                        Random.Range(-heightStep, heightStep);

                    heightMap[x, y] = height;

                    if (x == 0) heightMap[_size - 1, y] = height;
                    if (y == 0) heightMap[x, _size - 1] = height;
                }
            }

            squareSide /= 2;
            heightStep /= 2;
        }

        return heightMap;
    }

    private Mesh MapToMesh(float[,] map, float step)
    {
        Mesh mesh = new Mesh();

        int height = map.GetUpperBound(0) + 1;
        int width = map.Length / height;

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

        int currID = 0;

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (x == 0 || y == 0)
                    continue;
                
                vertices.AddRange(new Vector3[] {
                    //левый верхний треугольник
                    new Vector3(x * step, map[x, y - 1], (y - 1) * step),
                    new Vector3((x - 1) * step, map[x - 1, y - 1], (y - 1) * step),
                    new Vector3((x - 1) * step, map[x - 1, y], y * step),
                    //правый нижний треугольник
                    new Vector3(x * step, map[x, y - 1], (y - 1) * step),
                    new Vector3((x - 1) * step, map[x - 1, y], y * step),
                    new Vector3(x * step, map[x, y], y * step)
                });
                uvs.AddRange(new Vector2[] { 
                    //левый верхний треугольник
                    new Vector2((x - 1) * step, (y - 1) * step),
                    new Vector2(x * step, (y - 1) * step),
                    new Vector2((x - 1) * step, y * step),
                    //правый нижний треугольник
                    new Vector2((x - 1) * step, y * step),
                    new Vector2(x * step, (y - 1) * step),
                    new Vector2(x * step, y * step)
                });

                for (int i = 0; i < 6; i++)
                    triangles.Add(currID + i);

                currID += 6;
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        return mesh;
    }
}
