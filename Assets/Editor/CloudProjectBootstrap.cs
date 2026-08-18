using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public static class CloudProjectBootstrap
{
    private const string OpaqueVoxelGraph = "Assets/VoxelGame/Voxel/Shaders/ChunkUnlit.shadergraph";
    private const string TransparentVoxelGraph = "Assets/VoxelGame/Voxel/Shaders/ChunkUnlitTransparent.shadergraph";
    private const string HighFidelityRenderer = "Assets/Settings/URP-HighFidelity-Renderer.asset";
    private const string SunShaftsShaderPath = "Assets/AIEnhancements/Rendering/SunShafts.shader";

    private const string UnlitSubTarget = "UnityEditor.Rendering.Universal.ShaderGraph.UniversalUnlitSubTarget";
    private const string LitSubTarget = "UnityEditor.Rendering.Universal.ShaderGraph.UniversalLitSubTarget";

    [InitializeOnLoadMethod]
    private static void PatchProjectOnEditorLoad()
    {
        // Also fixes normal local-editor checkouts. The Cloud Build callback below repeats
        // the operation so a clean CI checkout is deterministic.
        EditorApplication.delayCall += () =>
        {
            if (PatchVoxelShaderGraphs())
            {
                AssetDatabase.SaveAssets();
                Debug.Log("[CloudProjectBootstrap] Voxel Shader Graphs upgraded from URP Unlit to URP Lit.");
            }
        };
    }

    public static void PreExport()
    {
        PatchVoxelShaderGraphs();
        SelectHighFidelityQuality();
        InstallSunShaftsRendererFeature();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        Debug.Log("[CloudProjectBootstrap] Cinematic URP lighting preparation complete.");
    }

    private static bool PatchVoxelShaderGraphs()
    {
        bool changed = false;
        changed |= PatchShaderGraph(OpaqueVoxelGraph);
        changed |= PatchShaderGraph(TransparentVoxelGraph);
        return changed;
    }

    private static bool PatchShaderGraph(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogWarning("[CloudProjectBootstrap] Shader Graph not found: " + path);
            return false;
        }

        string source = File.ReadAllText(path);
        if (!source.Contains(UnlitSubTarget))
            return false;

        File.WriteAllText(path, source.Replace(UnlitSubTarget, LitSubTarget));
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        Debug.Log("[CloudProjectBootstrap] Enabled real URP lighting/shadow passes for " + path);
        return true;
    }

    private static void SelectHighFidelityQuality()
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

    private static void InstallSunShaftsRendererFeature()
    {
        UniversalRendererData rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(HighFidelityRenderer);
        Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(SunShaftsShaderPath);

        if (rendererData == null)
        {
            Debug.LogError("[CloudProjectBootstrap] High Fidelity renderer data was not found.");
            return;
        }
        if (shader == null)
        {
            Debug.LogError("[CloudProjectBootstrap] Sun shafts shader was not found.");
            return;
        }

        SunShaftsRendererFeature feature = null;
        for (int i = 0; i < rendererData.rendererFeatures.Count; i++)
        {
            feature = rendererData.rendererFeatures[i] as SunShaftsRendererFeature;
            if (feature != null) break;
        }

        if (feature == null)
        {
            feature = ScriptableObject.CreateInstance<SunShaftsRendererFeature>();
            feature.name = "AI Screen-Space Sun Shafts";
            feature.shaftShader = shader;
            AssetDatabase.AddObjectToAsset(feature, rendererData);
            rendererData.rendererFeatures.Add(feature);
            Debug.Log("[CloudProjectBootstrap] Installed AI Screen-Space Sun Shafts renderer feature.");
        }
        else
        {
            feature.shaftShader = shader;
        }

        EditorUtility.SetDirty(feature);
        EditorUtility.SetDirty(rendererData);
    }
}
