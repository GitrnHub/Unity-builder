using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CloudProjectBootstrap
{
    private const string SceneFolder = "Assets/Scenes";
    private const string ScenePath = "Assets/Scenes/Main.unity";

    public static void PreExport()
    {
        Debug.Log("[CloudProjectBootstrap] PreExport started.");

        if (!AssetDatabase.IsValidFolder(SceneFolder))
        {
            AssetDatabase.CreateFolder("Assets", "Scenes");
        }

        // Create the build scene only at the documented pre-export hook. Doing this
        // from InitializeOnLoad can run during domain reload/import and is fragile in CI.
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 70f;
        camera.nearClipPlane = 0.05f;
        camera.farClipPlane = 350f;
        cameraObject.AddComponent<AudioListener>();
        cameraObject.AddComponent<FreeFlyController>();

        GameObject world = new GameObject("World");
        world.AddComponent<WanderWorld>();

        if (!EditorSceneManager.SaveScene(scene, ScenePath))
        {
            throw new System.Exception("Failed to save generated scene: " + ScenePath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(ScenePath, true)
        };

        PlayerSettings.companyName = "VibeCloud";
        PlayerSettings.productName = "Cloud Wander Demo";
        PlayerSettings.bundleVersion = "0.1.0";

        Debug.Log("[CloudProjectBootstrap] Scene generated and added to build settings: " + ScenePath);
    }
}
