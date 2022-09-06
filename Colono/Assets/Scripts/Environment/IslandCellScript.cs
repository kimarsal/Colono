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
    private Vector2[] selectedCells = new Vector2[0];
    private bool wasOtherCellHovered = false;
    private SelectMode selectMode;
    private bool isSelectionValid = false;

    private GameObject selectedBuilding;
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
    }

    private void Update()
    {
        if(selectMode == SelectMode.Building) //Si està col·locant un edifici
        {
            if (Input.GetKeyDown(KeyCode.Comma))
            {
                selectedBuilding.transform.Rotate(new Vector3(0, -90, 0)); //Es rota en sentit antihorari
                buildingOrientation -= 1;
                if (buildingOrientation == -1) buildingOrientation = 3;
                OnPointerMove(null);
            }
            else if (Input.GetKeyDown(KeyCode.Period))
            {
                selectedBuilding.transform.Rotate(new Vector3(0, 90, 0)); //Es rota en sentit horari
                buildingOrientation = (buildingOrientation + 1) % 4;
                OnPointerMove(null);
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                selectMode = SelectMode.None;
                Destroy(selectedBuilding); //Eliminar l'edifici
                buildingOrientation = 0; //Resetejar l'orientació
                DestroyAllCells();
                gameManagerScript.IslandDefaultButtons();
            }
        }
        else if(selectMode == SelectMode.Selecting)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                selectMode = SelectMode.None;
                DestroyAllCells();
                wasOtherCellHovered = false;
                gameManagerScript.IslandDefaultButtons();
            }
        }
    }

    private void GetPointerCoordinates(out int x, out int y)
    {
        x = y = -1;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (_collider.Raycast(ray, out hit, 100)) //Si el ratolí és sobre l'illa
        {
            Vector3 islandPoint = hit.point - transform.position;
            Vector2 mapPoint = new Vector2(IslandGenerator.mapChunkSize / 2 + islandPoint.x, IslandGenerator.mapChunkSize / 2 - islandPoint.z);
            x = Mathf.FloorToInt(mapPoint.x);
            y = Mathf.FloorToInt(mapPoint.y);
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

        if (selectMode == SelectMode.Building) //Si intenta col·locar l'edifici
        {
            if (!isSelectionValid) //Si la posició no és vàlida
            {
                Destroy(selectedBuilding); //Eliminar l'edifici
            }
            else
            {
                buildingScript = selectedBuilding.GetComponent<BuildingScript>();
                buildingScript.islandCellScript = this;
                buildingScript.cells = selectedCells;
                buildingScript.orientation = buildingOrientation;
                buildingScript.EnableCollider();
                islandScript.AddBuilding(selectedBuilding); //Col·locar edifici

                InvertRegions(selectedCells, false);
            }
            buildingOrientation = 0; //Resetejar l'orientació
            DestroyAllCells();

            gameManagerScript.IslandDefaultButtons();
        }

        else
        {
            gameManagerScript.HideButtons();

            DestroyAllCells();
            isSelectionValid = false;
            selectMode = SelectMode.Selecting;

            if (selectedCell.x == x && selectedCell.y == y) //Es deselecciona la cel·la actual
            {
                CreateCell(new Vector2(x, y), islandEditorScript.selectingMaterial);
            }
            else
            {
                if (regionMap[x, y] >= 0 && (regionMap[x, y] > 10 //Si és terreny disponible
                    || islandGeneratorScript.regions[regionMap[x, y]].name == "Grass")) //Si la cel·la és gespa
                {
                    isSelectionValid = true;
                    selectedCell = new Vector2(x, y);
                    selectedCells = new Vector2[] { selectedCell };
                    CreateCell(selectedCell, islandEditorScript.selectingMaterial);
                }
            }
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
            if (isSelectionValid)
            {
                if(selectedCells.Length == 1)
                {
                    cells[x, y].GetComponent<MeshRenderer>().material = islandEditorScript.selectedHoverMaterial;
                    SingleSelection();
                }
                else
                {
                    for (int i = 0; i < selectedCells.Length; i++)
                    {
                        cells[(int)selectedCells[i].x, (int)selectedCells[i].y].GetComponent<MeshRenderer>().material = islandEditorScript.selectedMaterial;
                    }
                    cells[(int)hoveredCell.x, (int)hoveredCell.y].GetComponent<MeshRenderer>().material = islandEditorScript.selectedHoverMaterial;

                    MultipleSelection();
                }
            }
            else
            {
                DestroyAllCells();
                if (selectedCell.x == x && selectedCell.y == y) //Es deselecciona la cel·la actual
                {
                    CreateCell(selectedCell, islandEditorScript.hoverMaterial);
                }
                gameManagerScript.IslandDefaultButtons();
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

        if (!wasOtherCellHovered //No hi havia cap cel·la hovered
            || !(hoveredCell.x == x && hoveredCell.y == y)) //La cel·la és diferent a la hovered
        {
            if(selectMode == SelectMode.Building)
            {
                hoveredCell = new Vector2(x, y);
                int vertexIndex = 0; //vèrtex a partir del qual comença el requadre
                int i = 0, j = 0; //sentit del pas pels eixos x i y

                switch (buildingOrientation) //segons la orientació de l'edifici
                {
                    case 0: i = 1;  j = 1; vertexIndex = 0; break;
                    case 1: i = -1; j = 1; vertexIndex = 1; break;
                    case 2: i = -1; j = -1; vertexIndex = 3; break;
                    case 3: i = 1; j = -1; vertexIndex = 2; break;
                }

                int endX = x + i * buildingScript.length;
                int endY = y + j * buildingScript.width;
                if (buildingOrientation % 2 != 0) //la meitat de les orientacions tindran la llargada i l'amplada intercanviades
                {
                    endX = x + i * buildingScript.width;
                    endY = y + j * buildingScript.length;
                }

                foreach (Vector2 cell in selectedCells)
                {
                    Destroy(cells[(int)cell.x, (int)cell.y].gameObject);
                }

                //es guarden les noves cel·les seleccionades a la llista i es comprova quin color hauran de tenir
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

                //s'obté la posició de l'edifici segons el vèrtex inicial i l'alçada mitjana de la zona
                Vector3 vertex = MeshGenerator.GetBuildingPosition(selectedCells, hoveredCell, vertexIndex, meshData);
                selectedBuilding.transform.position = transform.position + vertex;

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

                /*try
                {*/
                    if (wasOtherCellHovered)
                    {
                        if (selectedCells.Contains(hoveredCell)) //L'anterior cel·la hovered estava seleccionada
                        {
                            cells[(int)hoveredCell.x, (int)hoveredCell.y].GetComponent<MeshRenderer>().material = islandEditorScript.selectedMaterial;
                        }
                        else
                        {
                            Destroy(cells[(int)hoveredCell.x, (int)hoveredCell.y].gameObject); //S'elimina l'anterior cel·la hovered
                        }
                    }

                    if (regionMap[x, y] >= 0 && (regionMap[x, y] > 10 || islandGeneratorScript.regions[regionMap[x, y]].name == "Grass")) //Si la cel·la és gespa
                    {
                        hoveredCell = new Vector2(x, y);
                        if (selectedCells.Contains(hoveredCell)) //La nova cel·la hovered està seleccionada
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
                /*}
                catch (Exception e)
                {
                    CreateCell(hoveredCell, islandEditorScript.hoverMaterial);
                    Debug.Log(e); //S'ha intentat accedir a una cel·la eliminada
                    Debug.Log("Error al intentar moure el ratolí");
                }*/
            }
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

    public void DestroyAllCells()
    {
        for (int i = 0; i < selectedCells.Length; i++)
        {
            Destroy(cells[(int)selectedCells[i].x, (int)selectedCells[i].y].gameObject);
        }
        selectedCells = new Vector2[0];

        Destroy(cells[(int)hoveredCell.x, (int)hoveredCell.y].gameObject);
    }

    //--------------------------------- ZONES ---------------------------------------

    private void SingleSelection()
    {
        if (regionMap[(int)selectedCell.x, (int)selectedCell.y] > 10) //Orchard
        {
            selectedZone = islandScript.GetZoneByCell(selectedCell);
            gameManagerScript.PatchSelected(selectedZone.GetComponent<ZoneScript>().isPatchEmpty(selectedCells));
        }
        else
        {
            gameManagerScript.IslandDefaultButtons();
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
            bool isValidArea = selectedCells[0].x != selectedCells[selectedCells.Length - 1].x && selectedCells[0].y != selectedCells[selectedCells.Length - 1].y;
            if (isValidArea) gameManagerScript.AreaSelected();
        }
    }

    public void CreateZone(ZoneScript.ZoneType type)
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

        InvertRegions(selectedCells, type == ZoneScript.ZoneType.Orchard);

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

    public void DeleteZone()
    {
        Vector2[] zoneCells = selectedZone.GetComponent<ZoneScript>().cells;
        InvertRegions(zoneCells, selectedZone.GetComponent<ZoneScript>().type == ZoneScript.ZoneType.Orchard, true);
        islandScript.RemoveZone(selectedZone);
        Destroy(selectedZone);
    }

    private void InvertRegions(Vector2[] cells, bool isOrchard, bool deletingZone = false)
    {
        int minX = (int)cells[0].x - 1, maxX = (int)cells[cells.Length - 1].x + 1;
        int minY = (int)cells[0].y - 1, maxY = (int)cells[cells.Length - 1].y + 1;

        if (isOrchard)
        {
            for (int i = minX + 1; i < maxX; i++)
            {
                regionMap[i, minY] = -regionMap[i, minY];
                regionMap[i, maxY] = -regionMap[i, maxY];
                for (int j = minY + 1; j < maxY; j++)
                {
                    regionMap[i, j] = regionMap[i, j] + (deletingZone?-10:10);
                }
            }
            for (int j = minY; j <= maxY; j++)
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

    // -------------------------------- BUILDINGS ------------------------------------

    public void ChooseBuilding(BuildingScript.BuildingType type)
    {
        GameObject buildingPrefab;
        switch (type)
        {
            case BuildingScript.BuildingType.WoodHouse: buildingPrefab = islandEditorScript.woodHouse; break;
            case BuildingScript.BuildingType.StoneHouse: buildingPrefab = islandEditorScript.stoneHouse; break;
            case BuildingScript.BuildingType.Mine: buildingPrefab = islandEditorScript.mine; break;
            default: buildingPrefab = null; break;
        }

        selectedBuilding = Instantiate(buildingPrefab, transform.position, buildingPrefab.transform.rotation, islandScript.buildings.transform);
        buildingScript = selectedBuilding.GetComponent<BuildingScript>();

        selectMode = SelectMode.Building;
        if (wasOtherCellHovered)
        {
            Destroy(cells[(int)hoveredCell.x, (int)hoveredCell.y].gameObject);
            wasOtherCellHovered = false;
        }
    }

    public void SelectBuilding(GameObject building, BuildingScript.BuildingType type)
    {
        selectedBuilding = building;
        gameManagerScript.SelectBuilding(type);
    }

    public void DeleteBuilding()
    {
        Vector2[] buildingCells = selectedBuilding.GetComponent<BuildingScript>().cells;
        islandScript.RemoveBuilding(selectedBuilding);
        Destroy(selectedBuilding);

        InvertRegions(buildingCells, false, true);
    }
}
