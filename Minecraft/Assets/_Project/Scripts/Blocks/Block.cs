using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BlockType : byte
{
    Air,
    bGrass, // b = Billboard
    bFlower,
    Dirt,
    Grass,
    Stone,
    OakLog,
    OakLeaf
}

public enum TreeType : byte
{
    Oak,
    Palm
}

public enum BlockFaceDirection
{
    Top,
    Side,
    Bottom
}

/// <summary>
/// Defines a 1x1x1 cube in the world with coordinates, rendering properties and interactable properties
/// </summary>
public class Block
{
    public BlockType BlockID;
    public Vector3 BottomBackLeftCorner;
    public bool IsSolid => BlockID != BlockType.Air && BlockID != BlockType.bGrass && BlockID != BlockType.bFlower;
    public bool IsTransparent => BlockID == BlockType.Air || BlockID == BlockType.OakLeaf || BlockID == BlockType.bFlower || BlockID == BlockType.bGrass;


    public Block(int x, int y, int z)
    {
        BottomBackLeftCorner = new Vector3(x,y,z);
    }

    public Block(int x, int y, int z, BlockType blockType) : this(x, y, z)
    {
        BlockID = blockType;
    }

    public Vector3 BlockWorldPosition (Chunk chunk)
    {
        return chunk.ChunkPosition + BottomBackLeftCorner + (Vector3.one / 2);
    }

    public Vector3 CenterPosition { get => BottomBackLeftCorner + new Vector3(0.5f, 0.5f, 0.5f); }
}
