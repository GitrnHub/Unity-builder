using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelGame.Voxel
{
    [CreateAssetMenu(fileName = "VoxelStructure", menuName = "Voxel/Structure Scriptable Object")]
    public class VoxelStructureSO : ScriptableObject
    {
        public string structureTag;
        public Vector3Int offset;
        public VoxelStructure structure;
    }
}
