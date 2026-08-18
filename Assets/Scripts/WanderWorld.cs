using UnityEngine;

public sealed class WanderWorld : MonoBehaviour
{
    private Material CreateMaterial(Color color)
    {
        Shader shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
        Material material = new Material(shader);
        material.color = color;
        return material;
    }

    private GameObject Box(string name, Vector3 position, Vector3 scale, Color color)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.position = position;
        go.transform.localScale = scale;
        Renderer renderer = go.GetComponent<Renderer>();
        renderer.sharedMaterial = CreateMaterial(color);
        return go;
    }

    private void Awake()
    {
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.58f, 0.68f, 0.78f);
        RenderSettings.fogDensity = 0.008f;
        RenderSettings.ambientLight = new Color(0.45f, 0.48f, 0.52f);

        Camera camera = Camera.main;
        if (camera != null)
        {
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.58f, 0.68f, 0.78f);
            camera.farClipPlane = 350f;
        }

        GameObject sun = new GameObject("Sun");
        Light light = sun.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.25f;
        light.color = new Color(1f, 0.93f, 0.82f);
        sun.transform.rotation = Quaternion.Euler(42f, -35f, 0f);

        Box("Ground", new Vector3(0f, -0.5f, 22f), new Vector3(80f, 1f, 100f), new Color(0.22f, 0.32f, 0.22f));
        Box("Road", new Vector3(0f, 0.03f, 24f), new Vector3(7f, 0.12f, 85f), new Color(0.22f, 0.23f, 0.25f));

        for (int i = 0; i < 9; i++)
        {
            float z = -4f + i * 10f;
            float hLeft = 2.5f + (i % 4) * 1.4f;
            float hRight = 3.2f + ((i + 2) % 5) * 1.1f;
            Box("LeftBuilding_" + i, new Vector3(-10f, hLeft * 0.5f, z), new Vector3(9f, hLeft, 7f), new Color(0.62f, 0.46f + i * 0.015f, 0.32f));
            Box("RightBuilding_" + i, new Vector3(10f, hRight * 0.5f, z + 4f), new Vector3(9f, hRight, 7f), new Color(0.34f, 0.48f, 0.62f - i * 0.015f));
        }

        for (int i = 0; i < 18; i++)
        {
            float z = -10f + i * 5f;
            Box("LampPostL_" + i, new Vector3(-4.7f, 1.5f, z), new Vector3(0.16f, 3f, 0.16f), new Color(0.12f, 0.12f, 0.12f));
            Box("LampPostR_" + i, new Vector3(4.7f, 1.5f, z + 2.5f), new Vector3(0.16f, 3f, 0.16f), new Color(0.12f, 0.12f, 0.12f));
        }

        Box("Plaza", new Vector3(0f, 0.12f, 70f), new Vector3(26f, 0.25f, 20f), new Color(0.55f, 0.55f, 0.50f));
        Box("MonumentBase", new Vector3(0f, 1f, 70f), new Vector3(5f, 2f, 5f), new Color(0.42f, 0.42f, 0.45f));
        Box("Monument", new Vector3(0f, 6f, 70f), new Vector3(2f, 10f, 2f), new Color(0.72f, 0.66f, 0.47f));

        Box("ArchLeft", new Vector3(-3.2f, 2.5f, 38f), new Vector3(1.2f, 5f, 1.2f), new Color(0.48f, 0.42f, 0.36f));
        Box("ArchRight", new Vector3(3.2f, 2.5f, 38f), new Vector3(1.2f, 5f, 1.2f), new Color(0.48f, 0.42f, 0.36f));
        Box("ArchTop", new Vector3(0f, 5.1f, 38f), new Vector3(7.6f, 1.1f, 1.2f), new Color(0.48f, 0.42f, 0.36f));
    }

    private void OnGUI()
    {
        GUI.Box(new Rect(18, 18, 470, 92), "Cloud Wander Demo\nWASD: move   Space/E: up   Ctrl/Q: down   Shift: boost\nHold RIGHT MOUSE and drag to look; arrow keys also look around.");
    }
}
