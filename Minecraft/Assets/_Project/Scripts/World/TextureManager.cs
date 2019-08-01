using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct SerialisedBlockTextures
{
    public BlockType BlockType;
    public BlockTextures BlockTextures;
}

[Serializable]
public struct BlockTextures
{
    public int TopIndex;
    public int SideIndex;
    public int BottomIndex;

    public BlockTextures(int topIndex, int sideIndex, int bottomIndex)
    {
        TopIndex = topIndex;
        SideIndex = sideIndex;
        BottomIndex = bottomIndex;
    }
}

/// <summary>
/// Defines texture data for block types and gives UV data for generation
/// </summary>
public class TextureManager : MonoBehaviour
{
    /// <summary>
    /// Pixel width and height of each block face in the atlas
    /// </summary>
    public Vector2 TextureBlockSize = new Vector2(16, 16);

    /// <summary>
    /// Main texture atlas used for the game
    /// </summary>
    public Texture TextureAtlas;

    [HideInInspector]
    public Vector2 TextureInterval;

    /// <summary>
    /// The number of textures within the atlas
    /// </summary>
    [HideInInspector]
    public Vector2 AtlasSize;

    public static Dictionary<BlockType, BlockTextures> TextureMap = new Dictionary<BlockType, BlockTextures>();

    public SerialisedBlockTextures[] BlockTextures = new SerialisedBlockTextures[0];

    // caching texture positions
    void Awake()
    {
        AtlasSize = new Vector2(TextureAtlas.width / TextureBlockSize.x, TextureAtlas.height / TextureBlockSize.y);
        TextureInterval = new Vector2(1 / AtlasSize.x, 1 / AtlasSize.y);

        for (int i = 0; i < BlockTextures.Length; i++)
        {
            if (!TextureMap.ContainsKey(BlockTextures[i].BlockType))
            {
                TextureMap.Add(BlockTextures[i].BlockType, new BlockTextures(BlockTextures[i].BlockTextures.TopIndex, BlockTextures[i].BlockTextures.SideIndex, BlockTextures[i].BlockTextures.BottomIndex));
            }
        }
    }
}
