using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelGame.Voxel
{
    public static class VoxelData
    {
        public static readonly int chunkWidth = 16;
        public static readonly int chunkHeight = 128;
        public static readonly int textureAtlasSize = 16;
        public static readonly float textureAtlasElementSize = 0.0625f;

        public static readonly Vector3[] voxelVerts = new Vector3[8]
        {
            new (0f, 0f, 0f),
            new (1f, 0f, 0f),
            new (1f, 1f, 0f),
            new (0f, 1f, 0f),
            new (0f, 0f, 1f),
            new (1f, 0f, 1f),
            new (1f, 1f, 1f),
            new (0f, 1f, 1f) 
        };

        public static readonly Vector3[] voxelVertsDecorGrass = new Vector3[8]
        {
            new (0f, 0f, 0f),
            new (0.75f, 0f, 0f),
            new (0.75f, 1f, 0f),
            new (0f, 1f, 0f),
            new (0f, 0f, 0.75f),
            new (0.75f, 0f, 0.75f),
            new (0.75f, 1f, 0.75f),
            new (0f, 1f, 0.75f)
        };

        public static readonly Vector3[] faceChecks = new Vector3[6]
        {
            Vector3.back,
            Vector3.forward,
            Vector3.up,
            Vector3.down,
            Vector3.left,
            Vector3.right
        };

        public static readonly Vector3Int[] faceChecksInt = new Vector3Int[6]
        {
            Vector3Int.back,   
            Vector3Int.forward,
            Vector3Int.up,     
            Vector3Int.down,   
            Vector3Int.left,   
            Vector3Int.right   
        };

        public static readonly int[,] voxelTris = new int[6, 4]
        {
            {0, 3, 1, 2}, // back
            {5, 6, 4, 7}, // front
            {3, 7, 2, 6}, // top
            {1, 5, 0, 4}, // bottom
            {4, 7, 0, 3}, // left
            {1, 2, 5, 6}  // right
        };

        public static readonly Vector2[] voxelUvs = new Vector2[4]
        {
            new (0f, 0f),
            new (0f, 1f),
            new (1f, 0f),
            new (1f, 1f)
        };
    }
}
