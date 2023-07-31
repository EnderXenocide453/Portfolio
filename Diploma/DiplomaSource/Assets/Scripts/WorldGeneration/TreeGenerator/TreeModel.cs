using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeModel : HidebleObject
{
    public bool Hide = true;
    public bool Grow = false;
    public float GrowDelay = 0.2f, GrowFeed = 0.1f;

    public float GrowSpeed = 1;
    public float Feed = 1;

    public int Seed = 0;
    [Range(3, 10)]
    public int RingSize = 6;

    public float RadiusScale = 1, LengthScale = 1;
    public float 
        //���������� ������� �� ���� �� ������
        Ratio, 
        //������� ���������� ������ �� ���������� �����������
        Spread, 
        //������ �����, ��� ������� ��� �������
        SplitSize, 
        //���������� �������, ������������ �� ��������� �����
        SplitDecay, 
        //�������������� �����. ��� ������, ��� ���������
        Directedness;

    public Material BranchMaterial;
    public Material LeavesMaterial;

    //����������� ������� ������
    [Range(0.1f, 1)]
    public float Taper;
    
    //���������� ������
    public int BranchCount = 0;

    //������ ����
    public List<Vector3> Vertices;
    public List<int> Triangles;
    public List<Vector3> FarVertices;
    public List<int> FarTriangles;

    //��������� �����
    public Branch root { get; private set; }

    //���������� ��������� ������
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;

    //��� ����������� ������ �����
    private MeshFilter _farMeshFilter;
    private MeshRenderer _farMeshRenderer;

    private MeshCollider _collider;

    //������ ������
    public GameObject LeavesObj;
    public float LeavesScale = 10;
    public float MinLeavesRadius = 0.5f;

    protected override void Start()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
        
        _farMeshFilter = transform.GetChild(0).GetComponent<MeshFilter>();
        _farMeshRenderer = transform.GetChild(0).GetComponent<MeshRenderer>();

        _collider = GetComponent<MeshCollider>();

        GenerateTree();
        DisableFarMesh();

        if (Hide) {
            EnableFarMesh();
            base.Start();
        }
        if (Grow)
            StartCoroutine(DynamicGrow(GrowDelay, GrowFeed));
    }

    //������� ����� ������ �� ������� �������� ��������
    public void NormalGrow()
    {
        int count = (int)(Feed / 0.1f);

        for (int i = 0; i < count; i++) root.Grow(0.1f);
        root.Grow(Feed - count * 0.1f);
        Feed = 0;
        CreateMesh();
    }

    //�������, ���������� ��� ����������� ������
    protected override void Enable()
    {
        base.Enable();

        if (Feed > 0)
            NormalGrow();
    }

    //�������, ���������� ��� ��������� ������ � ������ �����������
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(TriggerType.TreeTrigger.ToString()))
            DisableFarMesh();
    }

    //�������, ���������� ��� ������ ������ �� ������� �����������
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(TriggerType.TreeTrigger.ToString()))
            EnableFarMesh();
    }

    //������� ��������� ������, ��������������� ����� ����� ��������
    private void EnableFarMesh()
    {
        _meshRenderer.enabled = false;
        _farMeshRenderer.enabled = true;
    }
    //������� ��������� ������� ������ ������
    private void DisableFarMesh()
    {
        _meshRenderer.enabled = true;
        _farMeshRenderer.enabled = false;
    }

    //������� ��������� ����� ������
    public void GenerateTree()
    {
        Vertices = new List<Vector3>();
        Triangles = new List<int>();
        FarVertices = new List<Vector3>();
        FarTriangles = new List<int>();

        root = new Branch(this, Ratio, Spread, SplitSize, SplitDecay, Directedness, 0, Seed);
    }

    //������� ��������� ������������� ����� ������ �����
    public void CreateMesh()
    {
        Mesh mesh = new Mesh();
        Mesh farMesh = new Mesh();

        Vertices = new List<Vector3>();
        Triangles = new List<int>();
        FarVertices = new List<Vector3>();
        FarTriangles = new List<int>();

        TreeModelGenerator.GenerateTreeModel(this);

        mesh.vertices = Vertices.ToArray();
        mesh.triangles = Triangles.ToArray();

        mesh.RecalculateNormals();

        _meshFilter.sharedMesh = mesh;
        _collider.sharedMesh = mesh;

        farMesh.vertices = FarVertices.ToArray();
        farMesh.triangles = FarTriangles.ToArray();

        farMesh.RecalculateNormals();
        _farMeshFilter.sharedMesh = farMesh;
    }

    public IEnumerator DynamicGrow(float delay, float feed)
    {
        while (true) {
            yield return new WaitForSeconds(delay);
            root.Grow(feed);
            CreateMesh();

            if (_meshFilter.sharedMesh.vertexCount >= 5000) break;
        }
    }
}

