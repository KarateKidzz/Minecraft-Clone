using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

/// <summary>
/// Collection class for storing a map on chunks and adding and removing chunks when the center index changes
/// </summary>
public class ChunkMap : MonoBehaviour
{
    struct TwoDIndex
    {
        public readonly int x;
        public readonly int z;

        public TwoDIndex (int x, int z)
        {
            this.x = x;
            this.z = z;
        }
    }
    public Chunk[,] Chunks;

    TwoDIndex Center;
    ChunkIndex CenterIndex;

    public int size;
    public ChunkGenerator ChunkGenerator;
    public Transform Player;


    void Awake()
    {
        Debug.Assert(size > 1);
        Debug.Assert((size - 1) % 2 == 0);  // make sure it's odd

        CenterIndex = ChunkAndBlockSearch.FindChunkIndex(Player.position);

        Chunks = new Chunk[size, size];

        Center = new TwoDIndex((size - 1) / 2, (size - 1) / 2);

        FillArray();
    }

    void Update()
    {
        ChunkIndex playerChunk = ChunkAndBlockSearch.FindChunkIndex(Player.position);
        if (CenterIndex != playerChunk)
        {
            int xDir = playerChunk.ChunkX - CenterIndex.ChunkX;
            int zDir = playerChunk.ChunkZ - CenterIndex.ChunkZ;

            CenterIndex = playerChunk;

            ShiftArray(xDir, zDir);
        }
    }

    void FillArray ()
    {
        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                ChunkIndex pos = new ChunkIndex(CenterIndex.ChunkX + (x - Center.x), CenterIndex.ChunkZ + (z - Center.z));

                if (!ChunkGenerator.Loading.Contains(pos))
                {
                    ChunkGenerator.RequestChunkGeneration(pos, null, OnChunkCreation, false);
                }
            }
        }
    }

    void ShiftArray (int xDir, int zDir)
    {
        int xLeft = 0 + xDir;   // anything below this value needs to be destroyed
        int xRight = size + xDir;   // anything above this value needs to be destroyed
        int zLeft = 0 + zDir;
        int zRight = size + zDir;

        if (xDir >= 0)
        {
            if (zDir >= 0)
            {
                for (int x = 0; x < size; x++)
                {
                    for (int z = 0; z < size; z++)
                    {
                        ShiftIndex(x, z, xLeft, zLeft, xRight, zRight, xDir, zDir);
                    }
                }
            }
            else
            {
                for (int x = 0; x < size; x++)
                {
                    for (int z = size - 1; z >= 0; z--)
                    {
                        ShiftIndex(x, z, xLeft, zLeft, xRight, zRight, xDir, zDir);
                    }
                }
            }
        }
        else
        {
            if (zDir >= 0)
            {
                for (int x = size - 1; x >= 0; x--)
                {
                    for (int z = 0; z < size; z++)
                    {
                        ShiftIndex(x, z, xLeft, zLeft, xRight, zRight, xDir, zDir);
                    }
                }
            }
            else
            {
                for (int x = size - 1; x >= 0; x--)
                {
                    for (int z = size - 1; z >= 0; z--)
                    {
                        ShiftIndex(x, z, xLeft, zLeft, xRight, zRight, xDir, zDir);
                    }
                }
            }
        }
    }

    void ShiftIndex (int x, int z, int xLeft, int zLeft, int xRight, int zRight, int xDir, int zDir)
    {
        if (x < xLeft || z < zLeft || x >= xRight || z >= zRight)
        {
            if (Chunks[x, z] != null)
            {
                Destroy(Chunks[x, z].gameObject);
                //Chunks[x, z] = null;
                //Debug.Log("Xleft: " + xLeft);
                //Debug.Log("XRight: " + xRight);
                //Debug.Log("X: " + x);
                Chunks[x, z] = null;
            }
            //continue;
        }

        int newX = x + xDir;
        int newZ = z + zDir;

        if (newX >= size || newZ >= size || newX < 0 || newZ < 0)
        {
            //Chunk chunk = Chunks[x, z];
            //if (chunk != null)
            //{
            //    Destroy(Chunks[x, z].gameObject);
            //}
            //Chunks[x, z] = null;


            ChunkIndex pos = new ChunkIndex(CenterIndex.ChunkX + (x - Center.x), CenterIndex.ChunkZ + (z - Center.z));

            if (!ChunkGenerator.Loading.Contains(pos))
            {
                ChunkGenerator.RequestChunkGeneration(pos, null, OnChunkCreation, false);
            }


            return;
        }
        //Chunks[x, z] = Chunks[x + -xDir, z + -zDir];
        Chunks[x, z] = Chunks[newX, newZ];
    }

    void OnChunkCreation (ChunkIndex index, Chunk chunk)
    {
        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                ChunkIndex pos = new ChunkIndex(CenterIndex.ChunkX + (x - Center.x), CenterIndex.ChunkZ + (z - Center.z));

                if (pos == index)
                {
                    Chunks[x, z] = chunk;
                    chunk.Visible = true;
                    return;
                }
            }
        }
        // destroy if the new chunk is already not part of the arrays
        Destroy(chunk.gameObject);
    }

    void OnDrawGizmosSelected ()
    {
        Gizmos.color = Color.red;

        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                if (Chunks != null)
                {
                    Chunk chunk = Chunks[x, z];
                    if (chunk != null)
                    {
                        Gizmos.DrawWireCube(chunk.ChunkCenter, Chunk.ChunkSize);
                    }
                }
            }
        }
    }
}
