using System;
using UnityEngine;

[Serializable]
public class ChunkIndex
{
    [ReadOnly]
    public readonly int ChunkX;
    [ReadOnly]
    public readonly int ChunkZ;

    public ChunkIndex(float chunkX, float chunkZ)
    {
        ChunkX = Mathf.RoundToInt(chunkX);
        ChunkZ = Mathf.RoundToInt(chunkZ);
    }

    public ChunkIndex(Vector3 chunkPositionIndex)
    {
        ChunkX = Mathf.RoundToInt(chunkPositionIndex.x);
        ChunkZ = Mathf.RoundToInt(chunkPositionIndex.z);
    }

    public ChunkIndex(int chunkX, int chunkZ)
    {
        ChunkX = chunkX;
        ChunkZ = chunkZ;
    }

    public Vector3 WorldPosition => ChunkVectors.CreateWorldPosition(this);

    public static bool operator == (ChunkIndex one, ChunkIndex two)
    {
        if (object.ReferenceEquals(one, null))
        {
            return object.ReferenceEquals(two, null);
        }

        return one.Equals(two);
    }

    public static bool operator != (ChunkIndex one, ChunkIndex two)
    {
        if (object.ReferenceEquals(one, null))
        {
            return object.ReferenceEquals(two, null);
        }

        return !one.Equals(two);
    }

    public override bool Equals(object obj)
    {
        if (!(obj is ChunkIndex))
        {
            return false;
        }

        var index = (ChunkIndex)obj;
        return ChunkX == index.ChunkX &&
               ChunkZ == index.ChunkZ;
    }

    public override int GetHashCode()
    {
        var hashCode = 1117171110;
        hashCode = hashCode * -1521134295 + ChunkX.GetHashCode();
        hashCode = hashCode * -1521134295 + ChunkZ.GetHashCode();
        return hashCode;
    }

    public override string ToString()
    {
        return string.Format("[ChunkIndex: ChunkX={0}, ChunkZ={1}]", ChunkX, ChunkZ);
    }
}
