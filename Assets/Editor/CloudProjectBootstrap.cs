using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class CloudProjectBootstrap
{
    private const string SceneFolder = "Assets/Scenes";
    private const string ScenePath = "Assets/Scenes/Main.unity";

    static CloudProjectBootstrap()
    {
        EnsureScene();
    }

    public static void PreExport()
    {
        EnsureScene();
    }

    private static void EnsureScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;

        if (!Directory.Exists(SceneFolder))
            Directory.CreateDirectory(SceneFolder);

        if (!File.Exists(ScenePath))
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject cameraObject = new GameObject("Main Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 70f;
            camera.nearClipPlane = 0.05f;
            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<FreeFlyController>();

            GameObject world = new GameObject("World");
            world.AddComponent<WanderWorld>();

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
        }

        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        PlayerSettings.companyName = "VibeCloud";
        PlayerSettings.productName = "Cloud Wander Demo";
        PlayerSettings.bundleVersion = "0.1.0";
    }
}
