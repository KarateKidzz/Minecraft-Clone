using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using System.Collections.Concurrent;

/// <summary>
/// Creates verticies, triangles and UVs to be passed back to a chunk and rendered
/// </summary>
public class ChunkGenerator : MonoBehaviour
{
    // quick reference to size
    Vector3 size => Chunk.ChunkSize;

    int numberLoading = 0;

    public HashSet<ChunkIndex> Loading = new HashSet<ChunkIndex>();
    ConcurrentQueue<GeneratedChunk> generatedChunks = new ConcurrentQueue<GeneratedChunk>();
    Queue<RequestedChunk> requestedChunks = new Queue<RequestedChunk>();    // space instantiation over multiple frames

    public GameObject ChunkPrefab;
    public TextureManager TextureManager;
    public NoiseSettings MainNoiseSettings;

    void Update()
    {
        if (requestedChunks.Count > 0)
        {
            for (int i = 0; i < requestedChunks.Count; i++)
            {
                Profiler.BeginSample("Deueue Requested");
                var rc = requestedChunks.Dequeue();
                Profiler.EndSample();

                // Spawns an empty chunk
                Profiler.BeginSample("Instantiate");
                Chunk chunkObject = Instantiate(ChunkPrefab, rc.chunkIndex.WorldPosition, Quaternion.identity, transform).GetComponent<Chunk>();
                Profiler.EndSample();

                Profiler.BeginSample("Set Visible");
                chunkObject.Visible = false;
                Profiler.EndSample();

                Profiler.BeginSample("Callback");
                rc.OnInstantiateCallback.Invoke(rc.chunkIndex, chunkObject);
                Profiler.EndSample();

                Profiler.BeginSample("Request Remove");
                lock (Loading)
                {
                    Loading.Remove(rc.chunkIndex);
                }
                Profiler.EndSample();

                Profiler.BeginSample("Start Generation Thread");
                // Starts a thread to handle mesh generation
                ThreadPool.QueueUserWorkItem(new WaitCallback(delegate
               {
                   GenerateChunkThreaded(chunkObject, rc.chunkIndex, rc.noiseSettings, rc.perlin);

               }));

                //ThreadStart threadStart = delegate {
                //    GenerateChunkThreaded(chunkObject, rc.chunkIndex, rc.noiseSettings, rc.perlin);
                //};
                //new Thread(threadStart).Start();
                Profiler.EndSample();

                numberLoading++;
            }
        }

        if (generatedChunks.Count > 0)
        {
            if (generatedChunks.TryDequeue(out var chunk))
            {
                if (chunk.ApplyBlocks)
                    chunk.chunk.ApplyBlocks(chunk.blocks);
                chunk.chunk.ApplyMesh(chunk.vertices, chunk.uvs, chunk.triangles);
                numberLoading--;
            }
        }
    }


    public bool IsLoading => numberLoading > 0;

    /// <summary>
    /// Spawns a new chunk, populates it with blocks and adds its mesh
    /// </summary>
    /// <param name="chunkIndex">Chunk index.</param>
    /// <param name="noiseSettings">Noise settings.</param>
    public void RequestChunkGeneration(ChunkIndex chunkIndex, NoiseSettings noiseSettings, Action<ChunkIndex, Chunk> callback, bool perlin)
    {
        lock (Loading)
        {
            Loading.Add(chunkIndex);
        }
        requestedChunks.Enqueue(new RequestedChunk(chunkIndex, MainNoiseSettings, callback, perlin));
    }

    /// <summary>
    /// Creates the <paramref name="chunk"/>'s mesh
    /// </summary>
    /// <param name="chunk">Chunk.</param>
    public void RequestChunkRefresh (Chunk chunk)
    {
        ThreadPool.QueueUserWorkItem(new WaitCallback(delegate
        {
            GenerateMeshThreaded(chunk.ChunkBlocks, chunk);

        }));

        //// Starts a thread to handle mesh generation
        //ThreadStart threadStart = delegate {
        //    GenerateMeshThreaded(chunk.ChunkBlocks, chunk);
        //};
        //new Thread(threadStart).Start();
    }

    public Chunk GenerateChunkNonThreaded (ChunkIndex chunkIndex, NoiseSettings noiseSettings, bool perlin)
    {
        Chunk chunkObject = Instantiate(ChunkPrefab, chunkIndex.WorldPosition, Quaternion.identity, transform).GetComponent<Chunk>();
        chunkObject.Visible = false;

        GenerateChunkNonThreaded(chunkObject, chunkIndex, noiseSettings, perlin);

        return chunkObject;
    }