public class Branch
{
    public int Seed { get; private set; } = 0;

    private System.Random _rand;

    //��������� �������������
    public TreeModel Tree { get; private set; }
    public int ID { get; private set; } = 0;
    public bool leaf { get; private set; } = true;
    public int depth { get; private set; } = 0;

    public Branch parent { get; private set; }
    public Branch a { get; private set; }
    public Branch b { get; private set; }

    //��������� �����
    private float _ratio, _spread, _splitSize, _splitDecay, _directedness;

    //��������� ����������� � �������
    public Vector3 dir { get; private set; } = Vector3.up;
    public float length { get; private set; } = 0;
    public float radius { get; private set; } = 0.01f;
    public float area { get; private set; } = 0.1f;

    //��������� ������ ������
    public LeavesModel leaves;
    public float branchFeed { get; private set; } = 0;

    public Branch(TreeModel tree, float ratio, float spread, float splitSize, float splitDecay, float directedness, float initialFeed, int seed = 0)
    {
        Tree = tree;
        tree.BranchCount++;

        branchFeed = initialFeed;

        _ratio = ratio;
        _spread = spread;
        _splitSize = splitSize;
        _splitDecay = splitDecay;
        _directedness = directedness;

        Seed = seed;
        _rand = new System.Random(seed);
    }

    public Branch(Branch parent, bool root, int seed = 0)
    {
        Tree = parent.Tree;
        Tree.BranchCount++;

        radius = parent.radius;
        branchFeed = parent.branchFeed;

        _ratio = parent._ratio;
        _spread = parent._spread;
        _splitSize = parent._splitSize;
        _splitDecay = parent._splitDecay;
        _directedness = parent._directedness;

        Seed = seed;
        _rand = new System.Random(seed);

        if (root) return;
        depth = parent.depth + 1;
        this.parent = parent;
    }

    //����
    public void Grow(float feed)
    {
        radius = Mathf.Sqrt(area / Mathf.PI);
        if (parent != null)
            branchFeed = parent.branchFeed + feed;

        //���� �������� ���
        if (leaf) {
            length += Mathf.Pow(feed, 1f / 3f);
            feed -= Mathf.Pow(feed, 1f / 3f) * area;
            area += feed / length;

            //������� ������������
            if (length > _splitSize * Mathf.Exp(-_splitDecay * depth))
                Split();

            return;
        }

        //���� ������� ����
        float pass = (a.area + b.area) / (a.area + b.area + area); //������ ������������ �������� �������

        area += pass * feed / length; //���������� �������
        feed *= 1 - pass; //���������� ������ �������

        //�������� ������� ��������
        //ratio - ������� ���������� ������
        a.Grow(feed * _ratio);
        b.Grow(feed * (1 - _ratio));
    }

    //������������
    public void Split()
    {
        leaf = false;

        a = new Branch(this, false, _rand.Next());
        b = new Branch(this, false, _rand.Next());

        a.ID = ID * 2 + 1;
        b.ID = ID * 2 + 2;

        //����������� ��������� �������
        Vector3 density = ComputeLeafDensity(depth - 1);
        //�������
        Vector3 normal = Vector3.Cross(dir, density).normalized;

        //����� �����������
        int flip = (_rand.Next() % 2) == 0 ? -1 : 1;

        //����������� ����������� �������� ����� � �������
        a.dir = Vector3.Lerp(normal * _spread * flip, dir, _ratio).normalized;
        b.dir = Vector3.Lerp(-normal * _spread * flip, dir, 1 - _ratio).normalized;
    }

    //������ ����������� ��������� ������� ����������� �������
    public Vector3 ComputeLeafDensity(int searchDepth)
    {
        Vector3 randVec = new Vector3((float)_rand.NextDouble() - 0.5f, (float)_rand.NextDouble() - 0.5f, (float)_rand.NextDouble() - 0.5f);

        //��������� ����������� ��� ������
        if (depth == 0)
            return randVec;

        //������ ����������� ������������ ������� ����� / ������
        Branch currBranch = this;
        Vector3 relativeDir = Vector3.zero;
        while (currBranch.depth > 0 && searchDepth-- >= 0) {
            relativeDir += currBranch.length * currBranch.dir;
            currBranch = currBranch.parent;
        }

        return _directedness * (GetLeafAverageDir(currBranch) - relativeDir) + (1 - _directedness) * randVec;
    }

    //����������� ������ ������ ����������� �����
    public Vector3 GetLeafAverageDir(Branch branch)
    {
        if (branch.leaf) 
            return branch.length * branch.dir;

        return branch.length * branch.dir + _ratio * GetLeafAverageDir(branch.a) + (1 - _ratio) * GetLeafAverageDir(branch.b);
    }
}