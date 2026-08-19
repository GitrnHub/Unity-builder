using UnityEditor;
using UnityEngine;

public static class CloudProjectBootstrap
{
    [InitializeOnLoadMethod]
    private static void PrepareEditorCheckout()
    {
        EditorApplication.delayCall += SelectHighFidelityQuality;
    }

    public static void PreExport()
    {
        // Lighting assets are now committed as real project assets so Cloud Build and
        // GameCI consume the exact same shader/material/renderer configuration. Do not
        // mutate Shader Graph JSON at build time.
        SelectHighFidelityQuality();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        Debug.Log("[CloudProjectBootstrap] Persistent cinematic URP assets verified for build.");
    }

    private static void SelectHighFidelityQuality()
    {
        string[] names = QualitySettings.names;
        for (int i = 0; i < names.Length; i++)
        {
            if (names[i].IndexOf("High Fidelity", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (QualitySettings.GetQualityLevel() != i)
                    QualitySettings.SetQualityLevel(i, true);
                Debug.Log("[CloudProjectBootstrap] High Fidelity quality selected.");
                return;
            }
        }

        Debug.LogWarning("[CloudProjectBootstrap] High Fidelity quality level not found; current quality retained.");
    }
}
