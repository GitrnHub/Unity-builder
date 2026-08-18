using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace VoxelGame.Core
{
    public class GameRenderSettings
    {
        public static void SetFog(float fogDistance, float fadeDistance)
        {
            RenderSettings.fogStartDistance = fogDistance;
            RenderSettings.fogEndDistance = fogDistance + fadeDistance;
        }

        public static void SetFrameRate(int targetFrameRate)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                Application.targetFrameRate = targetFrameRate;
            }
            else
            {
                Application.targetFrameRate = -1;
            }
        }

        public static void SetRenderScale(float renderScale)
        {
            if (renderScale < 0 || renderScale > 1)
                return;

            UniversalRenderPipelineAsset urpAsset = (UniversalRenderPipelineAsset)QualitySettings.renderPipeline;
            urpAsset.renderScale = renderScale;
        }
    }
}