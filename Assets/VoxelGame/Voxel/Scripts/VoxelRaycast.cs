using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelGame.Voxel
{
    public class VoxelRaycast
    {
        private static RaycastHit hit;

        public static bool Raycast(Vector3 postition, Vector3 direction, float distance, out Vector3Int hitVoxel, bool inVoxel, LayerMask layerMask)
        {
            if (Physics.Raycast(postition, direction, out hit, distance, layerMask))
            {
                hitVoxel = Vector3Int.FloorToInt(hit.point + (inVoxel ? -1f : 1f) * 0.5f * hit.normal);
                return true;
            }

            hitVoxel = Vector3Int.zero;
            return false;
        }
    }
}
