using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates height maps
/// </summary>
public static class NoiseGenerator
{
    const float simplexScale = 1 / 255;

    static SimplexNoise simplexNoise = new SimplexNoise(256145);

    /// <summary>
    /// Main Height Map Generation
    /// </summary>
    /// <returns>The d.</returns>
    /// <param name="chunkX">Chunk x.</param>
    /// <param name="chunkZ">Chunk z.</param>
    /// <param name="xSize">X size.</param>
    /// <param name="zSize">Z size.</param>
    /// <param name="octaves">Octaves.</param>
    /// <param name="amplitude">Amplitude.</param>
    /// <param name="smoothness">Smoothness.</param>
    /// <param name="heightOffset">Height offset.</param>
    /// <param name="roughness">Roughness.</param>
    public static float[,] Generate2D(int chunkX, int chunkZ, int xSize, int zSize, int octaves, int amplitude, int smoothness, int heightOffset, float roughness)
    {
        // Create array
        float[,] noise = new float[xSize, zSize];

        // Loop through X and Z positions
        // The value in the array is our height
        for (int x = 0; x < xSize; x++)
        {
            for (int z = 0; z < zSize; z++)
            {
                // Gets our world position for this block
                float newX = x + (chunkX * xSize);
                float newZ = z + (chunkZ * zSize);

                var totalValue = 0.0f;

                // Loop through octaves
                for (int i = 0; i < octaves - 1; i++)
                {
                    var frequency = Mathf.Pow(2f, i);
                    var l_amplitude = Mathf.Pow(roughness, i);

                    totalValue += Mathf.PerlinNoise(i + newX * frequency / smoothness, i + newZ * frequency / smoothness) * l_amplitude;
                }
                var val = (((totalValue / 2.1f) + 1.2f) * amplitude) + heightOffset;

                noise[x, z] = val > 0 ? val : 1;
            }
        }
        return noise;
    }

    public static float GetPixel (ChunkIndex chunkIndex, int x, int y, int z, NoiseSettings noiseSettings)
    {
        Vector3 chunkPos = chunkIndex.WorldPosition;

        float newX = x + noiseSettings.Seed + chunkPos.x;
        float newY = y + noiseSettings.Seed + chunkPos.y;
        float newZ = z + noiseSettings.Seed + chunkPos.z;

        float totalValue = 0f;

        for (int i = 0; i < noiseSettings.Octaves - 1; i++)
        {
            var frequency = Mathf.Pow(noiseSettings.Lacunarity, i);
            var amplitude = Mathf.Pow(noiseSettings.Roughness, i);

            totalValue += simplexNoise.Evaluate(i + newX * frequency / noiseSettings.Smoothness, i + newY * frequency / noiseSettings.Smoothness, i + newZ * frequency / noiseSettings.Smoothness) * amplitude;
        }
        var val = (((totalValue / 2.1f) + 1.2f) * noiseSettings.Amplitude) + noiseSettings.GroundHeight;

        return val > 0 ? val : 1; 
    }

    //public static float GetPixelRidged (ChunkIndex chunkIndex, int x, int y, int z, NoiseSettings noiseSettings)
    //{
    //    Vector3 chunkPos = chunkIndex.WorldPosition;

    //    float newX = (x + noiseSettings.Seed + chunkPos.x) * noiseSettings.Frequency;
    //    float newY = (y + noiseSettings.Seed + chunkPos.y) * noiseSettings.Frequency;
    //    float newZ = (z + noiseSettings.Seed + chunkPos.z) * noiseSettings.Frequency;

    //    float totalValue = 0f;
    //    float weight = 1;

    //    for (int i = 0; i < noiseSettings.Octaves; i++)
    //    {
    //        var signal = (float)LibNoise.Utils.GradientCoherentNoise3D(i + newX / noiseSettings.Smoothness, i + newY / noiseSettings.Smoothness, i + newZ / noiseSettings.Smoothness, noiseSettings.Seed, noiseSettings.Quality);
    //        signal = Mathf.Abs(signal);
    //        signal = noiseSettings.Offset - signal;
    //        signal *= signal;
    //        signal *= weight;
    //        weight = signal * noiseSettings.Gain;
    //        weight = Mathf.Clamp01(weight);

    //        totalValue += signal * noiseSettings.weights[i];

    //        newX *= noiseSettings.Lacunarity;
    //        newY *= noiseSettings.Lacunarity;
    //        newZ *= noiseSettings.Lacunarity;
    //    }
    //    var val = (((totalValue * 1.25f) - 1f) * noiseSettings.Amplitude) + noiseSettings.GroundHeight;

    //    return val > 0 ? val : 1;
    //}

