using UnityEngine;
using UnityEngine.EventSystems;

namespace VoxelGame.Core
{
    public class UIFirstSelect : MonoBehaviour
    {
        private void OnEnable()
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }
}
