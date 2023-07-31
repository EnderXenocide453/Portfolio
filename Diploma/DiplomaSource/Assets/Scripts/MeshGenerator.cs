using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ObjectMeshGenerator : MonoBehaviour
{
    // Параметры меша
    public int EdgesCount = 6;
    public float Width = 0.5f;

    public Material MeshMaterial;

    private Mesh _mesh;

    protected List<Vertex> _vertices;
    protected List<int> _triangles;
    protected List<Origin> _verticesOrigins;

    protected virtual void Start()
    {
        _mesh = gameObject.GetComponent<MeshFilter>().mesh;
        gameObject.GetComponent<MeshRenderer>().material = MeshMaterial;

        _mesh.Clear();

        // Инициализация списков
        _vertices = new List<Vertex>();
        _triangles = new List<int>();
        _verticesOrigins = new List<Origin>();

        // Генерация линий формы меша
        GenerateShape();

        // Генерация вершин и трекгольников меша
        GenerateMesh();

        // Передача данных мешу
        UpdateMesh();
    }

    protected virtual void GenerateShape() { }

    protected virtual void GenerateMesh() 
    {
        _verticesOrigins[0].SetID(0);
        for (int j = 0; j < EdgesCount; j++) {
            float a = 2 * Mathf.PI / EdgesCount;

            _verticesOrigins[0].AddVertex(new Vertex(
                new Vector3(Width * Mathf.Cos(a * j), 0, Width * Mathf.Sin(a * j)) + _verticesOrigins[0].position,
                new Color(0.416f, 0.266f, 0.154f),
                _verticesOrigins[0].position
            ));
            _verticesOrigins[0].AddVertex(new Vertex(
                new Vector3(Width * Mathf.Cos(a * (j + 1)), 0, Width * Mathf.Sin(a * (j + 1))) + _verticesOrigins[0].position,
                new Color(0.416f, 0.266f, 0.154f),
                _verticesOrigins[0].position
            ));
        }

        GenerateMeshAroundOrigin(_verticesOrigins[1], 1);
    }

    private int GenerateMeshAroundOrigin(Origin origin, int id)
    {
        origin.SetID(id);
        id++;
        origin.parent.ExtrudeVertices(origin);

        if (origin.GetChildren().Count == 0)
            return id;

        foreach (Origin child in origin.GetChildren()) {
            id = GenerateMeshAroundOrigin(child, id);
        }

        return id;
    }

    public void UpdateMesh()
    {
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
    }

    public void CreateTriangle(int v1, int v2, int v3)
    {
        _triangles.Add(v1);
        _triangles.Add(v2);
        _triangles.Add(v3);
    }

    public void AddVertex(Vertex vertex)
    {
        _vertices.Add(vertex);
    }

    public void AddVertices(IEnumerable<Vertex> vertices)
    {
        _vertices.AddRange(vertices);
    }

    protected Vector3 RotateVector(Vector3 axis, float angle, Vector3 vector)
    {
        return new RotationMatrix(axis, angle).Rotate(vector);
    }
}

public class RotationMatrix
{
    readonly float[,] m = new float[3, 3];

    public RotationMatrix(Vector3 axis, float angle)
    {
        angle *= Mathf.PI / 180;
        float sin = Mathf.Sin(angle), cos = Mathf.Cos(angle);
        float onemcos = 1 - cos;

        float ux = axis.x, uy = axis.y, uz = axis.z;

        m[0, 0] = cos + ux * ux * onemcos;
        m[0, 1] = ux * uy * onemcos - uz * sin;
        m[0, 2] = ux * uz * onemcos + uy * sin;

        m[1, 0] = uy * ux * onemcos + uz * sin;
        m[1, 1] = cos + uy * uy * onemcos;
        m[1, 2] = uy * uz * onemcos - ux * sin;

        m[2, 0] = uz * ux * onemcos - uy * sin;
        m[2, 1] = uz * uy * onemcos + ux * sin;
        m[2, 2] = cos + uz * uz * onemcos;
    }

    public Vector3 Rotate(Vector3 original)
    {
        return new Vector3(
            m[0, 0] * original.x + m[0, 1] * original.y + m[0, 2] * original.z,
            m[1, 0] * original.x + m[1, 1] * original.y + m[1, 2] * original.z,
            m[2, 0] * original.x + m[2, 1] * original.y + m[2, 2] * original.z);
    }
}

public class Vertex
{
    public Vector3 position { get; private set; }
    public Vector3 localPosition { get; private set; }
    public Vector2 uv { get; private set; }
    public Color vertexColor { get; private set; }
    public Vector3 origin { get; private set; }

    public Vertex(Vector3 position, Color color, Vector3 origin)
    {
        this.position = position;
        localPosition = position - origin;
        this.origin = origin;
        uv = new Vector2(position.x, position.z);
        vertexColor = color;
    }

