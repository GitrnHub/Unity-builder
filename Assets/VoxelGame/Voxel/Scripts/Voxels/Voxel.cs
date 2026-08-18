using UnityEngine;
using NaughtyAttributes;

namespace VoxelGame.Voxel
{
    [System.Serializable]
    public class Voxel
    {
        public string name;
        public int textureId;
        public int textureIdBottom;
        public int textureIdSide;
        public bool isSolidForSame = false;
        public bool isEmpty = false;
        public bool isDecor = false;
        public bool isShapeX = false;
        public bool isTransp = false;
        public Color color;
        [ShowAssetPreview]
        public Sprite sprite;
    }
}