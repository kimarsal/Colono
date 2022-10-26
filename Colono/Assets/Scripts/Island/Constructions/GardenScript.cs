using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GardenScript : EnclosureScript
{
    public GameObject patches;
    public Dictionary<Vector2, PatchScript> patchesDictionary = new Dictionary<Vector2, PatchScript>();
    private int lastWorkedOnPatch = -1;
    //private int lastCropGathered = -1;
    private ResourceScript.CropType[] crops;

    private void Start()
    {
        crops = new ResourceScript.CropType[maxPeasants];
        int[] cropAmount = new int[System.Enum.GetNames(typeof(ResourceScript.CropType)).Length];
        int totalCropAmount = 0;

        for(int i = 0; i < cropAmount.Length; i++)
        {
            cropAmount[i] = islandScript.GetCropAmount((ResourceScript.CropType)i);
            totalCropAmount += cropAmount[i];
        }

        if(totalCropAmount > 0) //Si hi ha llavors disponibles
        {
            int usedAmount = 0;

            for (int i = 0; i < cropAmount.Length; i++)
            {
                cropAmount[i] = (int)Mathf.Floor(((float)cropAmount[i] / (float)totalCropAmount) * maxPeasants);
                usedAmount += cropAmount[i];
            }

            int index = 0, cropIndex = 0;
            while (cropIndex < cropAmount.Length && cropAmount[cropIndex] == 0) cropIndex++;
            if (cropIndex == cropAmount.Length) cropIndex = 0;
            cropAmount[cropIndex] += (maxPeasants - usedAmount);

            foreach (Vector2 cell in cells)
            {
                if (cell.x != cells[0].x && cell.x != cells[cells.Length - 1].x
                    && cell.y != cells[0].y && cell.y != cells[cells.Length - 1].y)
                {
                    CreatePatch(cell, (ResourceScript.CropType)cropIndex);
                    crops[index] = (ResourceScript.CropType)cropIndex;
                    cropAmount[cropIndex]--;
                    while (cropIndex < cropAmount.Length && cropAmount[cropIndex] == 0) cropIndex++;
                }
            }
        }
        else //Sinó hi ha llavors disponibles
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

        meshRenderer.material = islandScript.islandCellScript.islandEditorScript.patchMaterial;

        MeshData cellMeshData = MeshGenerator.GenerateCell(cell, 0.02f, islandScript.islandCellScript.meshData);
        Mesh mesh = cellMeshData.CreateMesh();
        meshFilter.mesh = mesh;

        patch.transform.parent = patches.transform;
        patch.transform.localPosition = Vector3.zero;

        PatchScript patchScript = patch.AddComponent<PatchScript>();
        patchScript.gardenScript = this;
        patchScript.islandEditor = islandScript.islandCellScript.islandEditorScript;
        patchScript.cell = cell;
        patchScript.cropType = cropType;
        patchScript.center = islandScript.transform.position + MeshGenerator.GetCellCenter(cell, islandScript.islandCellScript.meshData);

        //patchScript.PlantCrop(PatchScript.CropType.Potato);

        patchesDictionary.Add(cell, patchScript);
    }

    public void AddCrop(Vector2 cell, PatchScript crop)
    {
        RemoveCrop(cell);
        patchesDictionary.Add(cell, crop);
    }

    public void RemoveCrop(Vector2 cell)
    {
        if (patchesDictionary.ContainsKey(cell))
        {
            Destroy(patchesDictionary[cell].gameObject);
            patchesDictionary.Remove(cell);
        }
    }

    public bool ArePatchesEmpty(Vector2[] selectedCells)
    {
        foreach(Vector2 cell in selectedCells)
        {
            if (patchesDictionary.ContainsKey(cell)) return false;
        }
        return true;
    }

    public override TaskScript GetNextPendingTask()
    {
        PatchScript patch = null;
        lastWorkedOnPatch = (lastWorkedOnPatch + 1) % patchesDictionary.Count;

        int index = 0;
        foreach (KeyValuePair<Vector2, PatchScript> pair in patchesDictionary)
        {
            if (index < lastWorkedOnPatch)
            {
                if(patch == null && pair.Value.peasantScript == null
                    && !(pair.Value.cropState == PatchScript.CropState.Barren && islandScript.GetCropAmount(pair.Value.cropType) == 0))
                {
                    //lastWorkedOnPatch = index;
                    patch = pair.Value;
                }
            }
            else
            {
                if (pair.Value.peasantScript == null
                    && !(pair.Value.cropState == PatchScript.CropState.Barren && islandScript.GetCropAmount(pair.Value.cropType) == 0))
                {
                    //lastWorkedOnPatch = index;
                    return pair.Value;
                }
            }
            index++;
        }
        return patch;
    }
}
