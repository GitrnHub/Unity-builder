namespace VoxelGame.Voxel
{
    public static class Voxels
    {
        public const byte AIR = 0;
        public const byte STONE = 1;
        public const byte DIRT = 2;
        public const byte GRASS = 3;
        public const byte PLANK = 4;
        public const byte DECOR_GRASS = 15;
        public const byte SAND = 18;
        public const byte WATER = 205;
        public const byte LEAVES = 52;

        public static readonly Voxel[] voxels = new Voxel[256];

        public static void Init(Voxel[] voxels)
        {
            for(int id = 0; id < voxels.Length; id++)
            {
                Voxels.voxels[id] = voxels[id];
            }
        }
    }
}