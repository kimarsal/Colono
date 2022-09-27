using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GardenScript : EnclosureScript
{
    public GameObject patches;
    public Dictionary<Vector2, PatchScript> patchesDictionary = new Dictionary<Vector2, PatchScript>();

    private void Start()
    {
        foreach(Vector2 cell in cells)
        {
            if(cell.x != cells[0].x && cell.x != cells[cells.Length-1].x
                && cell.y != cells[0].y && cell.y != cells[cells.Length - 1].y)
            {
                CreatePatch(cell, PatchScript.CropType.Tomato);
            }
        }
    }

    private void CreatePatch(Vector2 cell, PatchScript.CropType cropType)
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
        patchScript.islandEditor = islandScript.islandCellScript.islandEditorScript;
        patchScript.cell = cell;
        patchScript.cropType = cropType;
        patchScript.center = islandScript.transform.position + MeshGenerator.GetCellCenter(cell, islandScript.islandCellScript.meshData);

        patchScript.PlantCrop(PatchScript.CropType.Potato);

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

    public bool isPatchEmpty(Vector2[] selectedCells)
    {
        foreach(Vector2 cell in selectedCells)
        {
            if (patchesDictionary.ContainsKey(cell)) return false;
        }
        return true;
    }
}
