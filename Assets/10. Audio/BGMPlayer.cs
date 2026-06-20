using System.Collections;
using UnityEngine;

namespace Arcanist
{
    public class SceneBGMPlayer : MonoBehaviour
    {
        [SerializeField] private AudioClip bgmClip;
        [SerializeField] private float playDelay = 1.0f;

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(playDelay);

            AudioManager.Instance.PlayBGM(bgmClip);
        }
    }
}