using UnityEngine;
using Utilities;

namespace Arcanist
{
    public class AudioManager : SingletonBase<AudioManager>
    {
        [SerializeField] private AudioSource bgmSource;

        public void PlayBGM(AudioClip clip)
        {
            if (bgmSource.clip == clip)
                return;

            bgmSource.clip = clip;
            bgmSource.Play();
        }
    }
}