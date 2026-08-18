#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace VoxelGame.Voxel
{
    public class SaveLoadEditorMenu
    {
        [MenuItem("Voxel/SaveLoad/Open saved data folder")]
        private static void OpenSavedData()
        {
            EditorUtility.RevealInFinder(Application.persistentDataPath);
        }

        [MenuItem("Voxel/SaveLoad/Remove saved data")]
        private static void RemoveSavedData()
        {
            FileUtil.DeleteFileOrDirectory(Application.persistentDataPath);
        }

        [MenuItem("Voxel/PlayerPrefs/Clear")]
        private static void ClearPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
        }
    }
}
#endif