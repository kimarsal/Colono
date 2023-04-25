using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GardenScript : EnclosureScript
{
    private Transform patchesTransform;
    [JsonProperty] [JsonConverter(typeof(VectorDictionaryConverter))] public Dictionary<Vector2, PatchScript> patchDictionary = new Dictionary<Vector2, PatchScript>();
    [JsonProperty] [JsonConverter(typeof(VectorDictionaryConverter))] public Dictionary<Vector2, ResourceScript.CropType> cropDictionary = new Dictionary<Vector2, ResourceScript.CropType>();
    [JsonProperty] private int lastWorkedOnPatch = -1;

    public override EditorScript editorScript { get { return CanvasScript.Instance.gardenEditor; } }

    public override void InitializeEnclosure(EnclosureScript enclosureScript, IslandScript islandScript)
    {
        base.InitializeEnclosure(enclosureScript, islandScript);

        patchesTransform = new GameObject("Patches").transform;
        patchesTransform.parent = transform;
        patchesTransform.localPosition = Vector3.zero;

        GardenScript gardenScript = (GardenScript)enclosureScript;

        if(gardenScript is null)
        {
            CreateDefaultPatches();
        }
        else
        {
            foreach(KeyValuePair<Vector2, PatchScript> pair in gardenScript.patchDictionary)
            {
                ResourceScript.CropType cropType = gardenScript.cropDictionary[pair.Key];
                CreatePatch(pair.Key, pair.Value.cropType);
                cropDictionary[pair.Key] = cropType;
            }
            lastWorkedOnPatch = gardenScript.lastWorkedOnPatch;
        }
        
    }

    private void CreateDefaultPatches()
    {
        int[] cropAmount = new int[System.Enum.GetNames(typeof(ResourceScript.CropType)).Length];
        int totalCropAmount = 0;

        for (int i = 0; i < cropAmount.Length; i++)
        {
            cropAmount[i] = islandScript.GetResourceAmount(ResourceScript.ResourceType.Crop, i);
            totalCropAmount += cropAmount[i];
        }

        if (totalCropAmount > 0) //Si hi ha llavors disponibles
        {
            int usedAmount = 0;

            for (int i = 0; i < cropAmount.Length; i++)
            {
                cropAmount[i] = (int)Mathf.Floor(((float)cropAmount[i] / (float)totalCropAmount) * maxPeasants);
                usedAmount += cropAmount[i];
            }

            int cropIndex = 0;
            while (cropIndex < cropAmount.Length && cropAmount[cropIndex] == 0) cropIndex++;
            if (cropIndex == cropAmount.Length) cropIndex = 0;
            cropAmount[cropIndex] += (maxPeasants - usedAmount);

            foreach (Vector2 cell in cells)
            {
                if (cell.x != cells[0].x && cell.x != cells[cells.Length - 1].x
                    && cell.y != cells[0].y && cell.y != cells[cells.Length - 1].y)
                {
                    CreatePatch(cell, (ResourceScript.CropType)cropIndex);

                    cropAmount[cropIndex]--;
                    while (cropIndex < cropAmount.Length && cropAmount[cropIndex] == 0) cropIndex++;
                }
            }
        }
        else //Si no hi ha llavors disponibles
        {
            foreach (Vector2 cell in cells)
            {
                if (cell.x != cells[0].x && cell.x != cells[cells.Length - 1].x
                    && cell.y != cells[0].y && cell.y != cells[cells.Length - 1].y)
                {
                    CreatePatch(cell, 0);
                }
            }
        }
    }

    private void CreatePatch(Vector2 cell, ResourceScript.CropType cropType)
    {
        GameObject patch = new GameObject("patch");
        MeshRenderer meshRenderer = patch.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = patch.AddComponent<MeshFilter>();

        meshRenderer.material = ResourceScript.Instance.patchMaterial;

        MeshData cellMeshData = MeshGenerator.GenerateCell(cell, 0.01f, islandScript.meshData);
        Mesh mesh = cellMeshData.CreateMesh();
        meshFilter.mesh = mesh;

        patch.transform.parent = patchesTransform;
        patch.transform.localPosition = Vector3.zero;

        PatchScript patchScript = patch.AddComponent<PatchScript>();
        patchScript.taskSourceScript = this;
        patchScript.cell = cell;
        patchScript.cropType = cropType;
        patchScript.center = islandScript.transform.position + MeshGenerator.GetCellCenter(cell, islandScript.meshData);

        patchDictionary.Add(cell, patchScript);
        cropDictionary.Add(cell, cropType);
    }

    public override bool GetNextPendingTask(PeasantAdultScript peasantAdultScript)
    {
        if (!peasantAdultScript.CanBeAsignedTask())
        {
            peasantAdultScript.UpdateTask();
            return false;
        }

        PatchScript nextPatchScript = null;
        int index = (lastWorkedOnPatch + 1) % patchDictionary.Count;
        while(index != lastWorkedOnPatch)
        {
            PatchScript patchScript = patchDictionary.ElementAt(index).Value;
            if (patchScript.peasantAdultScript == null
                && !(patchScript.cropState == PatchScript.CropState.Barren
                && islandScript.GetResourceAmount(ResourceScript.ResourceType.Crop, (int)patchScript.cropType) == 0))
            {
                if(patchScript.cropState == PatchScript.CropState.Barren)
                {
                    islandScript.UseResource(ResourceScript.ResourceType.Crop, (int)patchScript.cropType);
                }
                lastWorkedOnPatch = index;
                nextPatchScript = patchScript;
                break;
            }
            index = (index + 1) % patchDictionary.Count;
        }

        peasantAdultScript.AssignTask(nextPatchScript);
        return nextPatchScript != null;
    }

    public override PeasantScript RemovePeasant()
    {
        PeasantScript peasantScript = base.RemovePeasant();
        peasantScript.constructionScript = null;
        return peasantScript;
    }

    public int UseNewCrops(ResourceScript.CropType cropType, int remainingAmount)
    {
        int i = 0;
        while(i < peasantList.Count)
        {
            PeasantAdultScript peasantAdultScript = (PeasantAdultScript)peasantList[i];
            if (peasantAdultScript.CanBeAsignedTask())
            {
                foreach(KeyValuePair<Vector2, PatchScript> pair in patchDictionary)
                {
                    if(pair.Value.peasantAdultScript == null
                        && pair.Value.cropState == PatchScript.CropState.Barren
                        && pair.Value.cropType == cropType)
                    {
                        peasantAdultScript.AssignTask(pair.Value);
                        remainingAmount--;
                        if (remainingAmount == 0) return 0;
                        else break;
                    }
                }
                break;
            }
            i++;
        }
        return remainingAmount;
    }

    public override void FinishUpBusiness()
    {
        base.FinishUpBusiness();
        foreach (KeyValuePair<Vector2, PatchScript> pair in patchDictionary)
        {
            if(pair.Value.cropState != PatchScript.CropState.Barren)
            {
                islandScript.AddResource(ResourceScript.ResourceType.Crop, (int)pair.Value.cropType);
            }
        }
    }
}