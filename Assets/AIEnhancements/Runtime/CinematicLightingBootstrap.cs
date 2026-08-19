using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public sealed class CinematicLightingBootstrap : MonoBehaviour
{
    private static bool sunDirectionApplied;

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
        InvokeRepeating(nameof(RefreshDynamicScene), 1.0f, 1.5f);
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

        QualitySettings.shadows = UnityEngine.ShadowQuality.All;
        QualitySettings.shadowResolution = UnityEngine.ShadowResolution.VeryHigh;
        QualitySettings.shadowDistance = 180f;
        QualitySettings.shadowCascades = 4;
        QualitySettings.antiAliasing = 4;
    }

    private static void RefreshDynamicScene()
    {
        Camera[] cameras = FindObjectsOfType<Camera>(true);
        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].allowHDR = true;
            UniversalAdditionalCameraData data = cameras[i].GetComponent<UniversalAdditionalCameraData>();
            if (data != null)
            {
                data.renderPostProcessing = true;
                data.requiresDepthTexture = true;
                data.requiresColorTexture = true;
            }
        }

        Light[] lights = FindObjectsOfType<Light>(true);
        Light sun = RenderSettings.sun;
        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i].type == LightType.Directional && (sun == null || lights[i].intensity > sun.intensity))
                sun = lights[i];
        }

        Scene active = SceneManager.GetActiveScene();
        bool isGame = active.IsValid() && active.name.IndexOf("Game", StringComparison.OrdinalIgnoreCase) >= 0;

        if (sun != null)
        {
            RenderSettings.sun = sun;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 1.0f;
            sun.shadowResolution = LightShadowResolution.VeryHigh;
            sun.shadowBias = 0.035f;
            sun.shadowNormalBias = 0.24f;
            sun.useColorTemperature = true;
            sun.colorTemperature = 5250f;
            sun.intensity = Mathf.Max(sun.intensity, 1.65f);

            // The original directional light is close to flat fill. A fixed, lower-angle
            // key light makes the voxel relief and the new shadow-caster pass readable.
            if (isGame && !sunDirectionApplied)
            {
                sun.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
                sunDirectionApplied = true;
            }
        }

        if (isGame)
        {
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.34f, 0.46f, 0.58f, 1f);
            RenderSettings.ambientEquatorColor = new Color(0.20f, 0.24f, 0.27f, 1f);
            RenderSettings.ambientGroundColor = new Color(0.065f, 0.075f, 0.065f, 1f);
            RenderSettings.ambientIntensity = 0.38f;
            RenderSettings.reflectionIntensity = 0.58f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.0018f;
            RenderSettings.fogColor = new Color(0.44f, 0.56f, 0.67f, 1f);
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
        bloom.intensity.Override(0.58f);
        bloom.threshold.Override(0.82f);
        bloom.scatter.Override(0.78f);
        bloom.clamp.Override(24f);

        Tonemapping tonemapping = profile.Add<Tonemapping>(true);
        tonemapping.mode.Override(TonemappingMode.ACES);

        ColorAdjustments color = profile.Add<ColorAdjustments>(true);
        color.postExposure.Override(0.14f);
        color.contrast.Override(19f);
        color.saturation.Override(7f);
        color.colorFilter.Override(new Color(1.0f, 0.975f, 0.93f, 1f));

        WhiteBalance whiteBalance = profile.Add<WhiteBalance>(true);
        whiteBalance.temperature.Override(8f);
        whiteBalance.tint.Override(1f);

        Vignette vignette = profile.Add<Vignette>(true);
        vignette.intensity.Override(0.12f);
        vignette.smoothness.Override(0.58f);

        FilmGrain grain = profile.Add<FilmGrain>(true);
        grain.type.Override(FilmGrainLookup.Thin1);
        grain.intensity.Override(0.045f);
        grain.response.Override(0.70f);
    }
}
