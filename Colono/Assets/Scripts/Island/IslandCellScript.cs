using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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
    private Vector2[] hoveredCells = new Vector2[0];
    private Vector2 selectedCell;
    private Vector2[] selectedCells = new Vector2[0];
    private bool wasOtherCellHovered = false;
    private SelectMode selectMode;
    private bool isSelectionValid = false;

    private GameObject selectedBuilding;
    private BuildingScript buildingScript;
    private int buildingOrientation = 0;
    private GameObject selectedEnclosure;
    private List<ItemScript> selectedItems;

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
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D) ||
            Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
        {
            OnPointerMove(null);
        }

        if (selectMode == SelectMode.Building) //Si està col·locant un edifici
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
                gameManagerScript.ShowButtons();
            }
        }
        else if(selectMode == SelectMode.Selecting)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                selectMode = SelectMode.None;
                DestroyAllCells();
                wasOtherCellHovered = false;
                gameManagerScript.ShowButtons();
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
                DestroyAllCells();
                gameManagerScript.ShowButtons();
            }
            else
            {
                ChangeSelectedBuildingColor(Color.white);

                buildingScript = selectedBuilding.GetComponent<BuildingScript>();
                buildingScript.cells = selectedCells;
                buildingScript.orientation = buildingOrientation;
                buildingScript.EnableCollider();

                islandScript.AddBuilding(selectedBuilding); //Col·locar edifici

                InvertRegions(selectedCells, false);
                foreach (Vector2 cell in selectedCells)
                {
                    cells[(int)cell.x, (int)cell.y].GetComponent<MeshRenderer>().material = islandEditorScript.selectedMaterial;
                }
                cells[x, y].GetComponent<MeshRenderer>().material = islandEditorScript.selectedHoverMaterial;

                gameManagerScript.SelectBuilding(buildingScript);
            }
            buildingOrientation = 0; //Resetejar l'orientació
        }

        else
        {
            if(hoveredCells.Length > 0)
            {
                wasOtherCellHovered = false;
                DestroyAllCells();

                bool isBuilding;
                GameObject construction = islandScript.GetConstructionByCell(hoveredCell, out isBuilding);
                selectedCells = hoveredCells;
                foreach (Vector2 cell in selectedCells)
                {
                    //CreateCell(cell, islandEditorScript.selectedMaterial);
                    cells[(int)cell.x, (int)cell.y].GetComponent<MeshRenderer>().material = islandEditorScript.selectedMaterial;
                }
                cells[x, y].GetComponent<MeshRenderer>().material = islandEditorScript.selectedHoverMaterial;
                hoveredCells = new Vector2[0];
                if (isBuilding)
                {
                    SelectBuilding(construction);
                }
                else
                {
                    SelectEnclosure(construction);
                }
            }
            else
            {
                DestroyAllCells();

                if (regionMap[x, y] >= 0 && (regionMap[x, y] > 10 //Si és terreny disponible
                    || islandGeneratorScript.regions[regionMap[x, y]].name.Contains("Grass"))) //Si la cel·la és gespa
                {
                    selectMode = SelectMode.Selecting;
                    gameManagerScript.HideButtons();

                    isSelectionValid = true;
                    selectedCell = new Vector2(x, y);
                    selectedCells = new Vector2[] { selectedCell };
                    CreateCell(selectedCell, islandEditorScript.selectingMaterial);
                }
                else
                {
                    isSelectionValid = false;
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
                if (selectedCells.Length == 1)
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
                selectMode = SelectMode.None;
                //OnPointerMove(null);
                gameManagerScript.ShowButtons();
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
                DestroyAllCells();

                hoveredCell = new Vector2(x, y);
                int vertexIndex = 0; //vèrtex a partir del qual comença el requadre
                int i = 0, j = 0; //sentit del pas pels eixos x i y

                switch (buildingOrientation) //segons la orientació de l'edifici
                {
                    case 0: i = -1; j = -1; vertexIndex = 3; break;
                    case 1: i = 1; j = -1; vertexIndex = 2; break;
                    case 2: i = 1;  j = 1; vertexIndex = 0; break;
                    case 3: i = -1; j = 1; vertexIndex = 1; break;
                }

                int endX = x + i * buildingScript.length;
                int endY = y + j * buildingScript.width;
                if (buildingOrientation % 2 != 0) //la meitat de les orientacions tindran la llargada i l'amplada intercanviades
                {
                    endX = x + i * buildingScript.width;
                    endY = y + j * buildingScript.length;
                }

                if (i == -1)
                {
                    int aux = x + 1; x = endX + 1; endX = aux;
                }
                if (j == -1)
                {
                    int aux = y + 1; y = endY + 1; endY = aux;
                }

                //es guarden les noves cel·les seleccionades a la llista i es comprova quin color hauran de tenir
                int index = 0;
                selectedCells = new Vector2[buildingScript.width * buildingScript.length];
                isSelectionValid = true;
                for (int col = x; col < endX; col++)
                {
                    for (int row = y; row < endY; row++)
                    {
                        selectedCells[index] = new Vector2(col, row);
                        if (regionMap[col, row] < 0 || !islandGeneratorScript.regions[regionMap[col, row]].name.Contains("Grass") || islandScript.isCellTaken(selectedCells[index]))
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
                ChangeSelectedBuildingColor(isSelectionValid ? Color.gray : Color.red);

            }
            else if (selectMode == SelectMode.Selecting)
            {
                DestroyAllCells();

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
                        if (regionMap[i, j] < 0 || (regionMap[i, j] < 10 && !islandGeneratorScript.regions[regionMap[i, j]].name.Contains("Grass")))
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
                if (wasOtherCellHovered)
                {
                    if (selectedCells.Contains(hoveredCell)) //L'anterior cel·la hovered estava seleccionada
                    {
                        cells[(int)hoveredCell.x, (int)hoveredCell.y].GetComponent<MeshRenderer>().material = islandEditorScript.selectedMaterial;
                    }
                    else if(hoveredCells.Length > 0)
                    {
                        if (!hoveredCells.Contains(new Vector2(x, y)))
                        {
                            for (int i = 0; i < hoveredCells.Length; i++)
                            {
                                Destroy(cells[(int)hoveredCells[i].x, (int)hoveredCells[i].y].gameObject);
                            }
                        }
                    }
                    else
                    {
                        Destroy(cells[(int)hoveredCell.x, (int)hoveredCell.y].gameObject); //S'elimina l'anterior cel·la hovered
                    }
                }

                hoveredCell = new Vector2(x, y);

                if (regionMap[x, y] < 0) //Si la cel·la està ocupada
                {
                    if (selectedCells.Contains(hoveredCell)) //La nova cel·la hovered està seleccionada
                    {
                        try
                        {
                            cells[x, y].GetComponent<MeshRenderer>().material = islandEditorScript.selectedHoverMaterial;

                        }
                        catch(Exception e)
                        {
                            Debug.Log(e);
                        }
                    }
                    else if (!hoveredCells.Contains(hoveredCell))
                    {
                        bool isBuilding;
                        GameObject construction = islandScript.GetConstructionByCell(hoveredCell, out isBuilding);
                        hoveredCells = construction.GetComponent<ConstructionScript>().cells;

                        foreach (Vector2 cell in hoveredCells)
                        {
                            CreateCell(cell, islandEditorScript.hoverMaterial);
                        }
                    }
                    wasOtherCellHovered = true;
                }
                else
                {
                    hoveredCells = new Vector2[0];
                    if (regionMap[x, y] > 10 || islandGeneratorScript.regions[regionMap[x, y]].name.Contains("Grass")) //Si la cel·la és gespa
                    {
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
                }
            }
        }
    }

    private void CreateCell(Vector2 position, Material cellMaterial)
    {
        GameObject newCell = new GameObject("cell");
        MeshRenderer meshRenderer = newCell.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = newCell.AddComponent<MeshFilter>();

        meshRenderer.material = cellMaterial;

        MeshData cellMeshData = MeshGenerator.GenerateCell(position, 0.1f, meshData);
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

        if (wasOtherCellHovered)
        {
            Destroy(cells[(int)hoveredCell.x, (int)hoveredCell.y].gameObject);
            wasOtherCellHovered = false;
        }
    }

    //--------------------------------- ENCLOSURES ---------------------------------------

    private void SingleSelection()
    {
        /*if (regionMap[(int)selectedCell.x, (int)selectedCell.y] > 10) //Garden
        {
            selectedEnclosure = islandScript.GetEnclosureByCell(selectedCell);
            gameManagerScript.PatchSelected(selectedEnclosure.GetComponent<GardenScript>().ArePatchesEmpty(selectedCells));
        }
        else
        {*/
        if (islandScript.isCellTaken(selectedCell))
            {
                ItemScript item = islandScript.GetItemByCell(selectedCell);
                selectedItems = new List<ItemScript>() { item };
                gameManagerScript.SelectItems(item.isScheduledForClearing);
            }
            else
            {
                gameManagerScript.ShowButtons();
            }
        //}
    }

    private void MultipleSelection()
    {
        /*if(regionMap[(int)selectedCell.x, (int)selectedCell.y] > 10) //Garden
        {
            selectedEnclosure = islandScript.GetEnclosureByCell(selectedCell);
            gameManagerScript.PatchSelected(selectedEnclosure.GetComponent<GardenScript>().ArePatchesEmpty(selectedCells));
        }
        else
        {*/
        bool areaContainsItems = false;
            bool areScheduledForClearing = false;
            selectedItems = new List<ItemScript>();
            foreach (Vector2 cell in selectedCells)
            {
                if (islandScript.isCellTaken(cell))
                {
                    areaContainsItems = true;
                    ItemScript item = islandScript.GetItemByCell(cell);
                    selectedItems.Add(item);
                    if (item.isScheduledForClearing) areScheduledForClearing = true;
                }
            }
            if (areaContainsItems)
            {
                gameManagerScript.SelectItems(areScheduledForClearing);
            }
            else
            {
                bool isValidArea = selectedCells[selectedCells.Length - 1].x - selectedCells[0].x >= 2 && selectedCells[selectedCells.Length - 1].y - selectedCells[0].y >= 2;
                if (isValidArea) gameManagerScript.SelectArea();
                else gameManagerScript.ShowButtons();
            }
        //}
    }

    public void ChangeSelectedItemsState(bool toClear)
    {
        foreach(ItemScript item in selectedItems)
        {
            if (item.ChangeItemClearingState(toClear)) //Ha canviat d'estat
            {
                if (toClear) islandScript.npcManager.AddItemToClear(item);
                else islandScript.npcManager.RemoveItemToClear(item);
            }
        }
    }

    public void CreateEnclosure(EnclosureScript.EnclosureType enclosureType)
    {
        GameObject enclosure = new GameObject("Enclosure");
        enclosure.transform.parent = islandScript.constructions.transform;
        enclosure.transform.localPosition = Vector3.zero;
        EnclosureScript enclosureScript = null;
        switch (enclosureType)
        {
            case EnclosureScript.EnclosureType.Garden: enclosureScript = enclosure.AddComponent<GardenScript>(); break;
            case EnclosureScript.EnclosureType.Barn: enclosureScript = enclosure.AddComponent<BarnScript>(); break;
            case EnclosureScript.EnclosureType.Training: enclosureScript = enclosure.AddComponent<TrainingScript>(); break;
        }
        enclosureScript.constructionType = ConstructionScript.ConstructionType.Enclosure;
        enclosureScript.cells = selectedCells;
        enclosureScript.enclosureType = enclosureType;
        enclosureScript.width = (int)selectedCells[selectedCells.Length - 1].x - (int)selectedCells[0].x;
        enclosureScript.length = (int)selectedCells[selectedCells.Length - 1].y - (int)selectedCells[0].y;
        Transform enclosureCenter = Instantiate(islandEditorScript.constructionCenterPrefab,
            transform.position + (MeshGenerator.GetCellCenter(selectedCells[0], meshData) + MeshGenerator.GetCellCenter(selectedCells[selectedCells.Length - 1], meshData)) / 2,
            Quaternion.Euler(Vector3.zero),
            enclosure.transform).transform;
        enclosureScript.center = enclosureCenter;
        enclosureScript.islandScript = islandScript;

        GameObject fences = new GameObject("Fences");
        fences.transform.parent = enclosure.transform;
        fences.transform.localPosition = Vector3.zero;
        Vector3[] positions;
        Quaternion[] rotations;
        MeshGenerator.GetFencePositions(selectedCells, meshData, out positions, out rotations);
        for (int i = 0; i < positions.Length - 1; i++)
        {
            GameObject fence = Instantiate(islandEditorScript.fences[UnityEngine.Random.Range(0, islandEditorScript.fences.Length)], transform.position + positions[i], rotations[i], fences.transform);
            if (i == 0) enclosureScript.minPos = fence.transform.localPosition;
            else if (i == positions.Length - 2) enclosureScript.maxPos = fence.transform.localPosition - new Vector3(0, 0, 1);
        }

        if (enclosureType == EnclosureScript.EnclosureType.Barn)
        {
            ((BarnScript)enclosureScript).openGate = Instantiate(islandEditorScript.gateOpen, transform.position + positions[positions.Length - 1], rotations[rotations.Length - 1], fences.transform);
            ((BarnScript)enclosureScript).openGate.SetActive(false);
            ((BarnScript)enclosureScript).closedGate = Instantiate(islandEditorScript.gateClosed, transform.position + positions[positions.Length - 1], rotations[rotations.Length - 1], fences.transform);

            ((BarnScript)enclosureScript).animals = new GameObject("Animals");
            ((BarnScript)enclosureScript).animals.transform.parent = enclosure.transform;
            ((BarnScript)enclosureScript).animals.transform.localPosition = Vector3.zero;
        }
        else
        {
            GameObject post = Instantiate(islandEditorScript.post, transform.position + positions[positions.Length - 1], rotations[rotations.Length - 1], fences.transform);
            if (enclosureType == EnclosureScript.EnclosureType.Garden)
            {
                ((GardenScript)enclosureScript).patches = new GameObject("Crops");
                ((GardenScript)enclosureScript).patches.transform.parent = enclosure.transform;
                ((GardenScript)enclosureScript).patches.transform.localPosition = Vector3.zero;
            }
        }

        BoxCollider boxCollider = enclosure.AddComponent<BoxCollider>();
        boxCollider.center = (enclosureScript.minPos + enclosureScript.maxPos) / 2;
        boxCollider.size = new Vector3(enclosureScript.maxPos.x - enclosureScript.minPos.x, 3, enclosureScript.minPos.z - enclosureScript.maxPos.z);
        boxCollider.isTrigger = true;

        enclosureScript.minPos += islandScript.transform.position;
        enclosureScript.maxPos += islandScript.transform.position;

        islandScript.AddEnclosure(enclosure);

        InvertRegions(selectedCells, enclosureType == EnclosureScript.EnclosureType.Garden);

        selectedEnclosure = enclosure;
        gameManagerScript.SelectEnclosure(enclosureScript);
    }

    /*public void Plant()
    {
        GardenScript gardenScript = selectedEnclosure.GetComponent<GardenScript>();
        Vector3[] positions;
        MeshGenerator.GetCropPositions(selectedCells, meshData, out positions);
        for (int i = 0; i < positions.Length; i++)
        {
            GameObject patch = Instantiate(islandEditorScript.tomato[1], transform.position + positions[i], islandEditorScript.tomato[1].transform.rotation, selectedEnclosure.transform.GetChild(1));
            PatchScript patchScript = patch.AddComponent<PatchScript>();
            patchScript.cell = selectedCells[i];
            patchScript.cropType = PatchScript.CropType.Tomato;
            gardenScript.AddCrop(selectedCells[i], patchScript);
        }

        DestroyAllCells();
    }

    public void ClearPatch()
    {
        GardenScript gardenScript = selectedEnclosure.GetComponent<GardenScript>();
        foreach (Vector2 cell in selectedCells)
        {
            gardenScript.RemoveCrop(cell);
        }

        DestroyAllCells();
    }*/

    public void SelectEnclosure(GameObject enclosure)
    {
        selectedEnclosure = enclosure;
        gameManagerScript.SelectEnclosure(enclosure.GetComponent<EnclosureScript>());
    }

    public void RemoveEnclosure()
    {
        Vector2[] enclosureCells = selectedEnclosure.GetComponent<EnclosureScript>().cells;
        InvertRegions(enclosureCells, selectedEnclosure.GetComponent<EnclosureScript>().enclosureType == EnclosureScript.EnclosureType.Garden, true);
        islandScript.RemoveEnclosure(selectedEnclosure);

        DestroyAllCells();
    }

    private void InvertRegions(Vector2[] cells, bool isGarden, bool deleting = false)
    {
        int minX = (int)cells[0].x, maxX = (int)cells[cells.Length - 1].x;
        int minY = (int)cells[0].y, maxY = (int)cells[cells.Length - 1].y;

        /*if (isGarden)
        {
            for (int i = minX + 1; i < maxX; i++)
            {
                regionMap[i, minY] = -regionMap[i, minY];
                regionMap[i, maxY] = -regionMap[i, maxY];
                for (int j = minY + 1; j < maxY; j++)
                {
                    regionMap[i, j] = regionMap[i, j] + (deleting?-10:10);
                }
            }
            for (int j = minY; j <= maxY; j++)
            {
                regionMap[minX, j] = -regionMap[minX, j];
                regionMap[maxX, j] = -regionMap[maxX, j];
            }
        }
        else
        {*/
        for (int i = minX; i <= maxX; i++)
            {
                for (int j = minY; j <= maxY; j++)
                {
                    regionMap[i, j] = -regionMap[i, j];
                }
            }
        //}
    }

    // -------------------------------- BUILDINGS ------------------------------------

    public void ChooseBuilding(BuildingScript.BuildingType type)
    {
        GameObject buildingPrefab;
        switch (type)
        {
            case BuildingScript.BuildingType.Warehouse: buildingPrefab = islandEditorScript.warehouse; break;
            case BuildingScript.BuildingType.Cabin: buildingPrefab = islandEditorScript.cabin; break;
            case BuildingScript.BuildingType.Tavern: buildingPrefab = islandEditorScript.tavern; break;
            case BuildingScript.BuildingType.Alchemist: buildingPrefab = islandEditorScript.alchemist; break;
            case BuildingScript.BuildingType.Mine: buildingPrefab = islandEditorScript.mine; break;
            default: buildingPrefab = null; break;
        }

        selectedBuilding = Instantiate(buildingPrefab, transform.position, buildingPrefab.transform.rotation, islandScript.constructions.transform);
        buildingScript = selectedBuilding.GetComponent<BuildingScript>();

        selectMode = SelectMode.Building;
        DestroyAllCells();
    }

    private void ChangeSelectedBuildingColor(Color color)
    {
        foreach (MeshRenderer buildingPieceRenderer in selectedBuilding.transform.GetChild(0).GetComponentsInChildren<MeshRenderer>())
        {
            buildingPieceRenderer.material.color = color;
        }
    }

    public void SelectBuilding(GameObject building)
    {
        selectedBuilding = building;
        gameManagerScript.SelectBuilding(building.GetComponent<BuildingScript>());
    }

    public void RemoveBuilding()
    {
        Vector2[] buildingCells = selectedBuilding.GetComponent<BuildingScript>().cells;
        islandScript.RemoveBuilding(selectedBuilding);

        InvertRegions(buildingCells, false, true);
        DestroyAllCells();
    }
}
