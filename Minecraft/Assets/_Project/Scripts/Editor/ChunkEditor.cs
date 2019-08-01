using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Chunk))]
public class ChunkEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Chunk chunk = target as Chunk;

        DrawDefaultInspector();
    }
}
