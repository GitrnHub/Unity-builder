using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
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

        // Depth is required by the shader to distinguish open sky from occluders.
        // The pass itself requests an intermediate color target for the RenderGraph blit.
        renderPass.ConfigureInput(ScriptableRenderPassInput.Depth);
        renderer.EnqueuePass(renderPass);
    }

#if URP_COMPATIBILITY_MODE
#pragma warning disable 618, 672
    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        if (renderPass == null || material == null || renderingData.cameraData.cameraType != CameraType.Game)
            return;

        renderPass.SetCompatibilityTarget(renderer.cameraColorTargetHandle);
    }
#pragma warning restore 618, 672
#endif

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

#if URP_COMPATIBILITY_MODE
        private RTHandle cameraColorTarget;
        private RTHandle temporaryColor;
#endif

        public SunShaftsPass(Material material, SunShaftsSettings settings)
        {
            this.material = material;
            this.settings = settings;
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            requiresIntermediateTexture = true;
        }

        private bool UpdateMaterial(Camera camera)
        {
            if (material == null || camera == null)
                return false;

            Light sun = RenderSettings.sun;
            if (sun == null || sun.type != LightType.Directional)
            {
                material.SetFloat("_Intensity", 0f);
                return true;
            }

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
            return true;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (material == null)
                return;

            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            if (cameraData.camera == null || cameraData.camera.cameraType != CameraType.Game)
                return;

            if (resourceData.isActiveTargetBackBuffer)
            {
                Debug.LogWarning("AI Sun Shafts skipped because the active color target is the back buffer.");
                return;
            }

            if (!UpdateMaterial(cameraData.camera))
                return;

            TextureHandle source = resourceData.activeColorTexture;
            if (!source.IsValid())
                return;

            TextureDesc destinationDesc = renderGraph.GetTextureDesc(source);
            destinationDesc.name = "CameraColor-AI-SunShafts";
            destinationDesc.clearBuffer = false;
            TextureHandle destination = renderGraph.CreateTexture(destinationDesc);

            RenderGraphUtils.BlitMaterialParameters blitParameters =
                new RenderGraphUtils.BlitMaterialParameters(source, destination, material, 0);
            renderGraph.AddBlitPass(blitParameters, "AI Screen Space Sun Shafts");

            // Make the blitted texture the camera color for all following passes. This is
            // the Unity 6 / URP 17 RenderGraph pattern and avoids touching the now-removed
            // ScriptableRenderer camera target API.
            resourceData.cameraColor = destination;
        }

#if URP_COMPATIBILITY_MODE
#pragma warning disable 618, 672
        public void SetCompatibilityTarget(RTHandle colorHandle) => cameraColorTarget = colorHandle;

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            if (cameraColorTarget == null)
                return;

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
            if (cameraColorTarget == null || temporaryColor == null || !UpdateMaterial(renderingData.cameraData.camera))
                return;

            CommandBuffer cmd = CommandBufferPool.Get();
            Blitter.BlitCameraTexture(cmd, cameraColorTarget, temporaryColor, material, 0);
            Blitter.BlitCameraTexture(cmd, temporaryColor, cameraColorTarget);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
#pragma warning restore 618, 672
#endif

        public void Dispose()
        {
#if URP_COMPATIBILITY_MODE
            temporaryColor?.Release();
            temporaryColor = null;
#endif
        }
    }
}