    public void GenerateChunkNonThreaded(Chunk chunk, ChunkIndex chunkIndex, NoiseSettings noiseSettings, bool perlin)
    {
        var generated = GenerateNew(chunk, chunkIndex, noiseSettings);

        chunk.ApplyBlocks(generated.blocks);
        chunk.ApplyMesh(generated.vertices, generated.uvs, generated.triangles);
    }

    /// <summary>
    /// Makes a generated chunk and adds it to the queue to be processed on the main thread
    /// </summary>
    /// <param name="chunk">Chunk.</param>
    /// <param name="chunkIndex">Chunk index.</param>
    /// <param name="noiseSettings">Noise settings.</param>
    void GenerateChunkThreaded(Chunk chunk, ChunkIndex chunkIndex, NoiseSettings noiseSettings, bool perlin)
    {
        var generated = GenerateNew(chunk, chunkIndex, noiseSettings);
        generatedChunks.Enqueue(generated);
    }

    static float[] ChunkYGradient = new float[(int)Chunk.ChunkSize.y];

    static ChunkGenerator ()
    {
        // Create a gradient along the Chunk Y axis from 0 to 1
        for (float i = 0; i < ChunkYGradient.Length; i++)
        {
            ChunkYGradient[(int)i] = Mathf.Lerp(0, Chunk.ChunkSize.y, i / ChunkYGradient.Length);
        }
    }

    static int OneMinusOne (float threshold, float value)
    {
        if (threshold > value)
            return 1;
        return -1;
    }

    static int OneZero (float threshold, float value)
    {
        if (threshold > value)
            return 1;
        return 0;
    }

    static int Flip (float value)
    {
        if (value > 0)
            return 0;
        return 1;
    }