    public static float GetPixelRidged(ChunkIndex chunkIndex, int x, int y, int z, RidgedNoiseSettings noiseSettings)
    {
        if (y > noiseSettings.HighCutoff)
            return 0;
        if (y < noiseSettings.LowCutoff)
            return 0;

        Vector3 chunkPos = chunkIndex.WorldPosition;

        float newX = (x + noiseSettings.Seed + chunkPos.x) * noiseSettings.Frequency;
        float newY = (y + noiseSettings.Seed + chunkPos.y) * noiseSettings.Frequency;
        float newZ = (z + noiseSettings.Seed + chunkPos.z) * noiseSettings.Frequency;

        float totalValue = 0f;
        float weight = 1;

        for (int i = 0; i < noiseSettings.Octaves; i++)
        {
            var signal = (float)LibNoise.Utils.GradientCoherentNoise3D(i + newX / noiseSettings.Smoothness, i + newY / noiseSettings.Smoothness, i + newZ / noiseSettings.Smoothness, noiseSettings.Seed, noiseSettings.Quality);
            signal = Mathf.Abs(signal);
            signal = noiseSettings.Offset - signal;
            signal *= signal;
            signal *= weight;
            weight = signal * noiseSettings.Gain;
            weight = Mathf.Clamp01(weight);

            totalValue += signal * noiseSettings.Weights[i];

            newX *= noiseSettings.Lacunarity;
            newY *= noiseSettings.Lacunarity;
            newZ *= noiseSettings.Lacunarity;
        }
        var val = (((totalValue * 1.25f) - 1f) * noiseSettings.Amplitude) + noiseSettings.HeightOffset;

        return val > 0 ? val : 1;
    }

    public static float[,] Generate2DSimplex(int chunkX, int chunkZ, int xSize, int zSize, int octaves, int amplitude, int smoothness, int heightOffset, float roughness)
    {
        // Create array
        float[,] noise = new float[xSize, zSize];

        var chunkWorldX = chunkX * xSize;
        var chunkworldZ = chunkZ * zSize;

        // Loop through X and Z positions
        // The value in the array is our height
        for (int x = 0; x < xSize; x++)
        {
            for (int z = 0; z < zSize; z++)
            {
                //// Gets our world position for this block
                float newX = x + chunkWorldX;
                float newZ = z + chunkworldZ;

                var totalValue = 0.0f;

                // Loop through octaves
                for (int i = 0; i < octaves - 1; i++)
                {
                    var frequency = Mathf.Pow(2f, i);
                    var l_amplitude = Mathf.Pow(roughness, i);

                    totalValue += simplexNoise.Evaluate(i + newX * frequency / smoothness, 1, i + newZ * frequency / smoothness) * l_amplitude; 
                }

                var val = (((totalValue / 2.1f) + 1.2f) * amplitude) + heightOffset;

                noise[x, z] = val > 0 ? val : 1;

                //noise[x, z] = totalValue + heightOffset;

                //noise[x, z] = heightOffset + simplexNoise.Evaluate(newX / smoothness, 1, newZ / smoothness) * amplitude;
            }
        }
        return noise;
    }

    public static float GetHeight (int x, int z, int chunkX, int chunkZ, int xSize, int zSize, NoiseSettings noiseSettings)
    {
        int newX = x + (chunkX * xSize);
        int newZ = z + (chunkZ * zSize);

        var totalValue = 0.0f;

        // Loop through octaves
        for (int i = 0; i < noiseSettings.Octaves - 1; i++)
        {
            var frequency = Mathf.Pow(2f, i);
            var l_amplitude = Mathf.Pow(noiseSettings.Roughness, i);

            totalValue += Mathf.PerlinNoise(i + newX * frequency / noiseSettings.Smoothness, i + newZ * frequency / noiseSettings.Smoothness) * l_amplitude;
        }
        var val = (((totalValue / 2.1f) + 1.2f) * noiseSettings.Amplitude) + noiseSettings.Amplitude;

        return val;
    }

    public static float[,,] Generate3D(int chunkX, int chunkZ, int xSize, int ySize, int zSize, int octaves, int amplitude, int smoothness, int heightOffset, float roughness)
    {
        float[,,] noise = new float[xSize, ySize, zSize];

        float max = float.MinValue;
        float min = float.MaxValue;

        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                for (int z = 0; z < zSize; z++)
                {
                    // Gets our world position for this block
                    var newX = x + (chunkX * xSize);
                    var newZ = z + (chunkZ * zSize);

                    var totalValue = 0.0f;

                    // Loop through octaves
                    for (int i = 0; i < octaves - 1; i++)
                    {
                        var frequency = Mathf.Pow(2f, i);
                        var l_amplitude = Mathf.Pow(roughness, i);

                        //totalValue += SimplexNoise.Noise.CalcPixel3D(Mathf.FloorToInt(i + newX * frequency / smoothness), Mathf.FloorToInt(i + y * frequency / smoothness), Mathf.FloorToInt(i + newZ * frequency / smoothness), 1) * l_amplitude;
                        //totalValue += Mathf.PerlinNoise(i + newX * frequency / smoothness, i + newZ * frequency / smoothness) * l_amplitude;
                    }
                    var val = (((totalValue / 2.1f) + 1.2f) * amplitude) + heightOffset;

                    if (val > max)
                    {
                        max = val;
                    }
                    if (val < min)
                    {
                        min = val;
                    }

                    noise[x, y, z] = val;
                }
            }
        }

        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                for (int z = 0; z < zSize; z++)
                {
                    noise[x, y, z] = Mathf.InverseLerp(min, max, noise[x, y, z]);
                }
            }
        }

        return noise;
    }
}
