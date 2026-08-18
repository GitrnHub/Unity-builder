using UnityEditor;
using UnityEngine;

namespace VoxelGame.Voxel
{
    public class VoxelEditor : EditorWindow
    {
        private VoxelsPresetSO voxelsPreset;
        private Vector3Int structureCenterOffset;
        private VoxelStructureSO selectedStructure;
        private Vector3Int cubeStart;
        private Vector3Int cubeEnd;
        private bool enableMouseRaycast;
        private bool isFirstCubeSetup = false;
        private bool setStructureSizeMode = false;

        private byte currentVoxel = 1;

        [MenuItem("Voxel/Editor", false, 10)]
        public static void Open()
        {
            EditorWindow.GetWindow<VoxelEditor>("VoxelEditor", true, typeof(EditorWindow));
        }

        void OnEnable() => SceneView.duringSceneGui += OnSceneGUI;
        void OnDisable() => SceneView.duringSceneGui -= OnSceneGUI;

        void OnGUI()
        {
            UISaveStructure();

            UIVoxelsEdit();
        }

        private void UISaveStructure()
        {
            EditorGUILayout.LabelField("Save structure");

            selectedStructure = EditorGUILayout.ObjectField("", selectedStructure, typeof(VoxelStructureSO), true) as VoxelStructureSO;
            structureCenterOffset = EditorGUILayout.Vector3IntField("Structure offset", structureCenterOffset);

            cubeStart = EditorGUILayout.Vector3IntField("CubeStart", cubeStart);
            cubeEnd = EditorGUILayout.Vector3IntField("CubeEnd", cubeEnd);

            if (GUILayout.Button("Save", "Button"))
            {
                SaveStructureFile();
            }

            if (EditorApplication.isPlaying)
            {
                setStructureSizeMode = GUILayout.Toggle(setStructureSizeMode, "Set structure size", "Button");


                if (setStructureSizeMode)
                {
                    EditorGUILayout.LabelField("Click on voxel chunks to set saved structure size");

                    if (enableMouseRaycast)
                        enableMouseRaycast = false;
                }
            }
            else
            {
                setStructureSizeMode = false;
            }
        }

        private void UIVoxelsEdit()
        {
            EditorGUILayout.Space(32);
            EditorGUILayout.LabelField("Voxels edit");

            voxelsPreset = EditorGUILayout.ObjectField("", voxelsPreset, typeof(VoxelsPresetSO), true) as VoxelsPresetSO;

            if (voxelsPreset == null)
            {
                GUI.contentColor = Color.red;
                EditorGUILayout.LabelField("Voxels Preset not select");
                return;
            }

            if (EditorApplication.isPlaying)
            {
                enableMouseRaycast = GUILayout.Toggle(enableMouseRaycast, "EDIT", "Button");

                if (enableMouseRaycast)
                {
                    if (setStructureSizeMode)
                        setStructureSizeMode = false;

                    EditorGUILayout.LabelField("Select voxel and click on voxel chunks");

                    for (int i = 0; i < voxelsPreset.Voxels.Length; i++)
                    {
                        if (voxelsPreset.Voxels[i].name == "")
                            continue;

                        if (GUILayout.Toggle(currentVoxel == i, voxelsPreset.Voxels[i].name, "Button"))
                        {
                            currentVoxel = (byte)i;
                        }
                    }
                }
            }
            else
            {
                enableMouseRaycast = false;
                GUI.contentColor = Color.red;
                EditorGUILayout.LabelField("Enable Editor Play Mode");
                return;
            }
        }

        private void OnSceneGUI(SceneView view)
        {
            if (enableMouseRaycast)
            {
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                {
                    Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit))
                    {
                        Vector3Int voxelPosition = Vector3Int.FloorToInt(hit.point + hit.normal * 0.5f);

                        ChunkSystem chunkSystem = FindObjectOfType<ChunkSystem>();

                        if (chunkSystem != null)
                        {
                            chunkSystem.SetVoxel(Vector3Int.FloorToInt(hit.point - hit.normal * 0.5f * (currentVoxel == Voxels.AIR ? 1f : -1)), currentVoxel, true, true);
                        }
                        else
                        {
                            Debug.LogWarning("ChunkSystem not found in scene");
                        }
                    }
                    Event.current.Use();
                }
            }

            if (setStructureSizeMode)
            {
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                {
                    Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit))
                    {
                        Vector3Int voxelPosition = Vector3Int.FloorToInt(hit.point + hit.normal * 0.5f);

                        if (isFirstCubeSetup)
                        {
                            cubeStart = voxelPosition;
                            isFirstCubeSetup = false;
                        }
                        else
                        {
                            cubeEnd = voxelPosition;
                            isFirstCubeSetup = true;
                        }

                        Repaint();
                    }
                    Event.current.Use();
                }

                Vector3 offset = Vector3.one * 0.5f;

                Handles.color = Color.red;
                Handles.DrawWireCube(cubeStart + offset, Vector3.one);
                Handles.color = Color.magenta;
                Handles.DrawWireCube(cubeEnd + offset, Vector3.one);

                Vector3Int size = new Vector3Int(
                    Mathf.Abs(cubeEnd.x - cubeStart.x),
                    Mathf.Abs(cubeEnd.y - cubeStart.y),
                    Mathf.Abs(cubeEnd.z - cubeStart.z)
                    );

                if (size.x != 0 && size.y != 0 && size.z != 0)
                {
                    Vector3 center = (Vector3)(cubeStart + cubeEnd) / 2f;

                    Handles.color = Color.white;
                    Handles.DrawWireCube(center + offset, size + offset * 2f);
                }
            }
        }

        private void SaveStructureFile()
        {
            if (selectedStructure == null)
            {
                Debug.LogWarning("Target structure not selected");
                return;
            }

            ChunkSystem chunkSystem = FindObjectOfType<ChunkSystem>();
            if (chunkSystem == null)
                return;

            VoxelStructure structure = new VoxelStructure();

            structure.elements.Clear();

            for (int x = Mathf.Min(cubeStart.x, cubeEnd.x); x <= Mathf.Max(cubeStart.x, cubeEnd.x); x++)
                for (int y = Mathf.Min(cubeStart.y, cubeEnd.y); y <= Mathf.Max(cubeStart.y, cubeEnd.y); y++)
                    for (int z = Mathf.Min(cubeStart.z, cubeEnd.z); z <= Mathf.Max(cubeStart.z, cubeEnd.z); z++)
                    {
                        Vector3Int pos = new Vector3Int(x, y, z);
                        byte voxel = chunkSystem.GetVoxel(pos);

                        if (voxel != Voxels.AIR)
                        {
                            Vector3Int localPos = pos - cubeEnd + structureCenterOffset;
                            VoxelStructureElement voxelStructureElement = new VoxelStructureElement(localPos, voxel);
                            structure.elements.Add(voxelStructureElement);

                            Debug.Log("save voxel");
                        }
                    }


            selectedStructure.structure = structure;

            Debug.Log($"Structure {selectedStructure.structureTag} saved");
        }
    }
}
