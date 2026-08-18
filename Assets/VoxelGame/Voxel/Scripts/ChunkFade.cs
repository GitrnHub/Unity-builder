using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelGame.Voxel
{
    public class ChunkFade : MonoBehaviour
    {
        private float fadeIntencity = 1f;
        private float speed = 2f;
        private string fadeValueName = "_FadeIntencity";

        private IEnumerator Start()
        {
            var mat = GetComponent<MeshRenderer>().material;

            while (fadeIntencity > 0)
            {
                mat.SetFloat(fadeValueName, fadeIntencity);
                fadeIntencity -= Time.deltaTime * speed;

                yield return null;

                if (fadeIntencity < 0)
                {
                    mat.SetFloat(fadeValueName, 0);
                    Destroy(this);
                }
            }
        }
    }
}
