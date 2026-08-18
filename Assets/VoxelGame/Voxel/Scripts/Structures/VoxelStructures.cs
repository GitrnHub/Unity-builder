using VoxelGame.Voxel;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelGame.Voxel
{
    public class VoxelStructures
    {
        public Dictionary<string, VoxelStructure> structures = new ();

        public VoxelStructures()
        {
            VoxelStructure testStructure = new VoxelStructure();
            testStructure.elements.Add(new VoxelStructureElement(new Vector3Int(0, 0, 0), Voxels.PLANK));
            testStructure.elements.Add(new VoxelStructureElement(new Vector3Int(0, 1, 0), Voxels.PLANK));
            testStructure.elements.Add(new VoxelStructureElement(new Vector3Int(0, 2, 0), Voxels.PLANK));
            testStructure.elements.Add(new VoxelStructureElement(new Vector3Int(0, 3, 0), Voxels.PLANK));
            testStructure.elements.Add(new VoxelStructureElement(new Vector3Int(0, 4, 0), Voxels.PLANK));

            testStructure.elements.Add(new VoxelStructureElement(new Vector3Int(0, 4, 1), Voxels.PLANK));
            testStructure.elements.Add(new VoxelStructureElement(new Vector3Int(0, 4, 2), Voxels.PLANK));
            testStructure.elements.Add(new VoxelStructureElement(new Vector3Int(0, 4, 3), Voxels.PLANK));
            testStructure.elements.Add(new VoxelStructureElement(new Vector3Int(0, 4, 4), Voxels.PLANK));

            testStructure.elements.Add(new VoxelStructureElement(new Vector3Int(0, 4, -1), Voxels.PLANK));
            testStructure.elements.Add(new VoxelStructureElement(new Vector3Int(0, 4, -2), Voxels.PLANK));
            testStructure.elements.Add(new VoxelStructureElement(new Vector3Int(0, 4, -3), Voxels.PLANK));
            testStructure.elements.Add(new VoxelStructureElement(new Vector3Int(0, 4, -4), Voxels.PLANK));

            testStructure.elements.Add(new VoxelStructureElement(new Vector3Int(1, 4, 0), Voxels.PLANK));
            testStructure.elements.Add(new VoxelStructureElement(new Vector3Int(2, 4, 0), Voxels.PLANK));
            testStructure.elements.Add(new VoxelStructureElement(new Vector3Int(3, 4, 0), Voxels.PLANK));
            testStructure.elements.Add(new VoxelStructureElement(new Vector3Int(4, 4, 0), Voxels.PLANK));

            testStructure.elements.Add(new VoxelStructureElement(new Vector3Int(-1, 4, 0), Voxels.PLANK));
            testStructure.elements.Add(new VoxelStructureElement(new Vector3Int(-2, 4, 0), Voxels.PLANK));
            testStructure.elements.Add(new VoxelStructureElement(new Vector3Int(-3, 4, 0), Voxels.PLANK));
            testStructure.elements.Add(new VoxelStructureElement(new Vector3Int(-4, 4, 0), Voxels.PLANK));

            RegistryStructure("test", testStructure);
        }

        public void RegistryStructure(string structureTag, VoxelStructure structure)
        {
            if (structures.ContainsKey(structureTag))
            {
                Debug.Log($"Structure {structureTag} already exist");
                return;
            }

            structures.Add(structureTag, structure);
        }

        public void RegistryStructure(VoxelStructureSO structureSO)
        {
            if (structures.ContainsKey(structureSO.structureTag))
            {
                Debug.Log($"Structure {structureSO.structureTag} already exist");
                return;
            }

            VoxelStructure structure = new VoxelStructure();
            foreach (var element in structureSO.structure.elements)
            {
                structure.elements.Add(new VoxelStructureElement(element.offset + structureSO.offset, element.voxel));
            }

            structures.Add(structureSO.structureTag, structure);
        }

        public VoxelStructure GetStructure(string structureTag)
        {
            if (structures.ContainsKey(structureTag))
            {
                return structures[structureTag];
            }

            Debug.Log($"Structure {structureTag} not found");
            return null;
        }
    }
}