using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelGame.Voxel
{
    public static class VoxelUtils
    {
        public static bool IsVoxelInChunk(Vector3Int pos)
        {
            if (pos.x < 0 || pos.x > VoxelData.chunkWidth - 1 ||
                pos.y < 0 || pos.y > VoxelData.chunkHeight - 1 ||
                pos.z < 0 || pos.z > VoxelData.chunkWidth - 1)
                return false;
            else
                return true;
        }

        public static Vector3Int GetLocalPos(Vector3Int worldPos, Vector2Int chunkCoords)
        {
            return worldPos - new Vector3Int(
                    chunkCoords.x * VoxelData.chunkWidth,
                    0,
                    chunkCoords.y * VoxelData.chunkWidth);
        }

        public static Vector2Int GetChunkCoords(Vector3Int worldPos)
        {
            return new Vector2Int(
                Mathf.FloorToInt(worldPos.x / (float)VoxelData.chunkWidth),
                Mathf.FloorToInt(worldPos.z / (float)VoxelData.chunkWidth));
        }

        public static Vector3Int GetChunkPosition(Vector2Int coords)
        {
            return new Vector3Int(
                coords.x * VoxelData.chunkWidth,
                0,
                coords.y * VoxelData.chunkWidth);
        }
    }
}
