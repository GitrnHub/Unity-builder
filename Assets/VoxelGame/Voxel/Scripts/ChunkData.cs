using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelGame.Voxel
{
    public class ChunkData
    {
        public Vector2Int Coords { get; private set; }
        public Vector3Int Position { get; private set; }
        public byte[,,] map;
        public byte[,,] light;

        public ChunkData(Vector2Int coords)
        {
            Coords = coords;
            Position = (Vector3Int.right * coords.x + Vector3Int.forward * coords.y) * VoxelData.chunkWidth;
            map = new byte[VoxelData.chunkWidth, VoxelData.chunkHeight, VoxelData.chunkWidth];
            light = new byte[VoxelData.chunkWidth, VoxelData.chunkHeight, VoxelData.chunkWidth];
        }
    }
}