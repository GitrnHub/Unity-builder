using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelGame.Utilities
{
    public class Follow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset;
        [SerializeField] private bool followX;
        [SerializeField] private bool followY;
        [SerializeField] private bool followZ;


        private void Update()
        {
            if (target == null)
                return;

            transform.position = new Vector3(
                followX ? target.position.x + offset.x : transform.position.x,
                followY ? target.position.y + offset.y : transform.position.y,
                followZ ? target.position.z + offset.z : transform.position.z
                );
        }
    }
}