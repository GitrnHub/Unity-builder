using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VoxelGame.Core
{
    public class UISelectHighlightControl : MonoBehaviour
    {
        [SerializeField]
        private Sprite highlightSprite;

        private GameObject highlight;
        private GameObject lastSelectedGameObject;

        void Update()
        {
            if (EventSystem.current == null)
                return;

            if (EventSystem.current.currentSelectedGameObject == null)
            {
                if (highlight != null)
                {
                    Destroy(highlight);
                }
                return;
            }

            if (EventSystem.current.currentSelectedGameObject == lastSelectedGameObject)
                return;

            Selectable selectable = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();

            if (selectable != null)
            {
                CreateHighlight(EventSystem.current.currentSelectedGameObject);
            }
        }

        private void CreateHighlight(GameObject target)
        {
            if (highlight != null)
            {
                Destroy(highlight);
                return;
            }

            lastSelectedGameObject = target;

            highlight = new GameObject();
            RectTransform highlightRT = highlight.AddComponent<RectTransform>();
            Image highlightImage = highlight.AddComponent<Image>();
            highlightImage.raycastTarget = false;
            highlightImage.sprite = highlightSprite;
            highlightImage.type = Image.Type.Sliced;
            highlightImage.pixelsPerUnitMultiplier = 0.5f;

            RectTransform parentRT = target.GetComponent<RectTransform>();
            highlightRT.SetParent(parentRT, false);
            highlightRT.localPosition = Vector3.zero;
            highlightRT.sizeDelta = parentRT.sizeDelta;
            highlightRT.localScale = Vector3.one;

            highlight.SetActive(true);
        }
    }
}