using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;

namespace VoxelGame.Utilities
{
    public class BatteryCounter : MonoBehaviour
    {
        private TextMeshProUGUI text;

        void Start()
        {
            text = GetComponent<TextMeshProUGUI>();
        }

        void Update()
        {
            if (Time.frameCount % 120 == 0)
            {
                text.text = $"{Mathf.FloorToInt(SystemInfo.batteryLevel * 100f)}%, {SystemInfo.batteryStatus}";
            }
        }
    }
}
