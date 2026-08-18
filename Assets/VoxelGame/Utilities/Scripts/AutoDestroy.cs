using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelGame.Utilities
{
    public class AutoDestroy : MonoBehaviour
    {
        public float lifeTime = 1f;

        void Update()
        {
            lifeTime -= Time.deltaTime;

            if (lifeTime < 0)
                Destroy(gameObject);
        }
    }
}
