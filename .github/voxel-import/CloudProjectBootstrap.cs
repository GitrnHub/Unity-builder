using UnityEditor;
using UnityEngine;

public static class CloudProjectBootstrap
{
    public static void PreExport()
    {
        string[] names = QualitySettings.names;
        for (int i = 0; i < names.Length; i++)
        {
            if (names[i].IndexOf("High Fidelity", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                QualitySettings.SetQualityLevel(i, true);
                Debug.Log("[CloudProjectBootstrap] High Fidelity quality selected.");
                return;
            }
        }
        Debug.LogWarning("[CloudProjectBootstrap] High Fidelity quality level not found; current quality retained.");
    }
}
