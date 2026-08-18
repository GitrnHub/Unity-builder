using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelGame.Utilities;

namespace VoxelGame.Voxel
{
    public class DefaultGenerator : IGenerator
    {
        private readonly FastNoiseLite noiseHeight;
        private readonly FastNoiseLite noiseMount;
        private readonly FastNoiseLite noiseOcean;
        private readonly FastNoiseLite noiseRnd;

        private readonly VoxelSettingsSO voxelSettings;

        public DefaultGenerator(VoxelSettingsSO voxelSettings)
        {
            this.voxelSettings = voxelSettings;

            noiseHeight = new FastNoiseLite(voxelSettings.Seed);
            noiseHeight.SetFrequency(0.005f);
            noiseHeight.SetFractalType(FastNoiseLite.FractalType.FBm);
            noiseHeight.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            noiseHeight.SetFractalOctaves(3);

            noiseMount = new FastNoiseLite(voxelSettings.Seed + 1);
            noiseMount.SetFrequency(0.008f);
            noiseMount.SetFractalType(FastNoiseLite.FractalType.FBm);
            noiseMount.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            noiseMount.SetFractalOctaves(4);

            noiseOcean = new FastNoiseLite(voxelSettings.Seed + 2);
            noiseOcean.SetFrequency(0.002f);
            noiseOcean.SetFractalType(FastNoiseLite.FractalType.FBm);
            noiseOcean.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            noiseOcean.SetFractalOctaves(3);
        }

        uint p = 2147483647;
        uint a = 16807;
        
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

            float cachedHeight = noiseHeight.GetNoise(position.x, position.z);
            float cachedCont = noiseMount.GetNoise(position.x, position.z);
            float cachedOcean = noiseOcean.GetNoise(position.x, position.z);

            for (int y = mapY.Length - 1; y >= 0; y--)
            {
                mapY[y] = GetVoxel(position + new Vector3Int(0, y, 0), cachedHeight, cachedCont, cachedOcean);
                deep = mapY[y] > 0 ? deep + 1 : 0;
                mapY[y] = FixVoxel(mapY[y], lastVoxel, deep, y);

                if (y < mapY.Length - 1)
                {
                    rndCurrent = Rand(rndCurrent);
                    mapY[y + 1] = FixPrevVoxel(mapY[y], lastVoxel, deep, y, (int)(rndCurrent % 50));
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

                    if ((int)(rndCurrent % 200) != 70)
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

        private byte GetVoxel(Vector3Int pos, float cachedHeight, float cachedMount, float cachedOcean)
        {
            int baseHeight = VoxelData.chunkHeight / 2 - 4;
            int waterHeight = baseHeight - 4;

            float hFactor = pos.y / (float)VoxelData.chunkHeight;
            float hFade = (hFactor - 0.5f) * 2f;
            float noise3d = noiseMount.GetNoise(pos.x, pos.y, pos.z);

            if (cachedHeight > 0 && cachedMount > 0)
            {
                hFade -= cachedHeight * cachedMount;
            }

            cachedHeight += cachedOcean;

            if (noise3d * cachedHeight - hFade > 0)
            {
                if (cachedHeight > 0.5f && cachedMount > 0.25f)
                    return Voxels.STONE;

                if (pos.y < waterHeight + 1 * Mathf.RoundToInt(cachedMount + 0.5f) && cachedHeight < 0f)
                    return Voxels.SAND;

                return Voxels.GRASS;
            }

            if (pos.y < waterHeight)
            {
                return Voxels.WATER;
            }

            return Voxels.AIR;
        }

        private byte FixVoxel(byte voxel, byte lastVoxel, int deep, int y)
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

        private byte FixPrevVoxel(byte voxel, byte lastVoxel, int deep, int y, int rnd100)
        {
            if (lastVoxel == 0 && y < VoxelData.chunkHeight - 1)
                if (voxel == Voxels.GRASS)
                    if (rnd100 < 5)
                        return Voxels.DECOR_GRASS;

            return lastVoxel;
        }
    }
}
