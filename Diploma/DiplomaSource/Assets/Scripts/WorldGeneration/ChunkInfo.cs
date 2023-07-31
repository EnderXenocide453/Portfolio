using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkInfo : MonoBehaviour
{
    public Mesh Mesh { get; private set; }

    public Vector3Int _cubeCoordinates { get; private set; }
    public Vector2Int _displaceCoord { get; private set; }

    public void SetInfo(Vector3Int cube, Vector2Int displace, Mesh mesh)
    {
        _cubeCoordinates = cube;
        _displaceCoord = displace;

        Mesh = mesh;
    }
    public Vector3Int GetCoord() => _cubeCoordinates;
}
