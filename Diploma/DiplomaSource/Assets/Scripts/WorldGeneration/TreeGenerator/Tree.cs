using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour
{
    // Общие переменные
    public Material TreeMaterial;

    private Mesh _mesh;

    private List<Vertex> _vertices;
    private List<int> _triangles;
    private List<Vector3> _origins;

    // Параметры ствола
    public float Height = 2;
    public float Radius = 0.2f;
    [Range (0f, 1f)]
    public float RadiusScaleIntencity = 1;
    public int Edges = 6;

    public Vector2 SloapModifier = Vector2.zero;

    // Параметры веток
    [Range (0.5f, 1f)]
    public float BranchRadiusModifier = 0.8f;
    public float BranchCountHorizontal = 1;
    public float BranchCountVertical = 0.5f;
    public float BranchGenerateHeight = 1;

    private void Start()
    {
        // Добавление необходимых компонентов
        _mesh = gameObject.AddComponent<MeshFilter>().mesh;
        gameObject.AddComponent<MeshRenderer>().material = TreeMaterial;

        _mesh.Clear();

        // Инициализация списков
        _vertices = new List<Vertex>();
        _triangles = new List<int>();
        _origins = new List<Vector3>();

        // Генерация меша дерева
        GenerateTree();

        // Передача данных мешу
        Vector3[] vertices = new Vector3[_vertices.Count];
        Vector2[] uv = new Vector2[_vertices.Count];
        Color[] colors = new Color[_vertices.Count];

        for (int i = 0; i < _vertices.Count; i++) {
            vertices[i] = _vertices[i].position;
            uv[i] = _vertices[i].uv;
            colors[i] = _vertices[i].vertexColor;
        }

        _mesh.vertices = vertices;
        _mesh.uv = uv;
        _mesh.colors = colors;
        _mesh.triangles = _triangles.ToArray();
        _mesh.RecalculateNormals();
        print(_mesh.normals[0] + "\n" + vertices[0]);
    }

    //private void OnDrawGizmos()
    //{
    //    if (_vertices == null)
    //        return;
    //    Gizmos.color = Color.red;
    //    for (int i = 0; i < _vertices.Count; i++) {
    //        Gizmos.DrawSphere(_vertices[i].Position, 0.02f);
    //    }
    //}

    private void CreateTriangle(int v1, int v2, int v3)
    {
        _triangles.Add(v1);
        _triangles.Add(v2);
        _triangles.Add(v3);
    }

    public void GenerateTree()
    {
        // Инициализация переменных
        int count = (int)(Height / 0.5f);

        Vector3 prevOrigin = Vector3.zero;
        Vector3 origin = Vector3.zero;
        Vector3 nextOrigin = Vector3.zero;

        float radius = Radius;

        //Генерация массивов вершин, треугольников и цветов для передачи их мешу
        for (int i = 0; i < count; i++) {
            // Вычисление позиции следующих вершин
            nextOrigin += new Vector3(Random.Range(-0.1f, 0.1f) + SloapModifier.x, Height / count, Random.Range(-0.1f, 0.1f) + SloapModifier.y);

            // Определение наклона вершин
            Vector3 axis = Vector3.Cross(nextOrigin - origin, Vector3.up).normalized;
            print(axis);

            float angle = Vector3.Angle(prevOrigin - origin, nextOrigin - origin) - Vector3.Angle(Vector3.up, (prevOrigin - origin));
            print(angle);

            RotationMatrix matrix = new RotationMatrix(axis, angle);

            // Генерация вершин и треугольников
            for (int j = 0; j < Edges; j++) {
                float a = 2 * Mathf.PI / Edges;

                _vertices.Add(new Vertex(
                    matrix.Rotate(new Vector3(radius * Mathf.Cos(a * j), 0, radius * Mathf.Sin(a * j))) + origin,
                    new Color(0.416f, 0.266f, 0.154f),
                    origin
                ));
                _vertices.Add(new Vertex(
                    matrix.Rotate(new Vector3(radius * Mathf.Cos(a * (j + 1)), 0, radius * Mathf.Sin(a * (j + 1)))) + origin,
                    new Color(0.416f, 0.266f, 0.154f),
                    origin
                ));
            }
            _origins.Add(origin);

            // Изменение радиуса и позиции вершин
            prevOrigin = origin;
            origin = nextOrigin;
            radius -= Radius / count * RadiusScaleIntencity;
        }

        // Создание треугольников из вершин
        for (int i = 0; i < _vertices.Count - Edges * 2; i += 2) {
            CreateTriangle(i, i + Edges * 2, i + Edges * 2 + 1);
            CreateTriangle(i, i + Edges * 2 + 1, i + 1);
        }

        //Генерация ветвей
        for (int i = 0; i < count; i++)
            if (_origins[i].y >= BranchGenerateHeight && Random.Range(0, 1) <= BranchCountVertical) {
                Vector3 dir = new Vector3(1, Random.Range(0, 0.2f), 0);

                RotationMatrix matrix = new RotationMatrix(Vector3.up, Random.Range(0, 360));

                for (int j = 0; j < BranchCountHorizontal; j++) {
                    dir = matrix.Rotate(dir);
                    GenerateBranch(_origins[i], dir, Radius - Radius / count * i, (Height - _origins[i].y) / 2);
                }
            }
    }

    public void GenerateBranch(Vector3 position, Vector3 direction, float startRadius, float length)
    {
        // Инициализация переменных
        int startID = _vertices.Count;
        int count = (int)(length / 0.25f);
        float radius = startRadius;
        direction = direction.normalized * length / count;

        Vector3 prevOrigin = position - direction;
        Vector3 origin = position;
        Vector3 nextOrigin = position;

        //Генерация массивов вершин, треугольников и цветов для передачи их мешу
        for (int i = 0; i < count; i++) {
            nextOrigin += direction + Vector3.one * Random.Range(-0.03f, 0.03f);

            // Определение оси и угла наклона вершин
            Vector3 axis = Vector3.Cross(nextOrigin - origin, Vector3.up).normalized;
            print(axis);

            float angle = Vector3.Angle(prevOrigin - origin, nextOrigin - origin) - Vector3.Angle(Vector3.up, (prevOrigin - origin));
            print(angle);

            RotationMatrix matrix = new RotationMatrix(axis, angle);

            // Генерация вершин и треугольников
            for (int j = 0; j < Edges; j++) {
                float a = 2 * Mathf.PI / Edges;

                _vertices.Add(new Vertex(
                    matrix.Rotate(new Vector3(radius * Mathf.Cos(a * j), 0, radius * Mathf.Sin(a * j))) + origin,
                    new Color(0.416f, 0.266f, 0.154f),
                    origin
                ));
                _vertices.Add(new Vertex(
                    matrix.Rotate(new Vector3(radius * Mathf.Cos(a * (j + 1)), 0, radius * Mathf.Sin(a * (j + 1)))) + origin,
                    new Color(0.416f, 0.266f, 0.154f),
                    origin
                ));
            }
            // Изменение радиуса и позиции вершин
            prevOrigin = origin;
            origin = nextOrigin;
            radius -= startRadius / count * RadiusScaleIntencity;
        }

        // Создание треугольников из вершин
        for (int i = startID; i < _vertices.Count - Edges * 2; i += 2) {
            CreateTriangle(i + Edges * 2 + 1, i + Edges * 2, i);
            CreateTriangle(i + 1, i + Edges * 2 + 1, i);
        }
    }
}
