using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VoxelGame.Utilities
{
    public class OnEnableTrigger : MonoBehaviour
    {
        public UnityEvent OnEnableEvent;

        private void OnEnable()
        {
            OnEnableEvent?.Invoke();
        }
    }
}
