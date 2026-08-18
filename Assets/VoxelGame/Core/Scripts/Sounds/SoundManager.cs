using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelGame.Core
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance => instance;
        private static SoundManager instance;

        [SerializeField] private AudioSource audioSource;
        [SerializeField] private Sound[] sounds;

        Dictionary<string, Sound> soundsDict;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                throw new UnityException($"Can`t be more than one {this.GetType()} instances");
            }

            Init();
        }

        private void Init()
        {
            soundsDict = new Dictionary<string, Sound>();

            foreach (var sound in sounds)
            {
                soundsDict.Add(sound.tag, sound);
            }
        }

        public void PlaySound(string soundTag, Vector3 position)
        {
            if (soundsDict.TryGetValue(soundTag, out var sound))
            {
                if (sound == null)
                    return;
                
                if (sound.clip == null)
                    return;

                audioSource.transform.position = position;
                audioSource.spatialBlend = sound.is3d ? 1f : 0f;
                audioSource.PlayOneShot(sound.clip, sound.volume);
            }
        }
    }

    [System.Serializable]
    public class Sound
    {
        public string tag;
        public AudioClip clip;
        public float volume;
        public bool is3d;
    }
}
