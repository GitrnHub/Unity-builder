using System.Collections.Generic;
using UnityEngine;

namespace VoxelGame.Voxel
{
    public class FlatGenerator : IGenerator
    {
        private readonly VoxelSettingsSO voxelSettings;

        public FlatGenerator(VoxelSettingsSO voxelSettings)
        {
            this.voxelSettings = voxelSettings;
        }

        const uint p = 2147483647;
        const uint a = 16807;
        
        private uint Rand(uint current)
        {
            current = a * current % p;
            return current;
        }

        public void GenerateMap(Vector3Int position, ref byte[,,] map)
        {
            if (map == null)
                return;

            int lengthX = map.GetLength(0);
            int lengthY = map.GetLength(1);
            int lengthZ = map.GetLength(2);

            if (lengthX == 0 || lengthY == 0 || lengthZ == 0)
                return;

            byte[] tmpMapColumn = new byte[lengthY];

            for (int x = 0; x < lengthX; x++)
                for (int z = 0; z < lengthZ; z++)
                {
                    GenerateMapColumn(position + new Vector3Int(x, 0, z), tmpMapColumn);

                    for (int y = 0; y < lengthY; y++)
                        map[x, y, z] = tmpMapColumn[y];
                }
        }

        public void GenerateChunkStructures(Vector2Int coords, ref byte[,,] map)
        {
            List<ChunkStructure> structures = new List<ChunkStructure>();

            for (int x = -1; x <= 1; x++)
                for (int z = -1; z <= 1; z++)
                    structures.AddRange(GetChunkStructures(coords + new Vector2Int(x, z)));

            ChunkStructure.CreateStructures(coords, map, structures);
        }

        private void GenerateMapColumn(Vector3Int position, byte[] mapY)
        {
            uint rndCurrent = (uint)(voxelSettings.Seed + position.x + position.z * 7 + voxelSettings.Seed * position.x);

            byte lastVoxel = 0;
            int deep = 0;

            for (int y = mapY.Length - 1; y >= 0; y--)
            {
                mapY[y] = GetVoxel(position + new Vector3Int(0, y, 0));
                deep = mapY[y] > 0 ? deep + 1 : 0;
                mapY[y] = FixVoxel(mapY[y], deep);

                if (y < mapY.Length - 1)
                {
                    rndCurrent = Rand(rndCurrent);
                    mapY[y + 1] = FixPrevVoxel(mapY[y], lastVoxel, y, (int)(rndCurrent % 50));
                }

                lastVoxel = mapY[y];
            }
        }

        private List<ChunkStructure> GetChunkStructures(Vector2Int coords)
        {
            Vector3Int position = VoxelUtils.GetChunkPosition(coords);

            uint rndCurrent = (uint)(position.x + position.z * 7 + voxelSettings.Seed);

            List<ChunkStructure> structures = new List<ChunkStructure>();

            for (int x = 0; x < VoxelData.chunkWidth; x++)
                for (int z = 0; z < VoxelData.chunkWidth; z++)
                {
                    rndCurrent = Rand(rndCurrent);

                    if ((int)(rndCurrent % 400) != 1)
                        continue;

                    byte[] mapColumn = new byte[VoxelData.chunkHeight];
                    GenerateMapColumn(position + new Vector3Int(x, 0, z), mapColumn);

                    for (int y = VoxelData.chunkHeight - 1; y >= 0; y--)
                    {
                        Vector3Int v = new Vector3Int(x, y, z);

                        if (!VoxelUtils.IsVoxelInChunk(v))
                            continue;

                        byte voxel = mapColumn[y];

                        if (voxel == Voxels.GRASS)
                        {
                            ChunkStructure s = new ChunkStructure();
                            s.position = new Vector3Int(x, y + 1, z) + position;
                            s.structure = voxelSettings.Structures.GetStructure("tree_1");
                            structures.Add(s);

                            break;
                        }
                    }
                }

            return structures;
        }

        private byte GetVoxel(Vector3Int pos)
        {
            return pos.y > VoxelData.chunkHeight / 2 ? Voxels.AIR : Voxels.GRASS;
        }

        private byte FixVoxel(byte voxel, int deep)
        {
            if (voxel == Voxels.GRASS)
            {
                if (deep == 1)
                {
                    return Voxels.GRASS;
                }
                else if (deep < 4)
                {
                    return Voxels.DIRT;
                }
                else
                {
                    return Voxels.STONE;
                }
            }

            return voxel;
        }

        private byte FixPrevVoxel(byte voxel, byte lastVoxel, int y, int rnd100)
        {
            if (lastVoxel == 0 && y < VoxelData.chunkHeight - 1)
                if (voxel == Voxels.GRASS)
                    if (rnd100 < 5)
                        return Voxels.DECOR_GRASS;

            return lastVoxel;
        }
    }
}
