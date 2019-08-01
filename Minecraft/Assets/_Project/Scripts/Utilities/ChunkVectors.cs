using UnityEngine;

/// <summary>
/// Utility class for creating chunk positions, indexes and chunk indexes
/// </summary>
public static class ChunkVectors
{
    public static Vector3 ChunkToWorld(this Vector3 chunkPosition)
    {
        return new Vector3(chunkPosition.x * Chunk.ChunkSize.x, 0, chunkPosition.z * Chunk.ChunkSize.z);
    }

    public static Vector3 WorldToChunk(this Vector3 worldPosition)
    {
        return new Vector3(worldPosition.x / Chunk.ChunkSize.x, 0, worldPosition.z / Chunk.ChunkSize.z);
    }

    public static Vector3 CreateWorldPosition(ChunkIndex chunkIndex)
    {
        return new Vector3(chunkIndex.ChunkX * Chunk.ChunkSize.x, 0, chunkIndex.ChunkZ * Chunk.ChunkSize.z);
    }

    public static ChunkIndex CreateChunkIndex(Vector3 worldPosition)
    {
        Vector3 local = worldPosition.WorldToChunk();
        return new ChunkIndex(local.x, local.z);
    }
}