using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public sealed class SunShaftsRendererFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public sealed class SunShaftsSettings
    {
        [Range(0f, 3f)] public float intensity = 1.15f;
        [Range(0f, 1.5f)] public float density = 0.82f;
        [Range(0.8f, 1f)] public float decay = 0.965f;
        [Range(0f, 0.2f)] public float weight = 0.055f;
        [Range(0f, 2f)] public float exposure = 0.72f;
    }

    public Shader shaftShader;
    public SunShaftsSettings settings = new SunShaftsSettings();

    private Material material;
    private SunShaftsPass renderPass;

    public override void Create()
    {
        CoreUtils.Destroy(material);
        material = shaftShader != null ? CoreUtils.CreateEngineMaterial(shaftShader) : null;
        renderPass = new SunShaftsPass(material, settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderPass == null || material == null || renderingData.cameraData.cameraType != CameraType.Game)
            return;

        renderer.EnqueuePass(renderPass);
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        if (renderPass == null || material == null || renderingData.cameraData.cameraType != CameraType.Game)
            return;

        // URP 14: requesting Color guarantees _CameraOpaqueTexture, and Depth guarantees
        // _CameraDepthTexture for occlusion of the shafts by terrain and trees.
        renderPass.ConfigureInput(ScriptableRenderPassInput.Color | ScriptableRenderPassInput.Depth);
        renderPass.SetTarget(renderer.cameraColorTargetHandle);
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(material);
        material = null;
    }

    private sealed class SunShaftsPass : ScriptableRenderPass
    {
        private readonly Material material;
        private readonly SunShaftsSettings settings;
        private RTHandle cameraColorTarget;

        public SunShaftsPass(Material material, SunShaftsSettings settings)
        {
            this.material = material;
            this.settings = settings;
            // ScriptableRenderPass already exposes profilingSampler. Assign the inherited
            // sampler instead of hiding it with another field.
            base.profilingSampler = new ProfilingSampler("AI Screen Space Sun Shafts");
            // Opaque color/depth are available here, while transparent objects can still
            // render on top of the atmospheric scattering afterwards.
            renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
        }

        public void SetTarget(RTHandle colorHandle)
        {
            cameraColorTarget = colorHandle;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            if (cameraColorTarget != null)
                ConfigureTarget(cameraColorTarget);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (material == null || cameraColorTarget == null)
                return;

            Camera camera = renderingData.cameraData.camera;
            Light sun = RenderSettings.sun;
            if (camera == null || sun == null || sun.type != LightType.Directional)
                return;

            Vector3 apparentSunWorld = camera.transform.position - sun.transform.forward * 10000f;
            Vector3 viewport = camera.WorldToViewportPoint(apparentSunWorld);

            float outsideX = Mathf.Max(0f, Mathf.Abs(viewport.x - 0.5f) - 0.65f);
            float outsideY = Mathf.Max(0f, Mathf.Abs(viewport.y - 0.5f) - 0.65f);
            float edgeVisibility = Mathf.Clamp01(1f - Mathf.Max(outsideX, outsideY) * 2.5f);
            float visibility = viewport.z > 0f ? edgeVisibility : 0f;

            Color sunColor = sun.color.linear * Mathf.Clamp(sun.intensity, 0.6f, 2.5f);
            material.SetVector("_SunViewport", new Vector4(viewport.x, viewport.y, viewport.z, 0f));
            material.SetColor("_SunColor", sunColor);
            material.SetFloat("_Intensity", settings.intensity * visibility);
            material.SetFloat("_Density", settings.density);
            material.SetFloat("_Decay", settings.decay);
            material.SetFloat("_Weight", settings.weight);
            material.SetFloat("_Exposure", settings.exposure);

            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, profilingSampler))
            {
                // This follows Unity's URP 14 Blitter pattern. The shader reads the
                // requested opaque/depth textures rather than sampling the target it writes.
                Blitter.BlitCameraTexture(cmd, cameraColorTarget, cameraColorTarget, material, 0);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
    }
}
