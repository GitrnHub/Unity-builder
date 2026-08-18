using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public sealed class GodRayShafts : MonoBehaviour
{
    private const int BeamCount = 10;
    private GameObject[] beams;
    private Material material;
    private Mesh beamMesh;
    private Camera targetCamera;
    private Light sun;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindObjectOfType<GodRayShafts>() != null) return;
        GameObject go = new GameObject("[AI] Volumetric God Rays");
        DontDestroyOnLoad(go);
        go.AddComponent<GodRayShafts>();
    }

    private void Start()
    {
        Shader shader = Resources.Load<Shader>("AIEnhancements/GodRayURP");
        if (shader == null)
        {
            Debug.LogWarning("[GodRayShafts] Shader missing; god rays disabled.");
            enabled = false;
            return;
        }

        material = new Material(shader);
        material.name = "AI God Ray Runtime Material";
        material.SetColor("_Color", new Color(1f, 0.86f, 0.58f, 0.07f));
        beamMesh = CreateCrossBeamMesh();
        beams = new GameObject[BeamCount];

        for (int i = 0; i < BeamCount; i++)
        {
            GameObject go = new GameObject("GodRay_" + i);
            go.transform.SetParent(transform, false);
            go.AddComponent<MeshFilter>().sharedMesh = beamMesh;
            MeshRenderer renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            beams[i] = go;
        }
    }

    private void LateUpdate()
    {
        if (beams == null) return;
        bool inGame = SceneManager.GetActiveScene().name.IndexOf("Game", System.StringComparison.OrdinalIgnoreCase) >= 0;
        if (!inGame)
        {
            SetActive(false);
            return;
        }

        if (targetCamera == null || !targetCamera.isActiveAndEnabled) targetCamera = Camera.main;
        if (targetCamera == null) targetCamera = FindObjectOfType<Camera>();
        if (sun == null || !sun.isActiveAndEnabled) sun = FindSun();
        if (targetCamera == null || sun == null)
        {
            SetActive(false);
            return;
        }

        SetActive(true);
        Quaternion rotation = Quaternion.FromToRotation(Vector3.down, -sun.transform.forward.normalized);
        Vector3 center = targetCamera.transform.position;

        for (int i = 0; i < beams.Length; i++)
        {
            float angle = i / (float)beams.Length * Mathf.PI * 2f + 0.37f;
            float radius = 12f + (i % 4) * 6.5f;
            float width = 2.6f + (i % 3) * 1.2f;
            float height = 28f + (i % 5) * 4f;
            beams[i].transform.position = center + new Vector3(Mathf.Cos(angle) * radius, height, Mathf.Sin(angle) * radius);
            beams[i].transform.rotation = rotation;
            beams[i].transform.localScale = new Vector3(width, 72f, width);
        }
    }

    private void SetActive(bool value)
    {
        for (int i = 0; i < beams.Length; i++)
            if (beams[i] != null && beams[i].activeSelf != value) beams[i].SetActive(value);
    }

    private static Light FindSun()
    {
        if (RenderSettings.sun != null) return RenderSettings.sun;
        Light[] lights = FindObjectsOfType<Light>(true);
        Light best = null;
        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i].type != LightType.Directional) continue;
            if (best == null || lights[i].intensity > best.intensity) best = lights[i];
        }
        return best;
    }

    private static Mesh CreateCrossBeamMesh()
    {
        Mesh mesh = new Mesh { name = "AI Cross Volumetric Beam" };
        mesh.vertices = new[]
        {
            new Vector3(-.22f,0,0), new Vector3(.22f,0,0), new Vector3(-.65f,-1,0), new Vector3(.65f,-1,0),
            new Vector3(0,0,-.22f), new Vector3(0,0,.22f), new Vector3(0,-1,-.65f), new Vector3(0,-1,.65f)
        };
        mesh.uv = new[]
        {
            new Vector2(0,0), new Vector2(1,0), new Vector2(0,1), new Vector2(1,1),
            new Vector2(0,0), new Vector2(1,0), new Vector2(0,1), new Vector2(1,1)
        };
        mesh.triangles = new[] { 0,2,1, 1,2,3, 4,6,5, 5,6,7 };
        mesh.RecalculateBounds();
        return mesh;
    }
}
