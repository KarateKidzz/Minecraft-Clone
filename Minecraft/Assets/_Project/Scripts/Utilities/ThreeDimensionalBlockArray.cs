using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreeDimensionalBlockArray : ThreeDimensionalArray<Block>
{
    public ThreeDimensionalBlockArray(int x, int y, int z) : base(x,y,z) { }

    public Vector3 Size => new Vector3(GetLength(0), GetLength(1), GetLength(2));

    /// <summary>
    /// Determines whether the block at position x,y,z plus the block face direction exists
    /// </summary>
    /// <returns><c>true</c>, if exists was elemented, <c>false</c> otherwise.</returns>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <param name="z">The z coordinate.</param>
    /// <param name="blockFace">Block face.</param>
    public bool BlockExists(int x, int y, int z, BlockFace blockFace)
    {
        switch (blockFace)
        {
            case BlockFace.Top:
                y += 1;
                break;
            case BlockFace.Bottom:
                y += -1;
                break;
            case BlockFace.Left:
                x += -1;
                break;
            case BlockFace.Right:
                x += 1;
                break;
            case BlockFace.Forward:
                z += 1;
                break;
            case BlockFace.Back:
                z += -1;
                break;
        }
        return BlockExists(x, y, z);
    }

    public bool BlockExists (int x, int y, int z)
    {
        return ElementExists(x, y, z) && Container[x, y, z].BlockID != BlockType.Air && Container[x, y, z].BlockID != BlockType.OakLeaf;
    }

    public bool RowIsEmpty (int y)
    {



        return false;
    }
}
