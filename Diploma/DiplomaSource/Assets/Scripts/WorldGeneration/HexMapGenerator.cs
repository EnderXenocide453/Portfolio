using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class HexMapGenerator : MonoBehaviour
{
    [Header("������� �����")]
    [Range(1, 100)]
    public int SubdivisionLevel = 1;
    public float ChunkRadius = 1;

    public int MapRadius = 1;

    //Land generation
    [Header("����� �������")]
    public int ShapeSeed = 0;
    [Range(0, 1)]
    public float OceanIntence = 0.5f;
    public float OceanDistanceInfluence = 1;
    public AnimationCurve OceanCurve;

    public float HeightMultiplier = 1;
    
    [Header("��������� �������")]
    public float ReliefNoiseScale = 1;
    public float ReliefH = 0.5f;
    public int ReliefOctavesCount = 3;

    public float LandNoiseScale = 1;
    public int LandOctavesCount = 1;

    public float Persistance = 0.5f;
    public float lacunarity = 1;

    public Vector2 NoiseOffset = Vector2.zero;

    [Header("������")]
    public int ErosionSeed = 0;
    public int ErosionIterations = 10;

    public float ErosionRadius = 1;
    [Range(0, 1)]
    public float ErosionInertia = 0.05f;
    public float SedimentCapacityFactor = 4;
    public float MinSedimentCapacity = .01f;
    [Range(0, 10)]
    public float ErodeSpeed = .3f;
    [Range(0, 10)]
    public float DepositSpeed = .3f;
    [Range(0, 1)]
    public float EvaporateSpeed = .01f;
    public float Gravity = 4;
    public int MaxDropletLifetime = 30;

    public float InitialWaterVolume = 1;
    public float InitialSpeed = 1;

    [Header("��������� ���������� ��������")]
    public int TreeSeed = 0;

    public TreeInfo TreeParameters;
    public float MinStartFeed;
    public float MaxStartFeed;

    [Header("����������� � ����")]
    public Text SeedText;

    [Header("������")]
    public bool AutoUpdate;
    public Transform Player;

    private Dictionary<Vector2Int, HexChunk> _chunks;
    private Dictionary<Vector2Int, Mesh> _meshes;
    private MapDisplay _mapDisplay;

    private void Awake()
    {
        _mapDisplay = FindObjectOfType<MapDisplay>();
        GenerateMap(true);
    }

    //��� ������� ����� ������������ ��� � ����������� �������
    private void Start()
    {
        SeedText.text = "����� ���������: " + ShapeSeed; //����������� ����� ��������� � ���� �����
        WorldTreesPlacer.PlaceTrees(_chunks, HeightMultiplier, TreeSeed, 10, 0.2f, TreeParameters);

        RaycastHit hit;
        Physics.Raycast(Vector3.up * HeightMultiplier * 1.5f, Vector3.down, out hit);
        Debug.Log(hit.point);
        Player.GetComponent<CharacterController>().enabled = false;
        Player.position = hit.point + Vector3.up;
        Player.GetComponent<CharacterController>().enabled = true;
    }

    //������� ��������� ����
    public void GenerateMap(bool useOtherSettings = false)
    {
        //���� ���������� ������� � ��������� �� ����, ����� ������������ ���� �������� �� ��������
        if (useOtherSettings) {
            ErosionRadius = ErosionRadius / MapRadius / ChunkRadius;

            ShapeSeed = WorldLoader.Seed;
            HeightMultiplier = WorldLoader.HeightScale;
            ChunkRadius = WorldLoader.ChunkRadius;
            MapRadius = WorldLoader.MapRadius;
            OceanIntence = WorldLoader.OceanHeight;
            ErosionIterations = WorldLoader.ErosionIterations;

            ErosionRadius *= MapRadius * ChunkRadius;
        }
        //�������� �������� � ����� VerticesContainer ��� ����������� ������� � ��� ��������
        VerticesContainer.Clear();
        VerticesContainer.GetContainer().HeightScale = HeightMultiplier;
        VerticesContainer.GetContainer().ChunkRadius = ChunkRadius;
        VerticesContainer.GetContainer().MapRadius = MapRadius;
        VerticesContainer.GetContainer().SubdivisionLevel = SubdivisionLevel;
        VerticesContainer.GetContainer().CalculateCorners();
        VerticesContainer.GetContainer().OceanCurve = OceanCurve == null ? new AnimationCurve(new Keyframe[] { new Keyframe(0, 0), new Keyframe(1, 1) }) : OceanCurve;

        //������������� ����������� ��������������� �����
        System.Random shapeRand = new System.Random(ShapeSeed);
        VerticesContainer.Random = new System.Random(ShapeSeed);

        //��������� ����� �����
        _chunks = TerrainMeshGenerator.GenerateMultiHexMesh(ChunkRadius, MapRadius, SubdivisionLevel, _mapDisplay.Materials == null ? 1 : _mapDisplay.Materials.Length);

        //��������� �������
        HexNoises.GeneratePerlinNoise(new Vector2(shapeRand.Next(10000), shapeRand.Next(10000)), ReliefNoiseScale, ReliefOctavesCount, Persistance, lacunarity);
        //��������� ������� �� ����� �������
        HexNoises.GenerateLand(new Vector2(shapeRand.Next(10000), shapeRand.Next(10000)), LandNoiseScale, OceanIntence, OceanDistanceInfluence);
        //������ �������
        ErosionGenerator.ErodeTerrain(this, ErosionIterations);

        //����������� ������������� �����
        DrawMesh();
        //��������� ������ ����
        GameObject.Find("Water").transform.position = Vector3.up * OceanIntence * HeightMultiplier * 0.8f;
    }

    //������� ����������� ������������� �����
    public void DrawMesh()
    {
        _mapDisplay.DrawMesh(_chunks, HeightMultiplier, out _meshes);
        _mapDisplay.DrawWaterMesh(_chunks);
        //Lightmapping.Bake();
    }

    //�����-��������� ��� �������� ���������� � ���������� ���������� ��������
    [System.Serializable]
    public class TreeInfo
    {
        public float MinGrowSpeed = 1;
        public float MaxGrowSpeed = 1;
        public float MinFeed = 1;
        public float MaxFeed = 1;

        [Range(3, 10)]
        public int RingSize = 6;

        public float 
            MinRadiusScale = 1, 
            MaxRadiusScale = 1, 
            MinLengthScale = 1, 
            MaxLengthScale = 1,
            MinRatio,
            MaxRatio,
            MinSpread,
            MaxSpread,
            MinSplitSize,
            MaxSplitSize,
            MinSplitDecay,
            MaxSplitDecay,
            MinDirectedness,
            MaxDirectedness;

        public float MinLeavesScale = 5,
            MaxLeavesScale = 15,
            MinInitLeavesRadius = 0.1f,
            MaxInitLeavesRadius = 0.5f;
    }

    private void OnValidate()
    {
        //MapRadius = MapRadius < 2 ? 2 : MapRadius;
        if (_mapDisplay == null) _mapDisplay = FindObjectOfType<MapDisplay>();
    }
}
