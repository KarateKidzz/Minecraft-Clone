using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds information about the block, its chunk and the block and chunk connected to it
/// </summary>
public struct BlockHitInfo
{
    public readonly Block block;
    public readonly Chunk chunk;
    public readonly Block closeBlock;
    public readonly Chunk closeBlockChunk;
    public bool Success => block != null && chunk != null && closeBlock != null && closeBlockChunk != null;

    public BlockHitInfo(Block block, Chunk chunk, Block closeBlock, Chunk closeBlockChunk)
    {
        this.block = block;
        this.chunk = chunk;
        this.closeBlock = closeBlock;
        this.closeBlockChunk = closeBlockChunk;
    }
}

/// <summary>
/// Allows quick and easy searching and finding of blocks and chunks
/// </summary>
public static class ChunkAndBlockSearch
{
    /// <summary>
    /// Finds the chunk index at the given world position
    /// </summary>
    /// <returns>The chunk index.</returns>
    /// <param name="worldPosition">World position.</param>
    public static ChunkIndex FindChunkIndex(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / Chunk.ChunkSize.x);
        int z = Mathf.FloorToInt(worldPosition.z / Chunk.ChunkSize.z);

        return new ChunkIndex(x, z);
    }

    /// <summary>
    /// Finds the chunk at the given position
    /// </summary>
    /// <returns>The chunk.</returns>
    /// <param name="worldPosition">World position.</param>
    public static Chunk FindChunk(Vector3 worldPosition)
    {
        ChunkIndex index = FindChunkIndex(worldPosition);
        if (Chunk.WorldChunks.TryGetValue(index, out Chunk chunk))
        {
            return chunk;
        }
        return null;
    }

    /// <summary>
    /// Finds the block at the given position
    /// </summary>
    /// <returns>The block.</returns>
    /// <param name="worldPosition">World position.</param>
    public static Block FindBlock(Vector3 worldPosition, Chunk chunk = null)
    {
        if (chunk == null)
        {
            chunk = FindChunk(worldPosition);
        }
        int x, y, z = 0;

        x = Mathf.FloorToInt(worldPosition.x);
        y = Mathf.FloorToInt(worldPosition.y);
        z = Mathf.FloorToInt(worldPosition.z);

        x = Mathu.mod(x, Mathf.FloorToInt(Chunk.ChunkSize.x));
        z = Mathu.mod(z, Mathf.FloorToInt(Chunk.ChunkSize.z));

        Block block = chunk.ChunkBlocks[x, y, z];

        if (block == null)
        {
            Debug.LogWarning("Failed to find block");
        }

        return block;
    }

    /// <summary>
    /// Fires a ray from the camera to find a block. If <paramref name="acceptAir"/> is true, the raycast is allowed to miss and get an air block
    /// </summary>
    /// <returns>The raycast.</returns>
    /// <param name="mainCamera">Main camera.</param>
    public static BlockHitInfo Raycast(Camera mainCamera, float distance, LayerMask layerMask, bool acceptAir = false)
    {
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Block block = null;
        Chunk chunk = null;
        Block closeBlock = null;
        Chunk closeChunk = null;

        if (Physics.Raycast(ray, out RaycastHit hit, distance, layerMask))
        {
            chunk = hit.collider.GetComponent<Chunk>();
            block = FindBlock(hit.point + (ray.direction * 0.1f), chunk);

            Vector3 closePosition = hit.point + (ray.direction * -0.1f);
            closeChunk = FindChunk(closePosition);

            if (closeChunk != null)
            {
                closeBlock = FindBlock(closePosition, closeChunk);
            }
        }
        else if (acceptAir)
        {
            Vector3 rayEndPoint = ray.origin + (ray.direction * distance);
            chunk = FindChunk(rayEndPoint);

            if (chunk != null)
            {
                block = FindBlock(rayEndPoint, chunk);
            }
        }

        return new BlockHitInfo(block, chunk, closeBlock, closeChunk);
    }
}
