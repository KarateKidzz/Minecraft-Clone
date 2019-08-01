using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleChunkGenerator : MonoBehaviour
{
    public int numberOfChunks = 1;
    Chunk[,] chunk;
    public ChunkGenerator chunkGenerator;
    public NoiseSettings noiseSettings;
    public bool UsePerlin = true;

    // Start is called before the first frame update
    void Start()
    {
        chunk = new Chunk[numberOfChunks, numberOfChunks];
        for (int x = 0; x < numberOfChunks; ++x)
        {
            for (int z = 0; z < numberOfChunks; ++z)
            {
                chunk[x,z] = chunkGenerator.GenerateChunkNonThreaded(new ChunkIndex(x, z), noiseSettings, UsePerlin);
                chunk[x, z].Visible = true;
            }
        }
    }

    public void Generate ()
    {
        for (int x = 0; x < numberOfChunks; ++x)
        {
            for (int z = 0; z < numberOfChunks; ++z)
            {

                chunkGenerator.GenerateChunkNonThreaded(chunk[x,z], chunk[x,z].ChunkIndex, noiseSettings, UsePerlin);

                chunk[x,z].Visible = true;
            }
        }
    }
}
