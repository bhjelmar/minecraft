using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blocks
{
    public static Dictionary<BlockType, BlockData> blockTypes = new Dictionary<BlockType, BlockData>
        {
            { BlockType.AIR,     new BlockData(BlockType.AIR,     false, 0,  0,  0,  0,  0,  0)  },
            { BlockType.BEDROCK, new BlockData(BlockType.BEDROCK, true,  9,  9,  9,  9,  9,  9)  },
            { BlockType.STONE,   new BlockData(BlockType.STONE,   true,  0,  0,  0,  0,  0,  0)  },
            { BlockType.GRASS,   new BlockData(BlockType.GRASS,   true,  2,  2,  7,  1,  2,  2)  },
            { BlockType.FURNACE, new BlockData(BlockType.FURNACE, true,  2,  2,  7,  1,  2,  2)  },
            { BlockType.SAND,    new BlockData(BlockType.SAND, true,  10, 10, 10, 10, 10, 10) },
            { BlockType.DIRT,    new BlockData(BlockType.DIRT, true,  1, 1, 1, 1, 1, 1) },
        };

    [System.Serializable]
    public class BlockData
    {
        public BlockType type;
        public bool isSolid;

        [Header("Texture Values")]
        public int backFaceTexture;
        public int frontFaceTexture;
        public int topFaceTexture;
        public int bottomFaceTexture;
        public int leftFaceTexture;
        public int rightFaceTexture;

        public BlockData(BlockType type, bool isSolid, int backFaceTexture, int frontFaceTexture, int topFaceTexture, int bottomFaceTexture, int leftFaceTexture, int rightFaceTexture)
        {
            this.type = type;
            this.isSolid = isSolid;
            this.backFaceTexture = backFaceTexture;
            this.frontFaceTexture = frontFaceTexture;
            this.topFaceTexture = topFaceTexture;
            this.bottomFaceTexture = bottomFaceTexture;
            this.leftFaceTexture = leftFaceTexture;
            this.rightFaceTexture = rightFaceTexture;
        }

        public int GetTextureId(int faceIndex)
        {
            switch (faceIndex)
            {
                case 0:
                    return backFaceTexture;
                case 1:
                    return frontFaceTexture;
                case 2:
                    return topFaceTexture;
                case 3:
                    return bottomFaceTexture;
                case 4:
                    return leftFaceTexture;
                case 5:
                    return rightFaceTexture;
                default:
                    Debug.Log($"Error in GetTextureId; invalid face indedx: {faceIndex}");
                    return 0;
            }
        }

    }

    [System.Serializable]
    public enum BlockType
    {
        AIR,
        BEDROCK,
        STONE,
        GRASS,
        FURNACE,
        SAND,
        DIRT,
    }
}
