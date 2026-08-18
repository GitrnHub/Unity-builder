using UnityEngine;

namespace VoxelGame.Voxel
{
    [System.Serializable]
    public class SavedChunkData
    {
        public byte[] map;

        public int x;
        public int z;

        public void SetCoords(Vector2Int coords)
        {
            x = coords.x;
            z = coords.y;
        }

        public byte[,,] GetMap()
        {
            byte[,,] m = new byte[VoxelData.chunkWidth, VoxelData.chunkHeight, VoxelData.chunkWidth];

            int id = 0;

            for (int x = 0; x < VoxelData.chunkWidth; x++)
                for (int y = 0; y < VoxelData.chunkHeight; y++)
                    for (int z = 0; z < VoxelData.chunkWidth; z++)
                    {
                        m[x, y, z] = map[id];
                        id++;
                    }

            return m;
        }

        public void SetMap(byte[,,] m)
        {
            map = new byte[VoxelData.chunkWidth * VoxelData.chunkHeight * VoxelData.chunkWidth];

            int id = 0;

            for (int x = 0; x < VoxelData.chunkWidth; x++)
                for (int y = 0; y < VoxelData.chunkHeight; y++)
                    for (int z = 0; z < VoxelData.chunkWidth; z++)
                    {
                        map[id] = m[x, y, z];
                        id++;
                    }
        }
    }
}