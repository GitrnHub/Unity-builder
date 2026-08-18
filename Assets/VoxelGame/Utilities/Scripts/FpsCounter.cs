using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace VoxelGame.Utilities
{
    public class FpsCounter : MonoBehaviour
    {
        private TextMeshProUGUI text;
        private List<float> frameTimes;

        void Start()
        {
            text = GetComponent<TextMeshProUGUI>();
            frameTimes = new List<float>();
        }

        void Update()
        {
            float delta = Time.unscaledDeltaTime;

            if (delta > 0f)
            {
                frameTimes.Add(delta);

                if (frameTimes.Count > 0)
                {
                    float totalDeltaTime = 0f;
                    foreach (var frameTime in frameTimes)
                    {
                        totalDeltaTime += frameTime;
                    }

                    if (Time.frameCount % 8 == 0)
                    {
                        float fps = frameTimes.Count / totalDeltaTime;
                        text.text = $"fps: {fps}";
                    }

                    if (frameTimes.Count > 30)
                    {
                        frameTimes.RemoveAt(0);
                    }
                }
            }
        }
    }
}
