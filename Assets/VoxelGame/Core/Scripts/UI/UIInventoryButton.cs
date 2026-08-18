using UnityEngine;
using UnityEngine.UI;

namespace VoxelGame.Core
{
    public class UIInventoryButton : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshProUGUI text;
        [SerializeField] private Button button;
        [SerializeField] private Image image;
        public Button Button => button;

        public void Set(string itemName, Sprite sprite)
        {
            text.text = itemName;
            image.sprite = sprite;
        }
    }
}