    GeneratedChunk GenerateNew (Chunk chunk, ChunkIndex chunkIndex, NoiseSettings noiseSettings)
    {
        System.Random random = new System.Random();

        ThreeDimensionalBlockArray Blocks = new ThreeDimensionalBlockArray((int)size.x, (int)size.y, (int)size.z);

        noiseSettings.SetWeights();

        // Loop through all blocks and get solid or air value
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                for (int z = 0; z < size.z; z++)
                {
                    if (y == 0 || y == 1 || y == 2)
                    {
                        Blocks[x, y, z] = new Block(x, y, z, BlockType.Stone);
                        continue;
                    }

                    float groundNoise = 0;
                    float caveNoise1 = 0;
                    float caveNoise2 = 0;

                    caveNoise1 = NoiseGenerator.GetPixelRidged(chunkIndex, x, y, z, noiseSettings.CaveOne);
                    groundNoise = NoiseGenerator.GetPixel(chunk.ChunkIndex, x, y, z, noiseSettings);
                    caveNoise2 = NoiseGenerator.GetPixelRidged(chunkIndex, x, y, z, noiseSettings.CaveTwo);

                    float groundSolid = 1;
                    int caveOpen1 = -1;
                    int caveOpen2 = -1;

                    groundSolid = OneMinusOne(groundNoise, ChunkYGradient[y]);

                    caveOpen1 = OneZero(caveNoise1, ChunkYGradient[y]);
                    caveOpen2 = OneZero(caveNoise2, ChunkYGradient[y]);

                    bool isSolid;

                    float finalOpen = groundSolid * Flip(caveOpen1 * caveOpen2);

                    isSolid = finalOpen >= 1;

                    //isSolid = caveOpen1 >= 1;
                    Blocks[x, y, z] = new Block(x, y, z, isSolid ? BlockType.Grass : BlockType.Air);
                }
            }
        }

        // Loop through all blocks and get grass, dirt or stone value
        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.z; z++)
            {
                float topBlockHeight = -1;   // the height we first found a block at

                for (int y = (int)size.y - 1; y >= 0; y--)
                {
                    // if we found the first solid block and are not below ground
                    if (Blocks[x,y,z].BlockID != BlockType.Air)
                    {
                        if (topBlockHeight < 0)
                            topBlockHeight = y;

                        if (y < topBlockHeight && y >= topBlockHeight - 3)
                        {
                            Blocks[x, y, z].BlockID = BlockType.Dirt;
                        }
                        else if (y < topBlockHeight)
                        {
                            Blocks[x, y, z].BlockID = BlockType.Stone;
                        }
                    }
                }
            }
        }

        //List<Vector3> trees = new List<Vector3>();

        //// Loop through 4x4 grids and place trees
        //for (int x = 0; x < size.x; x += 4)
        //{
        //    for (int z = 0; z < size.z; z += 4)
        //    {
        //        // to get delta height we index the array diagonally and min max the heights
        //        // there's no point going through each block
        //        int one, two, three, four;
        //        one = GetHeight(Blocks, x, z);
        //        two = GetHeight(Blocks, x + 1, z + 1);
        //        three = GetHeight(Blocks, x + 2, z + 2);
        //        four = GetHeight(Blocks, x + 3, z + 3);

        //        int min = int.MaxValue;
        //        int max = int.MinValue;

        //        if (one > max)
        //            one = max;
        //        if (two > max)
        //            two = max;
        //        if (three > max)
        //            three = max;
        //        if (four > max)
        //            four = max;

        //        if (one < min)
        //            one = min;
        //        if (two < min)
        //            two = min;
        //        if (three < min)
        //            three = min;
        //        if (four < min)
        //            four = min;

        //        int delta = max - min;
        //        if (delta < 5)  // only do trees if the ground is relatively flat
        //        {
        //            if (random.Next(0, 100 - noiseSettings.TreeFrequency) == 5)
        //            {
        //                trees.Add(new Vector3(x, one, z));
        //            }
        //        }
        //    }
        //}

        //foreach (Vector3 vector in trees)
        //{
        //    GenerateTree(Blocks, vector, TreeType.Oak, random);
        //}


        return GenerateMesh(Blocks, chunk);
    }

    int GetHeight (ThreeDimensionalBlockArray Blocks, int x, int z)
    {
        for (int y = (int)size.y - 1; y >= 0; y--)
        {
            if (Blocks[x, y, z].BlockID != BlockType.Air)
                return y;
        }

        return 0;
    }

    void GenerateMeshThreaded (ThreeDimensionalBlockArray Blocks, Chunk chunk)
    {
        var refreshed = GenerateMesh(Blocks, chunk);
        refreshed.ApplyBlocks = false;

        generatedChunks.Enqueue(refreshed);
    }

    Vector3 GetPositionForRidgedNoise (ChunkIndex chunkIndex, int x, int y, int z, int smoothness)
    {
        Vector3 chunkPos = chunkIndex.WorldPosition;

        float newX = x + chunkPos.x;
        float newY = y + chunkPos.y;
        float newZ = z + chunkPos.z;

        return new Vector3(newX, newY, newZ);
    }

    /// <summary>
    /// General method that returns a completed chunk
    /// </summary>
    /// <returns>The chunk.</returns>
    /// <param name="chunk">Chunk.</param>
    /// <param name="chunkIndex">Chunk index.</param>
    /// <param name="noiseSettings">Noise settings.</param>
    GeneratedChunk GenerateChunk (Chunk chunk, ChunkIndex chunkIndex, NoiseSettings noiseSettings, bool perlin)
    {
        // Create our blocks. This is not the mesh
        ThreeDimensionalBlockArray Blocks = new ThreeDimensionalBlockArray((int)size.x, (int)size.y, (int)size.z);

        // Heightmap
        float[,] noiseMap = null;

        if(perlin)
            noiseMap = NoiseGenerator.Generate2D(chunkIndex.ChunkX, chunkIndex.ChunkZ, (int)size.x, (int)size.z, noiseSettings.Octaves, noiseSettings.Amplitude, noiseSettings.Smoothness, noiseSettings.GroundHeight, noiseSettings.Roughness);
        else
            noiseMap = NoiseGenerator.Generate2DSimplex(chunkIndex.ChunkX, chunkIndex.ChunkZ, (int)size.x, (int)size.z, noiseSettings.Octaves, noiseSettings.Amplitude, noiseSettings.Smoothness, noiseSettings.GroundHeight, noiseSettings.Roughness);

        List<Vector3> trees = new List<Vector3>();

        // can't use unity due to threading
        System.Random random = new System.Random();

        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.z; z++)
            {
                int h = Mathf.RoundToInt(noiseMap[x, z]);
                for (int y = 0; y < size.y; y++)
                {
                    BlockType blockType = BlockType.Air;

                    if (y < h)
                    {
                        // Top block
                        if (y + 1 == h)
                        {
                            // TODO: Get top block
                            blockType = BlockType.Grass;

                            if (random.Next(0, 100 - noiseSettings.TreeFrequency) == 5)
                            {
                                trees.Add(new Vector3(x, y, z));
                            }
                            //if ((0, noiseSettings.TreeFrequency + 1) == 5)
                            //{
                            //    trees.Add(new Vector3(x, y, z));
                            //}
                        }
                        // Dirt
                        else if (y + 1 > h - 3)
                        {
                            blockType = BlockType.Dirt;
                        }
                        // Below
                        else
                        {
                            blockType = BlockType.Stone;
                        }
                    }

                    Blocks[x, y, z] = new Block(x, y, z, blockType);
                }
            }
        }

        //foreach (Vector3 pos in trees)
        //{
        //    GenerateTree(pos, TreeType.Oak);
        //}

        return GenerateMesh(Blocks, chunk);
    }

    GeneratedChunk GenerateMesh (ThreeDimensionalBlockArray Blocks, Chunk chunk)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

        int VerticesIndex;

        float blockSize = 1;

        ThreeDimensionalBlockArray leftBlocks, rightBlocks, backBlocks, frontBlocks;
        leftBlocks = rightBlocks = backBlocks = frontBlocks = null;
        ChunkIndex chunkIndex = chunk.ChunkIndex;
        ChunkIndex leftIndex, rightIndex, backIndex, frontIndex;

        leftIndex = new ChunkIndex(chunkIndex.ChunkX - 1, chunkIndex.ChunkZ);
        rightIndex = new ChunkIndex(chunkIndex.ChunkX + 1, chunkIndex.ChunkZ);
        backIndex = new ChunkIndex(chunkIndex.ChunkX, chunkIndex.ChunkZ - 1);
        frontIndex = new ChunkIndex(chunkIndex.ChunkX, chunkIndex.ChunkZ + 1);


        lock (Chunk.WorldChunks)
        {
            if (Chunk.WorldChunks.TryGetValue(leftIndex, out Chunk left)) leftBlocks = left.ChunkBlocks;
            if (Chunk.WorldChunks.TryGetValue(rightIndex, out Chunk right)) rightBlocks = right.ChunkBlocks;
            if (Chunk.WorldChunks.TryGetValue(backIndex, out Chunk back)) backBlocks = back.ChunkBlocks;
            if (Chunk.WorldChunks.TryGetValue(frontIndex, out Chunk front)) frontBlocks = front.ChunkBlocks;
        }

        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int z = 0; z < size.z; z++)
                {
                    if (Blocks[x,y,z].BlockID != BlockType.Air)
                    {
                        BlockType blockType = Blocks[x, y, z].BlockID;
                        BlockFaceDirection direction = BlockFaceDirection.Side;


                        if (!BlockExists(x, y, z, BlockFace.Top, Blocks, null))
                        {
                            direction = BlockFaceDirection.Top;

                            VerticesIndex = vertices.Count;

                            vertices.Add(new Vector3(x, y + blockSize, z));
                            vertices.Add(new Vector3(x, y + blockSize, z + blockSize));
                            vertices.Add(new Vector3(x + blockSize, y + blockSize, z + blockSize));
                            vertices.Add(new Vector3(x + blockSize, y + blockSize, z));

                            UpdateChunkUV(blockType, direction);
                        }

                        if (!BlockExists(x, y, z, BlockFace.Bottom, Blocks, null))
                        {
                            if (y == 0) continue;
                            direction = BlockFaceDirection.Bottom;

                            VerticesIndex = vertices.Count;

                            vertices.Add(new Vector3(x, y, z));
                            vertices.Add(new Vector3(x + blockSize, y, z));
                            vertices.Add(new Vector3(x + blockSize, y, z + blockSize));
                            vertices.Add(new Vector3(x, y, z + blockSize));

                            UpdateChunkUV(blockType, direction);
                        }

                        if (!BlockExists(x, y, z, BlockFace.Left, Blocks, leftBlocks))
                        {
                            direction = BlockFaceDirection.Side;

                            VerticesIndex = vertices.Count;

                            vertices.Add(new Vector3(x, y, z + blockSize));
                            vertices.Add(new Vector3(x, y + blockSize, z + blockSize));
                            vertices.Add(new Vector3(x, y + blockSize, z));
                            vertices.Add(new Vector3(x, y, z));

                            UpdateChunkUV(blockType, direction);
                        }

                        if (!BlockExists(x, y, z, BlockFace.Right, Blocks, rightBlocks))
                        {
                            direction = BlockFaceDirection.Side;

                            VerticesIndex = vertices.Count;

                            vertices.Add(new Vector3(x + blockSize, y, z));
                            vertices.Add(new Vector3(x + blockSize, y + blockSize, z));
                            vertices.Add(new Vector3(x + blockSize, y + blockSize, z + blockSize));
                            vertices.Add(new Vector3(x + blockSize, y, z + blockSize));

                            UpdateChunkUV(blockType, direction);
                        }

                        if (!BlockExists(x, y, z, BlockFace.Back, Blocks, backBlocks))
                        {
                            direction = BlockFaceDirection.Side;

                            VerticesIndex = vertices.Count;

                            vertices.Add(new Vector3(x, y, z));
                            vertices.Add(new Vector3(x, y + blockSize, z));
                            vertices.Add(new Vector3(x + blockSize, y + blockSize, z));
                            vertices.Add(new Vector3(x + blockSize, y, z));

                            UpdateChunkUV(blockType, direction);
                        }

                        if (!BlockExists(x, y, z, BlockFace.Forward, Blocks, frontBlocks))
                        {
                            direction = BlockFaceDirection.Side;

                            VerticesIndex = vertices.Count;

                            vertices.Add(new Vector3(x + blockSize, y, z + blockSize));
                            vertices.Add(new Vector3(x + blockSize, y + blockSize, z + blockSize));
                            vertices.Add(new Vector3(x, y + blockSize, z + blockSize));
                            vertices.Add(new Vector3(x, y, z + blockSize));


                            UpdateChunkUV(blockType, direction);
                        }
                    }
                }
            }
        }

        return new GeneratedChunk(chunk, Blocks, vertices.ToArray(), uvs.ToArray(), triangles.ToArray());

        void UpdateChunkUV(BlockType blockType, BlockFaceDirection direction)
        {
            triangles.Add(VerticesIndex);
            triangles.Add(VerticesIndex + 1);
            triangles.Add(VerticesIndex + 2);

            triangles.Add(VerticesIndex + 2);
            triangles.Add(VerticesIndex + 3);
            triangles.Add(VerticesIndex);

            Vector2 textureInterval = TextureManager.TextureInterval;

            int textureIndex = 0;

            switch (direction)
            {
                case BlockFaceDirection.Top:
                    textureIndex = TextureManager.TextureMap[blockType].TopIndex;
                    break;
                case BlockFaceDirection.Side:
                    textureIndex = TextureManager.TextureMap[blockType].SideIndex;
                    break;
                case BlockFaceDirection.Bottom:
                    textureIndex = TextureManager.TextureMap[blockType].BottomIndex;
                    break;
            }

            Vector2 textureId = new Vector2(textureInterval.x * (textureIndex % TextureManager.AtlasSize.x), textureInterval.y * Mathf.FloorToInt(textureIndex/TextureManager.AtlasSize.y));


            uvs.Add(new Vector2(textureId.x + textureInterval.x, textureId.y - textureInterval.y));
            uvs.Add(new Vector2(textureId.x + textureInterval.x, textureId.y));
            uvs.Add(new Vector2(textureId.x, textureId.y));
            uvs.Add(new Vector2(textureId.x, textureId.y - textureInterval.y));
        }
    }

    void GenerateTree(ThreeDimensionalBlockArray Blocks, Vector3 pos, TreeType treeType, System.Random random)
    {
        //System.Random random = new System.Random();

        int yStart = (int)pos.y + 1;
        int yEnd = yStart + random.Next(3, 7);
        int y = yStart;
        for (; y < yEnd; y++)
        {
            Blocks[(int)pos.x, y, (int)pos.z].BlockID = BlockType.OakLog;
        }
        Blocks[(int)pos.x, y, (int)pos.z].BlockID = BlockType.OakLeaf;

        // one leaf forward, back, left and right from the top leaf
        Vector3 current = new Vector3(pos.x, y, pos.z);
        Vector3 forward, back, left, right;
        forward = current + Vector3.forward;
        back = current + Vector3.back;
        left = current + Vector3.left;
        right = current + Vector3.right;

        if (Blocks.ElementExists(forward))
        {
            if (Blocks.Get(forward).BlockID == BlockType.Air)
                Blocks.Get(forward).BlockID = BlockType.OakLeaf;
        }
        if (Blocks.ElementExists(back))
        {
            if (Blocks.Get(back).BlockID == BlockType.Air)
                Blocks.Get(back).BlockID = BlockType.OakLeaf;
        }
        if (Blocks.ElementExists(left))
        {
            if (Blocks.Get(left).BlockID == BlockType.Air)
                Blocks.Get(left).BlockID = BlockType.OakLeaf;
        }
        if (Blocks.ElementExists(right))
        {
            if (Blocks.Get(right).BlockID == BlockType.Air)
                Blocks.Get(right).BlockID = BlockType.OakLeaf;
        }

        // one layer down, an entire ring
        current = new Vector3(pos.x, y - 1, pos.z);

        // 3x1x3 ring
        FillArea(Blocks, (int)current.x - 1, (int)current.y, (int)current.z - 1, (int)current.x + 1, (int)current.y, (int)current.z + 1, BlockType.OakLeaf);

        // 5x2x5 ring

        FillArea(Blocks, (int)current.x - 2, (int)current.y - 2, (int)current.z - 2, (int)current.x + 2, (int)current.y - 1, (int)current.z + 2, BlockType.OakLeaf);
    }

    void FillArea (ThreeDimensionalBlockArray Blocks, int xStart, int yStart, int zStart, int xEnd, int yEnd, int zEnd, BlockType blockType)
    {
        for (int y = yStart; y <= yEnd; y++)
        {
            for (int x = xStart; x <= xEnd; x++)
            {
                for (int z = zStart; z <= zEnd; z++)
                {
                    if (Blocks.ElementExists(x, y, z))
                    {
                        Block block = Blocks.Get(x, y, z);
                        if (block.BlockID == BlockType.Air)
                        {
                            block.BlockID = blockType;
                        }
                    }
                }
            }
        }
    }

    bool BlockExists (int x, int y, int z, BlockFace blockFace, ThreeDimensionalBlockArray currentBlocks, ThreeDimensionalBlockArray neighourBlocks)
    {
        if (neighourBlocks != null)
        {
            //Debug.Log("Neighbour is not null");
            // if at the edge of the array, we need to check neighbours
            if (x == 0 || x >= ((int)Chunk.ChunkSize.x - 1) || z == 0 || z >= ((int)Chunk.ChunkSize.z - 1))
            {
                switch (blockFace)
                {
                    case BlockFace.Top:
                    case BlockFace.Bottom:
                        break;

                    case BlockFace.Left:
                        if (x != 0) break;

                        return neighourBlocks.BlockExists((int)Chunk.ChunkSize.x, y, z);

                    case BlockFace.Right:
                        if (x != (int)Chunk.ChunkSize.x - 1) break;

                        return neighourBlocks.BlockExists(0, y, z);

                    case BlockFace.Forward:
                        if (z != (int)Chunk.ChunkSize.z - 1) break;

                        return neighourBlocks.BlockExists(x, y, 0);

                    case BlockFace.Back:
                        if (z != 0) break;

                        return neighourBlocks.BlockExists(x, y, (int)Chunk.ChunkSize.z - 1);
                }
            }
        }
        return currentBlocks.BlockExists(x, y, z, blockFace);
    }

    /// <summary>
    /// Represents a chunk after creating its blocks and mesh
    /// </summary>
    class GeneratedChunk
    {
        public bool ApplyBlocks = true;
        public readonly Chunk chunk;
        public readonly ThreeDimensionalBlockArray blocks;
        public readonly Vector3[] vertices;
        public readonly Vector2[] uvs;
        public readonly int[] triangles;

        public GeneratedChunk(Chunk chunk, ThreeDimensionalBlockArray blocks, Vector3[] vertices, Vector2[] uvs, int[] triangles)
        {
            this.chunk = chunk;
            this.blocks = blocks;
            this.vertices = vertices;
            this.uvs = uvs;
            this.triangles = triangles;
        }
    }

    /// <summary>
    /// Represents a chunk that needs to be processed and turned into a <see cref="GeneratedChunk"/>
    /// </summary>
    class RequestedChunk
    {
        public readonly ChunkIndex chunkIndex;
        public readonly NoiseSettings noiseSettings;
        public Action<ChunkIndex, Chunk> OnInstantiateCallback;
        public bool perlin;

        public RequestedChunk(ChunkIndex chunkIndex, NoiseSettings noiseSettings, Action<ChunkIndex, Chunk> onInstantiateCallback, bool perlin)
        {
            this.chunkIndex = chunkIndex;
            this.noiseSettings = noiseSettings;
            OnInstantiateCallback = onInstantiateCallback;
            this.perlin = perlin;
        }
    }
}
