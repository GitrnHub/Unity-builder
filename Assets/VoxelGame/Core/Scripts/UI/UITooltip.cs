using System.Collections;
using UnityEngine;

namespace VoxelGame.Core
{
    public class UITooltip : MonoBehaviour
    {
        [SerializeField] private float duration = 4f;
        [SerializeField] private float fadeSpeed = 1f;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private bool autoShowOnEnable;

        private void OnEnable()
        {
            if (autoShowOnEnable)
            {
                Show();
            }
        }

        public void Show()
        {
            StopAllCoroutines();
            StartCoroutine(ShowC());
        }

        private IEnumerator ShowC()
        {
            canvasGroup.alpha = 1f;
            yield return new WaitForSeconds(duration);
            for (float t = 1f; t > 0f; t -= Time.deltaTime * fadeSpeed)
            {
                canvasGroup.alpha = t;
                yield return null;
            }
            canvasGroup.alpha = 0f;
        }
    }
}