using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelGame.Utilities
{
    [RequireComponent(typeof(Camera))]
    public class CullDistances : MonoBehaviour
    {
        [SerializeField] private CullDistanceParam[] cullDistances;

        private void Awake()
        {
            var cam = GetComponent<Camera>();
            var camCullDistances = cam.layerCullDistances;
            foreach (var cd in cullDistances)
            {
                camCullDistances[cd.layerId] = cd.distance;
            }
            cam.layerCullDistances = camCullDistances;
        }

        [Serializable]
        internal struct CullDistanceParam
        {
            public int layerId;
            public float distance;
        }
    }
}
