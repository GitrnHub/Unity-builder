using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public sealed class SunShaftsRendererFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public sealed class SunShaftsSettings
    {
        [Range(0f, 3f)] public float intensity = 1.45f;
        [Range(0f, 1.5f)] public float density = 0.90f;
        [Range(0.8f, 1f)] public float decay = 0.965f;
        [Range(0f, 0.2f)] public float weight = 0.070f;
        [Range(0f, 2f)] public float exposure = 0.86f;
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

        renderPass.ConfigureInput(ScriptableRenderPassInput.Color | ScriptableRenderPassInput.Depth);
        renderPass.SetTarget(renderer.cameraColorTargetHandle);
    }

    protected override void Dispose(bool disposing)
    {
        renderPass?.Dispose();
        renderPass = null;
        CoreUtils.Destroy(material);
        material = null;
    }

    private sealed class SunShaftsPass : ScriptableRenderPass
    {
        private readonly Material material;
        private readonly SunShaftsSettings settings;
        private RTHandle cameraColorTarget;
        private RTHandle temporaryColor;

        public SunShaftsPass(Material material, SunShaftsSettings settings)
        {
            this.material = material;
            this.settings = settings;
            base.profilingSampler = new ProfilingSampler("AI Screen Space Sun Shafts");
            renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
        }

        public void SetTarget(RTHandle colorHandle) => cameraColorTarget = colorHandle;

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            if (cameraColorTarget == null) return;

            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;
            descriptor.msaaSamples = 1;
            RenderingUtils.ReAllocateIfNeeded(
                ref temporaryColor,
                descriptor,
                FilterMode.Bilinear,
                TextureWrapMode.Clamp,
                name: "_AISunShaftsTemp");
            ConfigureTarget(cameraColorTarget);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (material == null || cameraColorTarget == null || temporaryColor == null)
                return;

            Camera camera = renderingData.cameraData.camera;
            Light sun = RenderSettings.sun;
            if (camera == null || sun == null || sun.type != LightType.Directional)
                return;

            Vector3 apparentSunWorld = camera.transform.position - sun.transform.forward * 10000f;
            Vector3 viewport = camera.WorldToViewportPoint(apparentSunWorld);

            float outsideX = Mathf.Max(0f, Mathf.Abs(viewport.x - 0.5f) - 0.72f);
            float outsideY = Mathf.Max(0f, Mathf.Abs(viewport.y - 0.5f) - 0.72f);
            float edgeVisibility = Mathf.Clamp01(1f - Mathf.Max(outsideX, outsideY) * 2.2f);
            float visibility = viewport.z > 0f ? edgeVisibility : 0f;

            Color sunColor = sun.color.linear * Mathf.Clamp(sun.intensity, 0.7f, 3.0f);
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
                // Avoid an in-place source/destination blit. The effect samples the opaque
                // color and depth textures, writes a temporary target, then composites it
                // back to the camera color buffer.
                Blitter.BlitCameraTexture(cmd, cameraColorTarget, temporaryColor, material, 0);
                Blitter.BlitCameraTexture(cmd, temporaryColor, cameraColorTarget);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        public void Dispose()
        {
            temporaryColor?.Release();
            temporaryColor = null;
        }
    }
}
