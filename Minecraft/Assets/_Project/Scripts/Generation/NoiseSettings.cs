using System;
using UnityEngine;

[CreateAssetMenu]
public class NoiseSettings : ScriptableObject
{
    [Header("Ground Noise")]
    [Tooltip("Terrain Start Height")]
    [Range(-255, 255)]
    public int GroundHeight = 10;
    [Tooltip("Height")]
    [Range(1, 200)]
    public int Amplitude = 1;
    [Range(0, 256256)]
    public int Seed;
    [Tooltip("Detail")]
    [Range(1, 16)]
    public int Octaves = 1;
    [Tooltip("Height Increase Step")]
    [Range(1, 3000)]
    public int Smoothness = 1;
    [Tooltip("Balance between large and small objects. Values over 1 will exponentialy increase the terrain's height")]
    [Range(0, 2)]
    public float Roughness = 1;
    [Tooltip("A multiplier that determines how quickly the frequency increases for each successive octave")]
    [Range(0.1f, 4)]
    public float Lacunarity = 2;
    [Range(0, 100)]
    public int TreeFrequency = 60;

    [HideInInspector] public float[] weights = new float[16];

    public void SetWeights ()
    {
        CaveOne.CalculateWeights();
        CaveTwo.CalculateWeights();
    }

    public RidgedNoiseSettings CaveOne;
    public RidgedNoiseSettings CaveTwo;
}

[Serializable]
public class RidgedNoiseSettings
{
    [Range(0, 256256)]
    public int Seed;
    [Range(1, 16)]
    public int Octaves;
    [Range(0.1f, 100)]
    public float Frequency;
    [Range(1, 3000)]
    public int Smoothness = 1;
    [Range(0.1f, 3)]
    public float Exponent;
    [Range(0.1f, 10)]
    public float Gain;
    [Range(0, 10)]
    public float Offset;
    [Range(0.1f, 200)]
    public float Amplitude = 2;
    [Range(-255, 255)]
    public float HeightOffset = 10;
    [Range(0.1f, 4)]
    public float Lacunarity;

    public LibNoise.QualityMode Quality;

    [Range(0, 256)]
    public int LowCutoff = 0;
    [Range(256, 0)]
    public int HighCutoff = 256;

    [HideInInspector] public float[] Weights = new float[16];

    public RidgedNoiseSettings ()
    {
        CalculateWeights();
    }

    public void CalculateWeights ()
    {
        float f = 1.0f;
        for (var i = 0; i < 16; i++)
        {
            Weights[i] = Mathf.Pow(f, -Exponent);
            f *= Lacunarity;
        }
    }



}
