using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelGame.Voxel
{
    [CreateAssetMenu(fileName = "Voxels Preset", menuName = "Voxel/Voxels Preset Scriptable Object")]
    public class VoxelsPresetSO : ScriptableObject
    {
        [NaughtyAttributes.ReorderableList]
        [SerializeField] private Voxel[] voxels = new Voxel[256];
        
        public Voxel[] Voxels => voxels;
    }
}
