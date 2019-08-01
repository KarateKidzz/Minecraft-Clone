using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SingleChunkGenerator))]
public class SingleChunkGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SingleChunkGenerator chunk = target as SingleChunkGenerator;

        DrawDefaultInspector();

        EditorGUI.BeginChangeCheck();

        Editor editor = CreateEditor(chunk.noiseSettings);
        editor.OnInspectorGUI();

        if (GUILayout.Button("Generate"))
        {
            chunk.Generate();
        }
    }
}