    public Vertex GetExtruded(Vector3 origin)
    {
        return new Vertex(origin + localPosition, vertexColor, origin);
    }

    public Vertex SetPosition(Vector3 position) 
    {
        this.position = position;
        localPosition = position - origin;
        uv = new Vector2(position.x, position.z);

        return this;
    }

    public Vertex SetOffset(float offset)
    {
        SetPosition(localPosition.normalized * offset + origin);

        return this;
    }
}

public class Origin
{
    // Данные о центральной точке
    public Vector3 position { get; private set; }
    public Quaternion rotation { get; private set; }
    public Origin parent { get; private set; }
    public int id { get; private set; }

    private List<Origin> _children;

    // Данные о вершинах
    public float scale { get; private set; }
    private List<Vertex> _vertices;

    // Ссылка на генератор меша
    private ObjectMeshGenerator _generator;

    public Origin(Vector3 position, float scale, ObjectMeshGenerator generator)
    {
        this.position = position;
        parent = this;
        rotation = Quaternion.Euler(0, 0, 0);
        _children = new List<Origin>();

        this.scale = scale;
        _vertices = new List<Vertex>();

        _generator = generator;
    }

    public Origin SetID(int id)
    {
        this.id = id;

        return this;
    }

    public Origin SetParent(Origin origin)
    {
        parent = origin;
        origin.AddChild(this);
        return this;
    }

    public Origin AddChild(Origin origin)
    {
        _children.Add(origin);
        return this;
    }

    public Origin GetChild(int id) => _children[id];

    public List<Origin> GetChildren() => _children;
    public int GetChildrenCount() => _children.Count;

    public Origin AddVertex(Vertex vertex)
    {
        vertex.SetPosition(new RotationMatrix(new Vector3(rotation.x, rotation.y, rotation.z), rotation.w * 180 / Mathf.PI).Rotate(vertex.localPosition) + position);
        vertex.SetOffset(vertex.localPosition.magnitude * scale);

        _vertices.Add(vertex);
        _generator.AddVertex(vertex);
        return this;
    }

    public Origin AddVertices(IEnumerable<Vertex> newVertices)
    {
        foreach (Vertex vertex in newVertices) {
            AddVertex(vertex);
        }
        return this;
    }

    public List<Vertex> GetVertices() => _vertices;

    public Origin SetPosition(Vector3 pos)
    {
        foreach (Vertex vertex in _vertices)
            vertex.SetPosition(pos + vertex.localPosition);
        position = pos;
        return this;
    }

    public List<Vertex> ExtrudeVertices(Origin offsetOrigin)
    {
        // Выдавливание вершин и их передача следующей точке
        List<Vertex> extrVertices = new List<Vertex>();
        
        foreach (Vertex vertex in _vertices)
            extrVertices.Add(vertex.GetExtruded(offsetOrigin.position).SetOffset(_generator.Width / 2));

        offsetOrigin.AddVertices(extrVertices);
        offsetOrigin.rotation = rotation;
        //offsetOrigin.ClearRotation();

        // Генерацмя треугольников к выдавленным вершинам
        int currID = id * _vertices.Count;
        int nextID = offsetOrigin.id * _vertices.Count;
        for (int i = 0; i < _vertices.Count; i += 2) {
            _generator.CreateTriangle(currID + i, nextID + i, nextID + i + 1);
            _generator.CreateTriangle(currID + i, nextID + i + 1, currID + i + 1);
        }

        return extrVertices;
    }

    public Origin Rotate(Vector3 axis, float angle)
    {
        axis.Normalize();
        rotation *= Quaternion.AngleAxis(angle, axis);

        foreach (Vertex vertex in _vertices)
            vertex.SetPosition(new RotationMatrix(axis, angle).Rotate(vertex.localPosition) + position);

        return this;
    }

    public Origin Rotate(Quaternion quaternion)
    {
        rotation *= quaternion;

        foreach (Vertex vertex in _vertices)
            vertex.SetPosition(new RotationMatrix(new Vector3(quaternion.x, quaternion.y, quaternion.z), quaternion.w * 180 / Mathf.PI).Rotate(vertex.localPosition) + position);

        return this;
    }

    public Origin ClearRotation()
    {
        Rotate(new Vector3( rotation.x, rotation.y, rotation.z), -rotation.w * 180 / Mathf.PI);
        
        rotation = Quaternion.Euler(0, 0, 0);

        return this;
    }

    public Origin Scale(float scale)
    {
        this.scale *= scale;
        return this;
    }

    public Origin SetScale(float scale)
    {
        this.scale = scale;
        return this;
    }
}

/* TODO
 * Для каждой точки генерировать меш для квада от текущей точки до следующей
 */