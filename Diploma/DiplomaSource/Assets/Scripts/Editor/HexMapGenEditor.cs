using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(HexMapGenerator))]
public class HexMapGenEditor : Editor
{
    public override void OnInspectorGUI()
    {
        HexMapGenerator mapGen = (HexMapGenerator)target;

        if (DrawDefaultInspector() && mapGen.AutoUpdate) {
            mapGen.GenerateMap();
        }

        if (GUILayout.Button("Generate"))
            mapGen.GenerateMap();
    }
}