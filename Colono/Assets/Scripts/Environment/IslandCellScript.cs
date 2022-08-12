using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class IslandCellScript : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
{
    private enum SelectMode { None, Selecting, Building};
    public GameManager gameManagerScript;
    public IslandGenerator islandGeneratorScript;
    public IslandEditor islandEditorScript;
    public IslandScript islandScript;
    private Collider _collider;

    public MeshData meshData;
    public int[,] regionMap;
    private GameObject[,] cells;

    private Vector2 hoveredCell;
    private Vector2 selectedCell;
    private Vector2[] selectedCells;
    private bool wasOtherCellHovered = false;
    private SelectMode selectMode;
    private bool isSelectionValid = false;

    private GameObject building;
    private BuildingScript buildingScript;
    private int buildingOrientation = 0;
    private GameObject selectedZone;

    private void Start()
    {
        gameManagerScript = GameObject.Find("GameManager").GetComponent<GameManager>();
        islandGeneratorScript = GameObject.Find("GameManager").GetComponent<IslandGenerator>();
        islandEditorScript = GameObject.Find("GameManager").GetComponent<IslandEditor>();
        _collider = GetComponent<Collider>();
        cells = new GameObject[IslandGenerator.mapChunkSize, IslandGenerator.mapChunkSize];

        selectedCells = new Vector2[0];
    }

    private void Update()
    {
        if(selectMode == SelectMode.Building)
        {
            if (Input.GetKeyDown(KeyCode.Comma))
            {
                building.transform.Rotate(new Vector3(0, -90, 0));
                buildingOrientation -= 1;
                if (buildingOrientation == -1) buildingOrientation = 3;
                OnPointerMove(null);
            }
            else if (Input.GetKeyDown(KeyCode.Period))
            {
                building.transform.Rotate(new Vector3(0, 90, 0));
                buildingOrientation = (buildingOrientation + 1) % 4;
                OnPointerMove(null);
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        int x, y;
        GetPointerCoordinates(out x, out y);
        if (!gameManagerScript.isInIsland || x == -1 && y == -1)
        {
            return;
        }

        gameManagerScript.AreaUnselected();

        bool canChangeSelectedCell = false;
        /*if (selectedCells.Length == 0) canChangeSelectedCell = true; //No hi havia cap cel?la seleccionada
        else {
            if (!(selectedCell.x == x && selectedCell.y == y)) //La cel·la és diferent a la seleccionada
            {
                canChangeSelectedCell = true;
            }
            else //Es deselecciona la cel·la actual
            {
                cells[x, y].GetComponent<MeshRenderer>().material = islandEditorScript.selectingMaterial;
            }
        }*/
        canChangeSelectedCell = true;
        if (selectMode == SelectMode.Building)
        {
            Destroy(cells[(int)hoveredCell.x, (int)hoveredCell.y].gameObject);
            buildingOrientation = 0;
            if (!isSelectionValid)
            {
                Destroy(building);
            }

            InvertRegions(selectedCells, ZoneScript.ZoneType.Barn);
            canChangeSelectedCell = false;
        }

        for (int i = 0; i < selectedCells.Length; i++)
        {
            if (!(selectedCells[i].x == x && (int)selectedCells[i].y == y))
            {
                Destroy(cells[(int)selectedCells[i].x, (int)selectedCells[i].y].gameObject);
            }
        }

        if (canChangeSelectedCell)
        {

            if (regionMap[x, y] >= 0 && (regionMap[x, y] > 10 || islandGeneratorScript.regions[regionMap[x, y]].name == "Grass")) //Si la cel·la és gespa
            {
                try
                {
                    cells[x, y].GetComponent<MeshRenderer>().material = islandEditorScript.selectingMaterial;
                    selectMode = SelectMode.Selecting;
                    isSelectionValid = true;
                    selectedCell = new Vector2(x, y);
                    selectedCells = new Vector2[] { new Vector2(x, y) };
                }
                catch (Exception e)
                {
                    print(e.ToString());
                }

            }
            else
            {
                isSelectionValid = false;
            }
        }
        else
        {
            selectedCells = new Vector2[0];
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        int x, y;
        GetPointerCoordinates(out x, out y);
        if (!gameManagerScript.isInIsland || x == -1 && y == -1)
        {
            return;
        }

        if(selectMode == SelectMode.Selecting)
        {
            if (selectedCell.x == x && selectedCell.y == y)
            {
                cells[x, y].GetComponent<MeshRenderer>().material = islandEditorScript.selectedHoverMaterial;
                SingleSelection();
                /*cells[x, y].GetComponent<MeshRenderer>().material = islandEditorScript.hoverMaterial;
                selectedCells = new Vector2[0];*/
            }
            else
            {
                if (isSelectionValid)
                {
                    try
                    {
                        for (int i = 0; i < selectedCells.Length; i++)
                        {
                            cells[(int)selectedCells[i].x, (int)selectedCells[i].y].GetComponent<MeshRenderer>().material = islandEditorScript.selectedMaterial;
                        }
                    
                        MultipleSelection();
                    }
                    catch (Exception e)
                    {
                        print(e.ToString());
                    }
                }
                else
                {
                    for (int i = 0; i < selectedCells.Length; i++)
                    {
                        Destroy(cells[(int)selectedCells[i].x, (int)selectedCells[i].y].gameObject);
                    }
                    selectedCells = new Vector2[0];
                }
            }
        }

        selectMode = SelectMode.None;
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        int x, y;
        GetPointerCoordinates(out x, out y);
        if (!gameManagerScript.isInIsland || x == -1 && y == -1)
        {
            return;
        }

        bool canChangeHoveredCell = false;
        if (!wasOtherCellHovered) canChangeHoveredCell = true; //No hi havia cap cel·la hovered
        else if (!(hoveredCell.x == x && hoveredCell.y == y)) //La cel·la és diferent a la hovered
        {
            canChangeHoveredCell = true;
        }

        if (canChangeHoveredCell)
        {
            if(selectMode == SelectMode.Building)
            {
                hoveredCell = new Vector2(x, y);
                int vertexIndex = 0;

                int i = 0, j = 0;
                switch (buildingOrientation)
                {
                    case 0: i = 1;  j = 1; vertexIndex = 0; break;
                    case 1: i = -1; j = 1; vertexIndex = 1; break;
                    case 2: i = -1; j = -1; vertexIndex = 3; break;
                    case 3: i = 1; j = -1; vertexIndex = 2; break;
                }

                int endX = x + i * buildingScript.length;
                int endY = y + j * buildingScript.width;
                if (buildingOrientation % 2 != 0)
                {
                    endX = x + i * buildingScript.width;
                    endY = y + j * buildingScript.length;
                }

                foreach (Vector2 cell in selectedCells)
                {
                    Destroy(cells[(int)cell.x, (int)cell.y].gameObject);
                }

                int index = 0;
                selectedCells = new Vector2[buildingScript.width * buildingScript.length];
                isSelectionValid = true;
                for (int row = x; row != endX; row += i)
                {
                    for (int col = y; col != endY; col += j)
                    {
                        selectedCells[index] = new Vector2(row, col);
                        if (regionMap[row, col] < 0 || islandGeneratorScript.regions[regionMap[row, col]].name != "Grass")
                        {
                            isSelectionValid = false;
                        }
                        index++;
                    }
                }

                foreach (Vector2 cell in selectedCells)
                {
                    CreateCell(cell, isSelectionValid ? islandEditorScript.selectingMaterial : islandEditorScript.invalidSelectionMaterial);
                }

                Vector3 vertex = MeshGenerator.GetBuildingPosition(selectedCells, hoveredCell, vertexIndex, meshData);
                building.transform.position = transform.position + vertex;

            }
            else if (selectMode == SelectMode.Selecting)
            {
                for (int i = 0; i < selectedCells.Length; i++)
                {
                    Destroy(cells[(int)selectedCells[i].x, (int)selectedCells[i].y].gameObject);
                }

                hoveredCell = new Vector2(x, y);
                isSelectionValid = true;
                int numColumns = (int)Mathf.Abs(selectedCell.x - x);
                int numRows = (int)Mathf.Abs(selectedCell.y - y);
                selectedCells = new Vector2[(numColumns + 1) * (numRows + 1)];

                int i_init = (int)(selectedCell.x > x ? x : selectedCell.x);
                int i_fi = (int)(selectedCell.x > x ? selectedCell.x : x);
                int j_init = (int)(selectedCell.y > y ? y : selectedCell.y);
                int j_fi = (int)(selectedCell.y > y ? selectedCell.y : y);

                int index = 0;
                for (int i = i_init; i <= i_fi; i++)
                {
                    for (int j = j_init; j <= j_fi; j++)
                    {
                        selectedCells[index] = new Vector2(i, j);
                        if (regionMap[i, j] < 0 || (regionMap[i, j] < 10 && islandGeneratorScript.regions[regionMap[i, j]].name != "Grass"))
                        {
                            isSelectionValid = false;
                        }
                        index++;
                    }
                }

                foreach (Vector2 cell in selectedCells)
                {
                    CreateCell(cell, isSelectionValid ? islandEditorScript.selectingMaterial : islandEditorScript.invalidSelectionMaterial);
                }
                cells[(int)selectedCell.x, (int)selectedCell.y].GetComponent<MeshRenderer>().material = islandEditorScript.selectingFirstCellMaterial;
            }
            else
            {
                if (selectedCells.Contains(hoveredCell)) //La anterior cel·la hovered estava seleccionada
                {
                    try
                    {
                        cells[(int)hoveredCell.x, (int)hoveredCell.y].GetComponent<MeshRenderer>().material = islandEditorScript.selectedMaterial;
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e);
                    }
                }
                else
                {
                    if (wasOtherCellHovered) Destroy(cells[(int)hoveredCell.x, (int)hoveredCell.y].gameObject); //S'elimina l'anterior cel·la hovered
                }

                if (regionMap[x, y] >= 0 && (regionMap[x, y] > 10 || islandGeneratorScript.regions[regionMap[x, y]].name == "Grass")) //Si la cel·la és gespa
                {
                    hoveredCell = new Vector2(x, y);
                    if (selectedCells.Contains(new Vector2(x, y))) //La cel·la hovered està seleccionada
                    {
                        cells[(int)hoveredCell.x, (int)hoveredCell.y].GetComponent<MeshRenderer>().material = islandEditorScript.selectedHoverMaterial;
                    }
                    else
                    {
                        CreateCell(hoveredCell, islandEditorScript.hoverMaterial);
                    }
                    wasOtherCellHovered = true;
                }
                else
                {
                    wasOtherCellHovered = false;
                }
            }
        }
    }

    private void GetPointerCoordinates(out int x, out int y)
    {
        x = y = -1;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (_collider.Raycast(ray, out hit, 100))
        {
            Vector3 islandPoint = hit.point - transform.position;
            Vector2 mapPoint = new Vector2(IslandGenerator.mapChunkSize / 2 + islandPoint.x, IslandGenerator.mapChunkSize / 2 - islandPoint.z);
            x = Mathf.FloorToInt(mapPoint.x);
            y = Mathf.FloorToInt(mapPoint.y);
        }
    }

    private void CreateCell(Vector2 position, Material cellMaterial)
    {
        GameObject newCell = new GameObject("cell");
        MeshRenderer meshRenderer = newCell.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = newCell.AddComponent<MeshFilter>();

        meshRenderer.material = cellMaterial;

        MeshData cellMeshData = MeshGenerator.GenerateCell(position, meshData);
        Mesh mesh = cellMeshData.CreateMesh();
        meshFilter.mesh = mesh;

        newCell.transform.parent = islandScript.cells.transform;
        newCell.transform.localPosition = Vector3.zero;

        cells[(int)position.x, (int)position.y] = newCell;
    }

    //--------------------------------- ZONES ---------------------------------------

    private void SingleSelection()
    {
        if (regionMap[(int)selectedCell.x, (int)selectedCell.y] > 10) //Orchard
        {
            /*gameManagerScript.AreaUnselected();
            gameManagerScript.ZoneUnselected();*/
            selectedZone = islandScript.GetZoneByCell(selectedCell);
            gameManagerScript.PatchSelected(selectedZone.GetComponent<ZoneScript>().isPatchEmpty(selectedCells));
        }
        else
        {
            gameManagerScript.PatchUnselected();
        }
    }

    private void MultipleSelection()
    {
        if(regionMap[(int)selectedCell.x, (int)selectedCell.y] > 10) //Orchard
        {
            selectedZone = islandScript.GetZoneByCell(selectedCell);
            gameManagerScript.PatchSelected(selectedZone.GetComponent<ZoneScript>().isPatchEmpty(selectedCells));
        }
        else
        {
            gameManagerScript.PatchUnselected();
            bool isValidArea = selectedCells[0].x != selectedCells[selectedCells.Length - 1].x && selectedCells[0].y != selectedCells[selectedCells.Length - 1].y;
            if (isValidArea) gameManagerScript.AreaSelected();
        }
    }

    public void CreateOrchard()
    {
        CreateZone(ZoneScript.ZoneType.Orchard);
    }

    public void CreateBarn()
    {
        CreateZone(ZoneScript.ZoneType.Barn);
    }

    private void CreateZone(ZoneScript.ZoneType type)
    {
        GameObject zone = new GameObject("Zone");
        zone.transform.parent = islandScript.zones.transform;
        zone.transform.localPosition = Vector3.zero;
        ZoneScript zoneScript = zone.AddComponent<ZoneScript>();
        zoneScript.islandCellScript = this;
        zoneScript.cells = selectedCells;
        zoneScript.type = type;

        GameObject fences = new GameObject("Fences");
        fences.transform.parent = zone.transform;
        fences.transform.localPosition = Vector3.zero;
        Vector3[] positions;
        Quaternion[] rotations;
        MeshGenerator.GetFencePositions(selectedCells, meshData, out positions, out rotations);
        for (int i = 0; i < positions.Length - 1; i++)
        {
            GameObject fence = Instantiate(islandEditorScript.fences[UnityEngine.Random.Range(0, islandEditorScript.fences.Length)], transform.position + positions[i], rotations[i], fences.transform);
        }

        if (type == ZoneScript.ZoneType.Orchard)
        {
            GameObject post = Instantiate(islandEditorScript.post, transform.position + positions[positions.Length - 1], rotations[rotations.Length - 1], fences.transform);
            GameObject crops = new GameObject("Crops");
            crops.transform.parent = zone.transform;
            crops.transform.localPosition = Vector3.zero;
        }
        else
        {
            zoneScript.openGate = Instantiate(islandEditorScript.gateOpen, transform.position + positions[positions.Length - 1], rotations[rotations.Length - 1], fences.transform);
            zoneScript.openGate.SetActive(false);
            zoneScript.closedGate = Instantiate(islandEditorScript.gateClosed, transform.position + positions[positions.Length - 1], rotations[rotations.Length - 1], fences.transform);

            GameObject animals = new GameObject("Animals");
            animals.transform.parent = zone.transform;
            animals.transform.localPosition = Vector3.zero;
        }
        islandScript.AddZone(zone);

        InvertRegions(selectedCells, type);

        DestroyAllCells();
    }

    public void Plant()
    {
        ZoneScript zoneScript = selectedZone.GetComponent<ZoneScript>();
        Vector3[] positions;
        MeshGenerator.GetCropPositions(selectedCells, meshData, out positions);
        for (int i = 0; i < positions.Length; i++)
        {
            GameObject crop = Instantiate(islandEditorScript.tomato, transform.position + positions[i], islandEditorScript.tomato.transform.rotation, selectedZone.transform.GetChild(1));
            CropScript cropScript = crop.AddComponent<CropScript>();
            cropScript.cell = selectedCells[i];
            cropScript.type = CropScript.CropType.Tomato;
            zoneScript.AddCrop(selectedCells[i], cropScript);
        }

        DestroyAllCells();
    }

    public void ClearPatch()
    {
        ZoneScript zoneScript = selectedZone.GetComponent<ZoneScript>();
        foreach (Vector2 cell in selectedCells)
        {
            zoneScript.RemoveCrop(cell);
        }

        DestroyAllCells();
    }

    public void ZoneSelected(GameObject zone, ZoneScript.ZoneType type)
    {
        selectedZone = zone;
        gameManagerScript.ZoneSelected(type);
    }

    public void DeleteOrchard()
    {
        DeleteZone(ZoneScript.ZoneType.Orchard);
    }

    public void DeleteBarn()
    {
        DeleteZone(ZoneScript.ZoneType.Barn);
    }

    private void DeleteZone(ZoneScript.ZoneType type)
    {
        Vector2[] zoneCells = selectedZone.GetComponent<ZoneScript>().cells;
        islandScript.RemoveZone(selectedZone);
        Destroy(selectedZone);

        InvertRegions(zoneCells, type, true);
    }

    private void InvertRegions(Vector2[] cells, ZoneScript.ZoneType type, bool deletingZone = false)
    {
        int minX = (int)cells[0].x - 1, maxX = (int)cells[cells.Length - 1].x + 1;
        int minY = (int)cells[0].y - 1, maxY = (int)cells[cells.Length - 1].y + 1;

        if (type == ZoneScript.ZoneType.Orchard)
        {
            for (int i = minX; i <= maxX; i++)
            {
                regionMap[i, minY] = -regionMap[i, minY];
                regionMap[i, maxY] = -regionMap[i, maxY];
                for (int j = minY+1; j < maxY; j++)
                {
                    regionMap[i, j] = regionMap[i, j] + (deletingZone?-10:10);
                }
            }
            for (int j = minY + 1; j < maxY; j++)
            {
                regionMap[minX, j] = -regionMap[minX, j];
                regionMap[maxX, j] = -regionMap[maxX, j];
            }
        }
        else
        {
            for (int i = minX; i <= maxX; i++)
            {
                for (int j = minY; j <= maxY; j++)
                {
                    regionMap[i, j] = -regionMap[i, j];
                }
            }
        }
    }

    public void DestroyAllCells()
    {
        for (int i = 0; i < selectedCells.Length; i++)
        {
            Destroy(cells[(int)selectedCells[i].x, (int)selectedCells[i].y].gameObject);
        }
        selectedCells = new Vector2[0];

        Destroy(cells[(int)hoveredCell.x, (int)hoveredCell.y].gameObject);
    }

    // -------------------------------- BUILDINGS ------------------------------------

    public void SelectBuilding(BuildingScript.BuildingType type)
    {
        GameObject buildingPrefab;
        switch (type)
        {
            case BuildingScript.BuildingType.WoodHouse: buildingPrefab = islandEditorScript.woodHouse; break;
            default: buildingPrefab = islandEditorScript.stoneHouse; break;
        }

        building = Instantiate(buildingPrefab, transform.position, buildingPrefab.transform.rotation, islandScript.buildings.transform);
        buildingScript = building.GetComponent<BuildingScript>();

        Destroy(cells[(int)hoveredCell.x, (int)hoveredCell.y].gameObject);
        selectMode = SelectMode.Building;
        wasOtherCellHovered = false;
    }
}
