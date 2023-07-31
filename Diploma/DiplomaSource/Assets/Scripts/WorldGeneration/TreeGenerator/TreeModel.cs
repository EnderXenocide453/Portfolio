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
        //Разделение питания на одну из ветвей
        Ratio, 
        //Степень отклонения ветвей от начального направления
        Spread, 
        //Размер ветви, при котором она делится
        SplitSize, 
        //Уменьшение питания, переходящего на следующую ветку
        SplitDecay, 
        //Направленность веток. Чем меньше, тем хаотичнее
        Directedness;

    public Material BranchMaterial;
    public Material LeavesMaterial;

    //Коэффициент сужения ветвей
    [Range(0.1f, 1)]
    public float Taper;
    
    //Количество ветвей
    public int BranchCount = 0;

    //Данные меша
    public List<Vector3> Vertices;
    public List<int> Triangles;
    public List<Vector3> FarVertices;
    public List<int> FarTriangles;

    //Начальная ветвь
    public Branch root { get; private set; }

    //Компоненты отрисовки дерева
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;

    //Для оптимизации дерева вдали
    private MeshFilter _farMeshFilter;
    private MeshRenderer _farMeshRenderer;

    private MeshCollider _collider;

    //Объект листвы
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

    //Функция роста дерева на заранее заданное значение
    public void NormalGrow()
    {
        int count = (int)(Feed / 0.1f);

        for (int i = 0; i < count; i++) root.Grow(0.1f);
        root.Grow(Feed - count * 0.1f);
        Feed = 0;
        CreateMesh();
    }

    //Функция, вызываемая при отображении дерева
    protected override void Enable()
    {
        base.Enable();

        if (Feed > 0)
            NormalGrow();
    }

    //Функция, вызываемая при попадании дерева в радиус отображения
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(TriggerType.TreeTrigger.ToString()))
            DisableFarMesh();
    }

    //Функция, вызываемая при выходе дерева из радиуса отображения
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(TriggerType.TreeTrigger.ToString()))
            EnableFarMesh();
    }

    //Функция включения дерева, пердставляющего собой набор картинок
    private void EnableFarMesh()
    {
        _meshRenderer.enabled = false;
        _farMeshRenderer.enabled = true;
    }
    //Функция включения обычной модели дерева
    private void DisableFarMesh()
    {
        _meshRenderer.enabled = true;
        _farMeshRenderer.enabled = false;
    }

    //Функция генерации формы дерева
    public void GenerateTree()
    {
        Vertices = new List<Vector3>();
        Triangles = new List<int>();
        FarVertices = new List<Vector3>();
        FarTriangles = new List<int>();

        root = new Branch(this, Ratio, Spread, SplitSize, SplitDecay, Directedness, 0, Seed);
    }

    //Функция генерации полигональной сетки вокруг формы
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

    //Параметры идентификации
    public TreeModel Tree { get; private set; }
    public int ID { get; private set; } = 0;
    public bool leaf { get; private set; } = true;
    public int depth { get; private set; } = 0;

    public Branch parent { get; private set; }
    public Branch a { get; private set; }
    public Branch b { get; private set; }

    //Параметры роста
    private float _ratio, _spread, _splitSize, _splitDecay, _directedness;

    //Параметры направления и размера
    public Vector3 dir { get; private set; } = Vector3.up;
    public float length { get; private set; } = 0;
    public float radius { get; private set; } = 0.01f;
    public float area { get; private set; } = 0.1f;

    //Параметры листвы листву
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

    //Рост
    public void Grow(float feed)
    {
        radius = Mathf.Sqrt(area / Mathf.PI);
        if (parent != null)
            branchFeed = parent.branchFeed + feed;

        //Если потомков нет
        if (leaf) {
            length += Mathf.Pow(feed, 1f / 3f);
            feed -= Mathf.Pow(feed, 1f / 3f) * area;
            area += feed / length;

            //Условие разветвления
            if (length > _splitSize * Mathf.Exp(-_splitDecay * depth))
                Split();

            return;
        }

        //Если потомки есть
        float pass = (a.area + b.area) / (a.area + b.area + area); //Расчёт коэффициента передачи питания

        area += pass * feed / length; //Увеличение обхвата
        feed *= 1 - pass; //Уменьшение объема питания

        //Передача питания потомкам
        //ratio - степень ассиметрии ветвей
        a.Grow(feed * _ratio);
        b.Grow(feed * (1 - _ratio));
    }

    //Разветвление
    public void Split()
    {
        leaf = false;

        a = new Branch(this, false, _rand.Next());
        b = new Branch(this, false, _rand.Next());

        a.ID = ID * 2 + 1;
        b.ID = ID * 2 + 2;

        //Направление плотности листьев
        Vector3 density = ComputeLeafDensity(depth - 1);
        //Нормаль
        Vector3 normal = Vector3.Cross(dir, density).normalized;

        //Выбор направления
        int flip = (_rand.Next() % 2) == 0 ? -1 : 1;

        //Объединение направлений дочерних веток и текущей
        a.dir = Vector3.Lerp(normal * _spread * flip, dir, _ratio).normalized;
        b.dir = Vector3.Lerp(-normal * _spread * flip, dir, 1 - _ratio).normalized;
    }

    //Расчет направления плотности листьев рекурсивным поиском
    public Vector3 ComputeLeafDensity(int searchDepth)
    {
        Vector3 randVec = new Vector3((float)_rand.NextDouble() - 0.5f, (float)_rand.NextDouble() - 0.5f, (float)_rand.NextDouble() - 0.5f);

        //Случайное направление для ствола
        if (depth == 0)
            return randVec;

        //Расчёт направления относительно старшей ветви / ствола
        Branch currBranch = this;
        Vector3 relativeDir = Vector3.zero;
        while (currBranch.depth > 0 && searchDepth-- >= 0) {
            relativeDir += currBranch.length * currBranch.dir;
            currBranch = currBranch.parent;
        }

        return _directedness * (GetLeafAverageDir(currBranch) - relativeDir) + (1 - _directedness) * randVec;
    }

    //Рекурсивный расчёт общего направления веток
    public Vector3 GetLeafAverageDir(Branch branch)
    {
        if (branch.leaf) 
            return branch.length * branch.dir;

        return branch.length * branch.dir + _ratio * GetLeafAverageDir(branch.a) + (1 - _ratio) * GetLeafAverageDir(branch.b);
    }
}