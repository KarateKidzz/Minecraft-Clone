using System.Collections;
using UnityEngine;

/// <summary>
/// Defines a three dimensional array with utility functions to navigate the array
/// </summary>
public class ThreeDimensionalArray<T> where T : class
{
    protected readonly T[,,] Container;

    public ThreeDimensionalArray()
    {

    }

    public ThreeDimensionalArray(int x, int y, int z)
    {
        Container = new T[x, y, z];
    }

    public T this[int x, int y, int z]
    {
        get { return Container[x, y, z]; }
        set { Container[x, y, z] = value; }
    }

    /// <summary>
    /// Returns true if the value is not null and outputs its value to <paramref name="output"/>
    /// </summary>
    /// <returns><c>true</c>, if get value was tryed, <c>false</c> otherwise.</returns>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <param name="z">The z coordinate.</param>
    /// <param name="output">Output.</param>
    public bool TryGetValue(int x, int y, int z, out T output)
    {
        output = null;

        if (x > Container.GetLength(0) || y > Container.GetLength(1) || z > Container.GetLength(2))
        {
            return false;
        }

        output = Container[x, y, z];

        if (output != null) return true;

        return false;
    }

    /// <summary>
    /// Returns true if the element at <paramref name="x"/>, <paramref name="y"/>, <paramref name="z"/> exists
    /// </summary>
    /// <returns><c>true</c>, if exists was elemented, <c>false</c> otherwise.</returns>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <param name="z">The z coordinate.</param>
    public bool ElementExists (int x, int y, int z)
    {
        if (x < 0 || y < 0 || z < 0 || x >= Container.GetLength(0) || y >= Container.GetLength(1) || z >= Container.GetLength(2))
        {
            return false;
        }
        return Container[x, y, z] != null;
    }

    public bool ElementExists(Vector3 index)
    {
        return ElementExists((int)index.x, (int)index.y, (int)index.z);
    }

    public T Get(int x, int y, int z)
    {
        return Container[x, y, z];
    }

    public T Get (Vector3 index)
    {
        return Get((int)index.x, (int)index.y, (int)index.z);
    }

    /// <summary>
    /// Determines whether the block at position x,y,z plus the block face direction exists
    /// </summary>
    /// <returns><c>true</c>, if exists was elemented, <c>false</c> otherwise.</returns>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <param name="z">The z coordinate.</param>
    /// <param name="blockFace">Block face.</param>
    public bool ElementExists (int x, int y, int z, BlockFace blockFace)
    {
        int xOffset, yOffset, zOffset;
        xOffset = yOffset = zOffset = 0;

        switch (blockFace)
        {
            case BlockFace.Top:
                yOffset = 1;
                break;
            case BlockFace.Bottom:
                yOffset = -1;
                break;
            case BlockFace.Left:
                xOffset = -1;
                break;
            case BlockFace.Right:
                xOffset = 1;
                break;
            case BlockFace.Forward:
                zOffset = 1;
                break;
            case BlockFace.Back:
                zOffset = -1;
                break;
        }
        return ElementExists(x + xOffset, y + yOffset, z + zOffset);
    }

    public int GetLength (int dimension)
    {
        if (dimension < 0 || dimension > 2) return 0;

        return Container.GetLength(dimension);
    }

    public int Length 
    {
        get => Container.Length;
    }

    public IEnumerator GetEnumerator ()
    {
        if (Container == null) yield break;

        for (int x = 0; x < Container.GetLength(0); x++)
        {
            for (int y = 0; y < Container.GetLength(1); y++)
            {
                for (int z = 0; z < Container.GetLength(2); z++)
                {
                    if (Container[x, y, z] != null)
                        yield return Container[x, y, z];
                }
            }
        }
    }
}
