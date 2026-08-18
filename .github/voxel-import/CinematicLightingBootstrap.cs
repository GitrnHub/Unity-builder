using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public sealed class CinematicLightingBootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (FindObjectOfType<CinematicLightingBootstrap>() != null) return;
        GameObject go = new GameObject("[AI] Cinematic Lighting");
        DontDestroyOnLoad(go);
        go.AddComponent<CinematicLightingBootstrap>();
    }

    private void Awake()
    {
        SelectHighFidelityQuality();
        CreatePostProcessing();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        StartCoroutine(ApplyDelayed());
        InvokeRepeating(nameof(RefreshDynamicScene), 2f, 2f);
    }

    private void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => StartCoroutine(ApplyDelayed());

    private IEnumerator ApplyDelayed()
    {
        yield return null;
        yield return null;
        RefreshDynamicScene();
    }

    private static void SelectHighFidelityQuality()
    {
        string[] names = QualitySettings.names;
        for (int i = 0; i < names.Length; i++)
        {
            if (names[i].IndexOf("High Fidelity", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (QualitySettings.GetQualityLevel() != i) QualitySettings.SetQualityLevel(i, true);
                break;
            }
        }
    }

    private static void RefreshDynamicScene()
    {
        Camera[] cameras = FindObjectsOfType<Camera>(true);
        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].allowHDR = true;
            UniversalAdditionalCameraData data = cameras[i].GetComponent<UniversalAdditionalCameraData>();
            if (data != null) data.renderPostProcessing = true;
        }

        Light[] lights = FindObjectsOfType<Light>(true);
        Light sun = RenderSettings.sun;
        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i].type == LightType.Directional && (sun == null || lights[i].intensity > sun.intensity))
                sun = lights[i];
        }

        if (sun != null)
        {
            RenderSettings.sun = sun;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.92f;
            sun.shadowResolution = LightShadowResolution.VeryHigh;
            sun.shadowBias = 0.035f;
            sun.shadowNormalBias = 0.25f;
            if (sun.intensity < 1.05f) sun.intensity = 1.05f;
        }

        Scene active = SceneManager.GetActiveScene();
        if (active.IsValid() && active.name.IndexOf("Game", StringComparison.OrdinalIgnoreCase) >= 0 && !RenderSettings.fog)
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.0032f;
            RenderSettings.fogColor = Color.Lerp(new Color(0.46f, 0.57f, 0.68f), RenderSettings.ambientSkyColor, 0.35f);
        }
    }

    private void CreatePostProcessing()
    {
        GameObject volumeObject = new GameObject("[AI] Global Cinematic Volume");
        volumeObject.transform.SetParent(transform, false);
        Volume volume = volumeObject.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 1000f;

        VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
        profile.name = "AI Cinematic Runtime Profile";
        volume.profile = profile;

        Bloom bloom = profile.Add<Bloom>(true);
        bloom.intensity.Override(0.22f);
        bloom.threshold.Override(1.05f);
        bloom.scatter.Override(0.62f);

        Tonemapping tonemapping = profile.Add<Tonemapping>(true);
        tonemapping.mode.Override(TonemappingMode.ACES);

        ColorAdjustments color = profile.Add<ColorAdjustments>(true);
        color.postExposure.Override(0.05f);
        color.contrast.Override(7f);
        color.saturation.Override(4f);

        WhiteBalance whiteBalance = profile.Add<WhiteBalance>(true);
        whiteBalance.temperature.Override(3f);

        Vignette vignette = profile.Add<Vignette>(true);
        vignette.intensity.Override(0.12f);
        vignette.smoothness.Override(0.45f);
    }
}
