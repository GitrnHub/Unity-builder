using System;
using UnityEngine;

using Lean.Transition;

namespace VoxelGame.Core
{
    public class CanvasFader : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] LeanManualAnimation fadeInTransition;
        [SerializeField] LeanManualAnimation fadeOutTransition;

        private Action onFadeInComplete;
        private Action onFadeOutComplete;

        private void Awake()
        {
            canvasGroup.gameObject.SetActive(true);
            Fade(false);
        }

        public void Fade(bool isFade, Action onComplete = null)
        {
            if (isFade)
            {
                fadeInTransition.BeginTransitions();

                if (onComplete != null)
                {
                    onFadeInComplete = onComplete;
                }
            }
            else
            {
                fadeOutTransition.BeginTransitions();

                if (onComplete != null)
                {
                    onFadeOutComplete = onComplete;
                }
            }
        }

        public void FadeInComplete()
        {
            onFadeInComplete?.Invoke();
            onFadeInComplete = null;
        }

        public void FadeOntComplete()
        {
            onFadeOutComplete?.Invoke();
            onFadeOutComplete = null;
        }
    }
}
