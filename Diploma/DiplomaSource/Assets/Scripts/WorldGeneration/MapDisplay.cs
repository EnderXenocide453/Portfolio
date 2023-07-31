using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    //Ссылки на объекты, необходимые для создания полигональных сеток
    public Renderer TextureRenderer;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public MeshCollider meshCollider;

    public GameObject MeshObj;
    public GameObject WaterMeshObj;
    public Transform MeshParent;
    public Material TerrainMaterial;

    public Material[] Materials;

    private Transform[] _subMeshes;
    private Transform[] _waterSubMeshes;

    public void DrawTexture(Texture2D texture)
    {
        TextureRenderer.sharedMaterial.mainTexture = texture;
        TextureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height) / 10;
    }

    public void DrawMesh(MeshData meshData, Texture2D texture)
    {
        meshFilter.sharedMesh = meshData.CreateMesh();
        meshRenderer.sharedMaterial.mainTexture = texture;
        meshCollider.sharedMesh = meshFilter.sharedMesh;
    }

    public void DrawMesh(MeshData[,] chunksData, Vector2Int chunkSize, Texture2D texture)
    {
        if (_subMeshes != null && _subMeshes.Length > 0)
            for (int i = _subMeshes.Length - 1; i >= 0; i--)
                try {
                    DestroyImmediate(_subMeshes[i].gameObject);
                }
                catch { }

        int xCount = chunksData.GetLength(0);
        int yCount = chunksData.GetLength(1);

        _subMeshes = new Transform[xCount * yCount];

        for (int y = 0; y < yCount; y++)
            for (int x = 0; x < xCount; x++) {
                int id = x + y * xCount;
                _subMeshes[id] = Instantiate(MeshObj, new Vector3(x * (chunkSize.x - 1) * MeshObj.transform.localScale.x, 0, -y * (chunkSize.y - 1) * MeshObj.transform.localScale.z), Quaternion.identity).transform;
                Texture2D chunkTex = new Texture2D(chunkSize.x, chunkSize.y);

                for (int i = 0; i < chunkSize.x; i++)
                    for (int j = 0; j < chunkSize.y; j++)
                        chunkTex.SetPixel(i, j, texture.GetPixel(i + x * chunkSize.x, j + y * chunkSize.y));

                Mesh mesh = chunksData[x, y].CreateMesh();
                _subMeshes[id].GetComponent<MeshFilter>().sharedMesh = mesh;
                _subMeshes[id].GetComponent<MeshCollider>().sharedMesh = mesh;

                Material material = new Material(TerrainMaterial);
                material.mainTexture = texture;
                material.mainTextureScale = new Vector2(1 / (float)xCount, 1 / (float)yCount);
                material.mainTextureOffset = new Vector2((float)x / (float)xCount, (float)y / (float)yCount);
                _subMeshes[id].GetComponent<MeshRenderer>().sharedMaterial = material;
            }
    }

    public void DrawMesh(Dictionary<Vector2Int, HexChunk> chunks, float heightMultiplier, out Dictionary<Vector2Int, Mesh> meshes)
    {
        meshes = new Dictionary<Vector2Int, Mesh>();

        if (MeshParent != null)
            DestroyImmediate(MeshParent.gameObject);
        MeshParent = new GameObject("terrainMeshes").transform;
        MeshParent.localScale = new Vector3(1, heightMultiplier, 1);

        _subMeshes = new Transform[chunks.Count];

        int id = 0;
        foreach (Vector2Int chunkPos in chunks.Keys) {
            _subMeshes[id] = Instantiate(MeshObj, chunks[chunkPos].Offset, Quaternion.identity, MeshParent).transform;
            MeshFilter filter = _subMeshes[id].GetComponent<MeshFilter>();
            MeshCollider collider = _subMeshes[id].GetComponent<MeshCollider>();
            ChunkInfo chunkInfo = _subMeshes[id].GetComponent<ChunkInfo>();

            if (Materials == null) {
                Material mat = new Material(TerrainMaterial);
                _subMeshes[id].GetComponent<MeshRenderer>().sharedMaterial = mat;
            } else {
                _subMeshes[id].GetComponent<MeshRenderer>().sharedMaterials = Materials;
            }

            Mesh mesh = chunks[chunkPos].CreateMesh();
            meshes.Add(chunkPos, mesh);

            collider.sharedMesh = mesh;
            filter.sharedMesh = mesh;

            chunkInfo.SetInfo(chunks[chunkPos].CubeCoord, chunkPos, mesh);
            chunks[chunkPos].ChunkInfo = chunkInfo;

            id++;
        }
    }

    public void DrawWaterMesh(Dictionary<Vector2Int, HexChunk> chunks)
    {
        _waterSubMeshes = new Transform[chunks.Count];

        int id = 0;
        foreach (Vector2Int chunkPos in chunks.Keys) {
            if (chunks[chunkPos].WTriangles.Count == 0) continue;//Если нет ни одного треугольника, то пропуск

            _waterSubMeshes[id] = Instantiate(WaterMeshObj, chunks[chunkPos].Offset, Quaternion.identity, MeshParent).transform;
            MeshFilter filter = _waterSubMeshes[id].GetComponent<MeshFilter>();
            MeshCollider collider = _waterSubMeshes[id].GetComponent<MeshCollider>();
            ChunkInfo chunkInfo = _waterSubMeshes[id].GetComponent<ChunkInfo>();

            Mesh mesh = chunks[chunkPos].WCreateMesh();

            collider.sharedMesh = mesh;
            filter.sharedMesh = mesh;

            chunkInfo.SetInfo(chunks[chunkPos].CubeCoord, chunkPos, mesh);
            chunks[chunkPos].ChunkInfo = chunkInfo;

            id++;
        }
    }

    public void RedrawMaterials(Dictionary<Vector2Int, HexChunk> chunks)
    {
        foreach (Vector2Int chunkPos in chunks.Keys) {
            chunks[chunkPos].RedrawMaterials();
        }
    }
}
