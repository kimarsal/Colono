using System.Collections.Generic;
using UnityEngine;

public class GardenScript : EnclosureScript
{
    private Transform patchesTransform;
    public List<PatchScript> patchList = new List<PatchScript>();
    public Dictionary<Vector2, ResourceScript.CropType> cropDictionary = new Dictionary<Vector2, ResourceScript.CropType>();
    private int lastWorkedOnPatch = -1;

    private void Start()
    {
        if (patchList.Count == 0) InitializeGarden();
    }

    public void InitializeGarden(GardenInfo gardenInfo = null)
    {
        patchesTransform = new GameObject("Patches").transform;
        patchesTransform.parent = transform;
        patchesTransform.localPosition = Vector3.zero;

        if(gardenInfo == null)
        {
            CreateDefaultPatches();
        }
        else
        {
            foreach(PatchInfo patchInfo in gardenInfo.patchList)
            {
                CreatePatch(patchInfo.cell.UnityVector, patchInfo.cropType);
            }
            foreach(KeyValuePair<SerializableVector2, ResourceScript.CropType> pair in gardenInfo.cropDictionary)
            {
                cropDictionary[pair.Key.UnityVector] = pair.Value;
            }
            lastWorkedOnPatch = gardenInfo.lastWorkedOnPatch;
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

        meshRenderer.material = islandEditor.patchMaterial;

        MeshData cellMeshData = MeshGenerator.GenerateCell(cell, 0.01f, islandScript.meshData);
        Mesh mesh = cellMeshData.CreateMesh();
        meshFilter.mesh = mesh;

        patch.transform.parent = patchesTransform;
        patch.transform.localPosition = Vector3.zero;

        PatchScript patchScript = patch.AddComponent<PatchScript>();
        patchScript.gardenScript = this;
        patchScript.cell = cell;
        patchScript.cropType = cropType;
        patchScript.center = islandScript.transform.position + MeshGenerator.GetCellCenter(cell, islandScript.meshData);

        patchList.Add(patchScript);
        cropDictionary.Add(cell, cropType);
    }

    public override TaskScript GetNextPendingTask()
    {
        PatchScript nextPatch = null;
        lastWorkedOnPatch = (lastWorkedOnPatch + 1) % patchList.Count;

        int index = 0;
        foreach (PatchScript patchScript in patchList)
        {
            if (index < lastWorkedOnPatch)
            {
                if(nextPatch == null && patchScript.peasantScript == null
                    && !(patchScript.cropState == PatchScript.CropState.Barren && islandScript.GetResourceAmount(ResourceScript.ResourceType.Crop, (int)patchScript.cropType) == 0))
                {
                    //lastWorkedOnPatch = index;
                    nextPatch = patchScript;
                }
            }
            else
            {
                if (patchScript.peasantScript == null
                    && !(patchScript.cropState == PatchScript.CropState.Barren && islandScript.GetResourceAmount(ResourceScript.ResourceType.Crop, (int)patchScript.cropType) == 0))
                {
                    //lastWorkedOnPatch = index;
                    islandScript.UseResource(ResourceScript.ResourceType.Crop, (int)patchScript.cropType);
                    return patchScript;
                }
            }
            index++;
        }

        return nextPatch;
    }

    public override void FinishUpBusiness()
    {
        base.FinishUpBusiness();
        foreach (PatchScript patchScript in patchList)
        {
            if(patchScript.cropState != PatchScript.CropState.Barren)
            {
                islandScript.AddResource(ResourceScript.ResourceType.Crop, (int)patchScript.cropType);
            }
        }
    }

    public GardenInfo GetGardenInfo()
    {
        GardenInfo gardenInfo = new GardenInfo();
        foreach(PatchScript patchScript in patchList)
        {
            gardenInfo.patchList.Add(patchScript.GetPatchInfo());
        }
        foreach (KeyValuePair<Vector2, ResourceScript.CropType> pair in cropDictionary)
        {
            gardenInfo.cropDictionary.Add(new SerializableVector2(pair.Key), pair.Value);
        }
        gardenInfo.lastWorkedOnPatch = lastWorkedOnPatch;
        return gardenInfo;
    }

}

[System.Serializable]
public class GardenInfo : EnclosureInfo
{
    public List<PatchInfo> patchList = new List<PatchInfo>();
    public Dictionary<SerializableVector2, ResourceScript.CropType> cropDictionary = new Dictionary<SerializableVector2, ResourceScript.CropType>();
    public int lastWorkedOnPatch;
}