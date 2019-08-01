using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using Unity.Jobs;

public class InfiniteTerrain : MonoBehaviour
{
    public ChunkGenerator ChunkGenerator;
    public NoiseSettings BiomeNoiseSettings;
    public NoiseSettings FlatBiomeSettings;
    public NoiseSettings LightForestSettings;
    public NoiseSettings HeavyForestSettings;
    public const float maxViewDistance = 128;
    public Transform Viewer;
    public bool perlin = true;

    public static Vector3 ViewerPosition;
    int chunksVisibleInView;

    Dictionary<ChunkIndex, Chunk> chunkDictionary = new Dictionary<ChunkIndex, Chunk>();
    List<Chunk> lastVisible = new List<Chunk>();

    void Start()
    {
        chunksVisibleInView = Mathf.RoundToInt(maxViewDistance / Chunk.ChunkSize.x);
        ViewerPosition = Viewer.transform.position;
        //UpdateVisibleChunks();
    }

    void Update()
    {
        ViewerPosition = Viewer.transform.position;

        Profiler.BeginSample("Chunk Update");
        UpdateVisibleChunks();
        Profiler.EndSample();
    }

    void UpdateVisibleChunks()
    {
        // Get 'index' of chunk we are on
        int xCoord = Mathf.RoundToInt(ViewerPosition.x / Chunk.ChunkSize.x);
        int yCoord = Mathf.RoundToInt(ViewerPosition.y / Chunk.ChunkSize.y);
        int zCoord = Mathf.RoundToInt(ViewerPosition.z / Chunk.ChunkSize.z);

        // The rounded off chunk posistion
        // This is not a world position but a chunk index
        Vector3 centerChunkPosition = new Vector3(xCoord, 0, zCoord);

        Profiler.BeginSample("Hide Last Visible");
        // Set invisible
        for (int i = 0; i < lastVisible.Count; i++)
        {
            lastVisible[i].Visible = false;
        }
        lastVisible.Clear();
        Profiler.EndSample();

        // Loop
        Profiler.BeginSample("Loop");
        for (int x = -chunksVisibleInView; x < chunksVisibleInView; x++)
        {
            for (int z = -chunksVisibleInView; z < chunksVisibleInView; z++)
            {
                //Vector3 chunkPosition = new Vector3(xCoord + x, 0, zCoord + z);
                ChunkIndex chunkPosition = new ChunkIndex(xCoord + x, zCoord + z);
                //Vector3 chunkWorldPosition = chunkSize * chunkPosition;

                bool gotValue = false;

                Profiler.BeginSample("Chunk Lookup");
                if (chunkDictionary.TryGetValue(chunkPosition, out Chunk terrainChunk))
                {
                    Profiler.BeginSample("Set Visible");
                    terrainChunk.Visible = true;
                    Profiler.EndSample();

                    Profiler.BeginSample("Add To List");
                    lastVisible.Add(terrainChunk);
                    Profiler.EndSample();

                    gotValue = true;
                }
                Profiler.EndSample();
                if (!gotValue)
                {
                    Profiler.BeginSample("Loading Lookup");
                    if (!ChunkGenerator.Loading.Contains(chunkPosition))
                    {
                        Profiler.BeginSample("Request Generation");
                        ChunkGenerator.RequestChunkGeneration(chunkPosition, GetBiome(1, 3, chunkPosition.ChunkX, chunkPosition.ChunkZ), AddToDictionary, perlin);
                        Profiler.EndSample();
                    }
                    Profiler.EndSample();
                }
            }
        }
        Profiler.EndSample();
    }

    void AddToDictionary (ChunkIndex chunkIndex, Chunk chunk)
    {
        chunkDictionary.Add(chunkIndex, chunk);
    }

    NoiseSettings GetBiome (int x, int z, int chunkX, int chunkZ)
    {
        return FlatBiomeSettings;
        //float biomeValue = NoiseGenerator.GetHeight(x, z, chunkX, chunkZ, (int)Chunk.ChunkSize.x, (int)Chunk.ChunkSize.z, BiomeNoiseSettings);

        //if (biomeValue > 340)
        //{

        //}
        //else
        //{
        //    return LightForestSettings;
        //}
    }
}
