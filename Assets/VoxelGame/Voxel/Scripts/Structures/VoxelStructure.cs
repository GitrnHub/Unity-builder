using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelGame.Voxel
{
    [System.Serializable]
    public class VoxelStructure
    {
        public List<VoxelStructureElement> elements = new List<VoxelStructureElement>();
    }

    [System.Serializable]
    public class VoxelStructureElement
    {
        public Vector3Int offset;
        public byte voxel;

        public VoxelStructureElement(Vector3Int offset, byte voxel)
        {
            this.offset = offset;
            this.voxel = voxel;
        }
    }
}