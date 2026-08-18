using UnityEngine;

namespace VoxelGame.Core
{
    public class RandomSound : MonoBehaviour
    {
        [SerializeField] private AudioSource source;
        [SerializeField] private AudioClip[] clips;

        private float lastTimePlay;

        public void PlayRandomSound()
        {
            if (clips.Length == 0)
                return;

            var clip = clips[Random.Range(0, clips.Length)];

            source.PlayOneShot(clip);
            lastTimePlay = Time.time;
        }

        public void PlayRandomSound(float delay)
        {
            if (Time.time - lastTimePlay < delay)
                return;

            PlayRandomSound();
        }

        public void PlayRandomSound(Vector3 position)
        {
            transform.position = position;
            PlayRandomSound();
        }
    }
}
