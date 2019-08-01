using UnityEngine;
using UnityEngine.Profiling;
using System.Threading;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    /// <summary>
    /// Constant Chunk size of all chunks
    /// </summary>
    /// <value>The size of the chunk.</value>
    public static Vector3 ChunkSize { get; } = new Vector3(16, 256, 16);

    /// <summary>
    /// All chunks in the world
    /// </summary>
    public static Dictionary<ChunkIndex, Chunk> WorldChunks = new Dictionary<ChunkIndex, Chunk>();

    Mesh chunkMesh;
    MeshFilter chunkMeshFilter;
    MeshRenderer chunkMeshRenderer;
    MeshCollider chunkMeshCollider;
    Bounds ChunkBounds;


    /// <summary>
    /// Blocks contained in the chunk
    /// </summary>
    public ThreeDimensionalBlockArray ChunkBlocks;

    void Awake()
    {
        chunkMeshFilter = gameObject.GetComponent<MeshFilter>();
        chunkMeshRenderer = GetComponent<MeshRenderer>();
        chunkMeshCollider = gameObject.GetComponent<MeshCollider>();

        chunkMeshFilter.mesh = chunkMesh = new Mesh();
        chunkMeshCollider.sharedMesh = chunkMesh;

        chunkMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        ChunkBounds = new Bounds(ChunkPosition, ChunkSize);

        WorldChunks.Add(ChunkVectors.CreateChunkIndex(ChunkPosition), this);

        ChunkIndex = ChunkVectors.CreateChunkIndex(ChunkPosition);
    }

    void OnDestroy()
    {
        WorldChunks.Remove(ChunkVectors.CreateChunkIndex(ChunkPosition));
    }

    #region Generation

    /// <summary>
    /// Sets the blocks of this chunk
    /// </summary>
    /// <param name="blocks">Blocks.</param>
    public void ApplyBlocks (ThreeDimensionalBlockArray blocks)
    {
        ChunkBlocks = blocks;
    }

    /// <summary>
    /// Applies the verts, <paramref name="uvs"/> and triangles to the mesh. Also updates the mesh collision
    /// </summary>
    /// <param name="verts">Verts.</param>
    /// <param name="uvs">Uvs.</param>
    /// <param name="triangles">Triangles.</param>
    public void ApplyMesh (Vector3[] verts, Vector2[] uvs, int[] triangles)
    {
        chunkMesh.Clear();

        chunkMesh.vertices = verts;
        chunkMesh.triangles = triangles;
        chunkMesh.uv = uvs;

        chunkMesh.RecalculateNormals();

        if (chunkMeshCollider != null)
            chunkMeshCollider.sharedMesh = chunkMesh;
        
    }

    #endregion

    #region Public Methods

    [SerializeField]
    private bool IsVisible;

    /// <summary>
    /// Whether this chunk is rendering
    /// </summary>
    /// <value><c>true</c> if is visible; otherwise, <c>false</c>.</value>
    public bool Visible
    {
        get => chunkMeshRenderer.enabled;
        set { chunkMeshRenderer.enabled = value; IsVisible = value; }
    }

    /// <summary>
    /// Whether the chunk contains the specified position.
    /// </summary>
    /// <returns>The contains.</returns>
    /// <param name="position">Position.</param>
    public bool Contains (Vector3 position)
    {
        return ChunkBounds.Contains(position);
    }

    #endregion

    #region Public Positions


    /// <summary>
    /// World position of the bottom back left corner
    /// </summary>
    /// <value>The chunk position.</value>
    public Vector3 ChunkPosition
    {
        get => transform.position.Round();
        set => transform.position = value.Round();
    }

    /// <summary>
    /// Represents the exact center of the chunk in world position
    /// </summary>
    /// <value>The chunk center.</value>
    public Vector3 ChunkCenter
    {
        get => ChunkPosition + (ChunkSize / 2);
    }

    public ChunkIndex ChunkIndex { get; private set; }

    #endregion
}
