using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NoiseSettings))]
public class NoiseSettingsEditor : Editor
{

    public override void OnInspectorGUI()
    {
        NoiseSettings settings = target as NoiseSettings;

        DrawDefaultInspector();
    }
}
