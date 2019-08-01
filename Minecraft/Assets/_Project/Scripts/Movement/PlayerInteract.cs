using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    public Camera MainCamera;
    public LayerMask BlockLayerMask;
    public ChunkGenerator ChunkGenerator;
    public Rigidbody Rigidbody;
    Vector3 block;
    //Vector3 chunkPos;
    BlockHitInfo blockHitInfo;

    void Update()
    {
        if (!blockHitInfo.Success)
        {
            block = Vector3.zero;
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (blockHitInfo.Success)
            {
                if (blockHitInfo.block.BlockID != BlockType.Air)
                {
                    blockHitInfo.block.BlockID = BlockType.Air;
                    ChunkGenerator.RequestChunkRefresh(blockHitInfo.chunk);
                }
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (blockHitInfo.Success)
            {
                if (blockHitInfo.closeBlock.BlockID == BlockType.Air)
                {
                    blockHitInfo.closeBlock.BlockID = BlockType.Dirt;
                    ChunkGenerator.RequestChunkRefresh(blockHitInfo.chunk);
                }
                else if (blockHitInfo.block.BlockID == BlockType.Air)
                {
                    blockHitInfo.block.BlockID = BlockType.Dirt;
                    ChunkGenerator.RequestChunkRefresh(blockHitInfo.chunk);
                }
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        blockHitInfo = ChunkAndBlockSearch.Raycast(MainCamera, 4, BlockLayerMask, false);
        if (blockHitInfo.Success)
        {
            block = blockHitInfo.block.BlockWorldPosition(blockHitInfo.chunk);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(block, Vector3.one);
    }

    void OnDrawGizmosSelected()
    {
        // world center
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(Vector3.zero + (Vector3.one / 2), new Vector3(1, 200, 1));
    }
}
