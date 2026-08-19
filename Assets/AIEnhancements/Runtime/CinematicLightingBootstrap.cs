using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public sealed class CinematicLightingBootstrap : MonoBehaviour
{
    private const string LightingRevision = "AI lighting v3 / Unity 6.3";
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
        Debug.Log($"[{LightingRevision}] bootstrap active; quality={QualitySettings.names[QualitySettings.GetQualityLevel()]}");
    }

    private void Start()
    {
        StartCoroutine(ApplyDelayed());
        InvokeRepeating(nameof(RefreshDynamicScene), 0.5f, 1.0f);
    }

    private void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Every Game scene owns a new directional light, so a previous scene must not
        // prevent the cinematic sun direction from being applied to the new light.
        sunDirectionApplied = false;
        SelectHighFidelityQuality();
        StartCoroutine(ApplyDelayed());
    }

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
        QualitySettings.shadowDistance = 220f;
        QualitySettings.shadowCascades = 4;
        QualitySettings.antiAliasing = 4;
    }

    private static void RefreshDynamicScene()
    {
        Camera[] cameras = FindObjectsOfType<Camera>(true);
        for (int i = 0; i < cameras.Length; i++)
        {
            Camera camera = cameras[i];
            camera.allowHDR = true;

            UniversalAdditionalCameraData data = camera.GetComponent<UniversalAdditionalCameraData>();
            if (data != null)
            {
                data.renderPostProcessing = true;
                data.requiresDepthTexture = true;
                data.requiresColorTexture = true;

                // The runtime Volume is created on the default layer. Some imported
                // camera assets carried a restrictive volume mask, which made the
                // Volume exist but silently prevented Bloom/ACES/grading from applying.
                data.volumeLayerMask = ~0;
                data.volumeTrigger = camera.transform;
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
            sun.shadowBias = 0.025f;
            sun.shadowNormalBias = 0.18f;
            sun.useColorTemperature = true;
            sun.colorTemperature = 5050f;
            sun.intensity = Mathf.Max(sun.intensity, 2.05f);

            // A lower key light produces long, readable voxel shadows instead of the
            // nearly flat top lighting of the original scene.
            if (isGame && !sunDirectionApplied)
            {
                sun.transform.rotation = Quaternion.Euler(55f, -38f, 0f);
                sunDirectionApplied = true;
            }
        }

        if (isGame)
        {
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.25f, 0.36f, 0.48f, 1f);
            RenderSettings.ambientEquatorColor = new Color(0.13f, 0.16f, 0.20f, 1f);
            RenderSettings.ambientGroundColor = new Color(0.035f, 0.045f, 0.04f, 1f);
            RenderSettings.ambientIntensity = 0.26f;
            RenderSettings.reflectionIntensity = 0.72f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.00145f;
            RenderSettings.fogColor = new Color(0.40f, 0.52f, 0.64f, 1f);
        }
    }

    private void CreatePostProcessing()
    {
        GameObject volumeObject = new GameObject("[AI] Global Cinematic Volume");
        volumeObject.layer = 0;
        volumeObject.transform.SetParent(transform, false);

        Volume volume = volumeObject.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 10000f;
        volume.weight = 1f;

        VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
        profile.name = "AI Cinematic Runtime Profile v3";
        volume.profile = profile;

        Bloom bloom = profile.Add<Bloom>(true);
        bloom.intensity.Override(0.92f);
        bloom.threshold.Override(0.72f);
        bloom.scatter.Override(0.82f);
        bloom.clamp.Override(32f);

        Tonemapping tonemapping = profile.Add<Tonemapping>(true);
        tonemapping.mode.Override(TonemappingMode.ACES);

        ColorAdjustments color = profile.Add<ColorAdjustments>(true);
        color.postExposure.Override(0.18f);
        color.contrast.Override(28f);
        color.saturation.Override(9f);
        color.colorFilter.Override(new Color(1.0f, 0.965f, 0.90f, 1f));

        WhiteBalance whiteBalance = profile.Add<WhiteBalance>(true);
        whiteBalance.temperature.Override(11f);
        whiteBalance.tint.Override(2f);

        Vignette vignette = profile.Add<Vignette>(true);
        vignette.intensity.Override(0.16f);
        vignette.smoothness.Override(0.62f);

        FilmGrain grain = profile.Add<FilmGrain>(true);
        grain.type.Override(FilmGrainLookup.Thin1);
        grain.intensity.Override(0.035f);
        grain.response.Override(0.72f);
    }
}
