using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(InfiniteTerrain))]
public class InfiniteTerrainEditor : Editor
{
    InfiniteTerrain infiniteTerrain;
    bool mainFoldout = true;
    bool biomeFoldout;
    bool flatFoldout;
    bool lightForestFoldout;
    bool heavyForestFoldout;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        mainFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(mainFoldout, "Noise Settings");
        if (mainFoldout)
        {
            biomeFoldout = EditorGUILayout.Foldout(biomeFoldout, "Biome Noise Settings");
            if (biomeFoldout)
            {
                DrawSettingsEditor(infiniteTerrain.BiomeNoiseSettings);
            }

            flatFoldout = EditorGUILayout.Foldout(flatFoldout, "Flat Biome Noise Settings");
            if (flatFoldout)
            {
                DrawSettingsEditor(infiniteTerrain.FlatBiomeSettings);
            }

            lightForestFoldout = EditorGUILayout.Foldout(lightForestFoldout, "Light Forest Noise Settings");
            if (lightForestFoldout)
            {
                DrawSettingsEditor(infiniteTerrain.LightForestSettings);
            }

            heavyForestFoldout = EditorGUILayout.Foldout(heavyForestFoldout, "Heavy Forest Noise Settings");
            if (heavyForestFoldout)
            {
                DrawSettingsEditor(infiniteTerrain.HeavyForestSettings);
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    void DrawSettingsEditor (Object settings)
    {
        if (settings != null)
        {
            Editor editor = CreateEditor(settings);

            editor.OnInspectorGUI();
        }
    }

    void OnEnable()
    {
        infiniteTerrain = (InfiniteTerrain)target;
    }
}
