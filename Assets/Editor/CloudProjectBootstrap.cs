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
    private const string VoxelSubTargetObjectId = "9ad4023d137a4c36b2747480779c0869";

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

        // The existing graph already contains BaseColor, Specular and Smoothness blocks.
        // Switch the URP sub-target and serialize the Lit-specific settings explicitly.
        string oldBlock =
            "\"m_Type\": \"" + UnlitSubTarget + "\",\n" +
            "    \"m_ObjectId\": \"" + VoxelSubTargetObjectId + "\"";
        string newBlock =
            "\"m_Type\": \"" + LitSubTarget + "\",\n" +
            "    \"m_ObjectId\": \"" + VoxelSubTargetObjectId + "\",\n" +
            "    \"m_WorkflowMode\": 1,\n" +
            "    \"m_NormalDropOffSpace\": 0,\n" +
            "    \"m_ClearCoat\": false";

        string patched = source.Replace(oldBlock, newBlock);
        if (patched == source)
        {
            // Fallback for a future Shader Graph formatting change.
            patched = source.Replace(UnlitSubTarget, LitSubTarget);
        }

        File.WriteAllText(path, patched);
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

            // ScriptableRendererData keeps a parallel local-file-ID map. Populate it so
            // the newly-created feature survives serialization into the player build.
            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(feature, out string _, out long localId))
            {
                SerializedObject serializedRenderer = new SerializedObject(rendererData);
                SerializedProperty featureMap = serializedRenderer.FindProperty("m_RendererFeatureMap");
                if (featureMap != null)
                {
                    int index = featureMap.arraySize;
                    featureMap.InsertArrayElementAtIndex(index);
                    featureMap.GetArrayElementAtIndex(index).longValue = localId;
                    serializedRenderer.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            Debug.Log("[CloudProjectBootstrap] Installed AI Screen-Space Sun Shafts renderer feature.");
        }
        else
        {
            feature.shaftShader = shader;
        }

        EditorUtility.SetDirty(feature);
        EditorUtility.SetDirty(rendererData);
        rendererData.SetDirty();
    }
}
