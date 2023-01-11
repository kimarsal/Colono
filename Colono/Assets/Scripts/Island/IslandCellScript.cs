using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class IslandCellScript : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
{
    private enum SelectMode { None, Selecting, Building};
    public GameManager gameManager;
    public IslandGenerator islandGenerator;
    public IslandEditor islandEditor;
    public IslandScript islandScript;

    private GameObject[,] cells;

    private Vector2 hoveredCell;
    private Vector2[] hoveredCells = new Vector2[0];
    private Vector2 selectedCell;
    private Vector2[] selectedCells = new Vector2[0];
    private bool wasOtherCellHovered = false;
    private SelectMode selectMode;
    private bool isSelectionValid = false;
    private Vector2 lastMousePosition;

    private BuildingScript selectedBuilding;
    private int buildingOrientation = 0;
    private List<ItemScript> selectedItems;

    private void Start()
    {
        gameManager = GetComponent<GameManager>();
        islandGenerator = GetComponent<IslandGenerator>();
        islandEditor = GetComponent<IslandEditor>();
        cells = new GameObject[IslandGenerator.mapChunkSize, IslandGenerator.mapChunkSize];
    }

    private void Update()
    {
        if (gameManager.CanSelect() && (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0))
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
                Destroy(selectedBuilding.gameObject); //Eliminar l'edifici
                buildingOrientation = 0; //Resetejar l'orientació
                DestroyAllCells();
                gameManager.canvasScript.ShowButtons();
            }
        }
        else if(selectMode == SelectMode.Selecting)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                selectMode = SelectMode.None;
                DestroyAllCells();
                wasOtherCellHovered = false;
                gameManager.canvasScript.ShowButtons();
            }
        }
    }

    private void GetPointerCoordinates(PointerEventData eventData, out int x, out int y)
    {
        Vector3 islandPoint = eventData.pointerCurrentRaycast.worldPosition - islandScript.transform.position;
        Vector2 mapPoint = new Vector2(IslandGenerator.mapChunkSize / 2 + islandPoint.x, IslandGenerator.mapChunkSize / 2 - islandPoint.z);
        x = Mathf.FloorToInt(mapPoint.x);
        y = Mathf.FloorToInt(mapPoint.y);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (gameManager.hasSelectedPeasant) //Evitar confusions al seleccionar un NPC
        {
            gameManager.hasSelectedPeasant = false;
            return;
        }

        int x, y;
        GetPointerCoordinates(eventData, out x, out y);

        if (selectMode == SelectMode.Building) //Si intenta col·locar l'edifici
        {
            if (!isSelectionValid) //Si la posició no és vàlida
            {
                Destroy(selectedBuilding.gameObject); //Eliminar l'edifici
                gameManager.canvasScript.ShowButtons();
            }
            else
            {
                ChangeSelectedBuildingColor(Color.white);
                selectedBuilding.islandScript = islandScript;
                selectedBuilding.islandEditor = islandEditor;
                selectedBuilding.cells = selectedCells;
                selectedBuilding.orientation = buildingOrientation;
                selectedBuilding.outline = selectedBuilding.AddComponent<Outline>();
                islandScript.AddConstruction(selectedBuilding); //Col·locar edifici

                InvertRegions(selectedCells);

                gameManager.SelectConstruction(selectedBuilding);
            }
            DestroyAllCells();
            buildingOrientation = 0; //Resetejar l'orientació
        }

        else
        {
            if(hoveredCells.Length > 0)
            {
                wasOtherCellHovered = false;
                DestroyAllCells();

                ConstructionScript constructionScript = islandScript.GetConstructionByCell(hoveredCell);
                hoveredCells = new Vector2[0];
                gameManager.SelectConstruction(constructionScript);
            }
            else
            {
                DestroyAllCells();

                if (islandScript.regionMap[x, y] >= 0 && (islandScript.regionMap[x, y] > 10 //Si és terreny disponible
                    || islandGenerator.regions[islandScript.regionMap[x, y]].name.Contains("Grass"))) //Si la cel·la és gespa
                {
                    selectMode = SelectMode.Selecting;
                    gameManager.canvasScript.HideButtons();

                    isSelectionValid = true;
                    selectedCell = new Vector2(x, y);
                    selectedCells = new Vector2[] { selectedCell };
                    CreateCell(selectedCell, islandEditor.selectingMaterial);
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
        GetPointerCoordinates(eventData, out x, out y);

        if (selectMode == SelectMode.Selecting)
        {
            if (isSelectionValid)
            {
                if (selectedCells.Length == 1)
                {
                    cells[x, y].GetComponent<MeshRenderer>().material = islandEditor.selectedHoverMaterial;
                    SingleSelection();
                }
                else if (selectedCells.Length > 1)
                {
                    for (int i = 0; i < selectedCells.Length; i++)
                    {
                        cells[(int)selectedCells[i].x, (int)selectedCells[i].y].GetComponent<MeshRenderer>().material = islandEditor.selectedMaterial;
                    }
                    cells[(int)hoveredCell.x, (int)hoveredCell.y].GetComponent<MeshRenderer>().material = islandEditor.selectedHoverMaterial;

                    MultipleSelection();
                }
            }
            else
            {
                DestroyAllCells();
                selectMode = SelectMode.None;
                //OnPointerMove(null);
                gameManager.canvasScript.ShowButtons();
            }
        }

        selectMode = SelectMode.None;
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        int x, y;
        if (eventData == null)
        {
            x = (int)lastMousePosition.x;
            y = (int)lastMousePosition.y;
        }
        else
        {
            GetPointerCoordinates(eventData, out x, out y);
            lastMousePosition = new Vector2(x, y);
        }
        if (wasOtherCellHovered && hoveredCell.x == x && hoveredCell.y == y) return; //La cel·la és la mateixa a la hovered
        
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

            int endX = x + i * selectedBuilding.length;
            int endY = y + j * selectedBuilding.width;
            if (buildingOrientation % 2 != 0) //la meitat de les orientacions tindran la llargada i l'amplada intercanviades
            {
                endX = x + i * selectedBuilding.width;
                endY = y + j * selectedBuilding.length;
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
            selectedCells = new Vector2[selectedBuilding.width * selectedBuilding.length];
            isSelectionValid = true;
            for (int col = x; col < endX; col++)
            {
                for (int row = y; row < endY; row++)
                {
                    selectedCells[index] = new Vector2(col, row);
                    if (islandScript.regionMap[col, row] < 0 || !islandGenerator.regions[islandScript.regionMap[col, row]].name.Contains("Grass") || islandScript.isCellTaken(selectedCells[index]))
                    {
                        isSelectionValid = false;
                    }
                    index++;
                }
            }

            foreach (Vector2 cell in selectedCells)
            {
                CreateCell(cell, isSelectionValid ? islandEditor.selectingMaterial : islandEditor.invalidSelectionMaterial);
            }

            //s'obté la posició de l'edifici segons el vèrtex inicial i l'alçada mitjana de la zona
            Vector3 vertex = MeshGenerator.GetBuildingPosition(selectedCells, hoveredCell, vertexIndex, islandScript.meshData);
            selectedBuilding.transform.position = islandScript.transform.position + vertex;
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

            for (int j = j_init; j <= j_fi; j++)
            {
                for (int i = i_init; i <= i_fi; i++)
                {
                    selectedCells[index] = new Vector2(i, j);
                    if (islandScript.regionMap[i, j] < 0 || (islandScript.regionMap[i, j] < 10 && !islandGenerator.regions[islandScript.regionMap[i, j]].name.Contains("Grass")))
                    {
                        isSelectionValid = false;
                    }
                    index++;
                }
            }

            foreach (Vector2 cell in selectedCells)
            {
                CreateCell(cell, isSelectionValid ? islandEditor.selectingMaterial : islandEditor.invalidSelectionMaterial);
            }
            cells[(int)selectedCell.x, (int)selectedCell.y].GetComponent<MeshRenderer>().material = islandEditor.selectingFirstCellMaterial;
        }
        else
        {
            if (wasOtherCellHovered)
            {
                if (selectedCells.Contains(hoveredCell)) //L'anterior cel·la hovered estava seleccionada
                {
                    cells[(int)hoveredCell.x, (int)hoveredCell.y].GetComponent<MeshRenderer>().material = islandEditor.selectedMaterial;
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

            if (islandScript.regionMap[x, y] < 0) //Si la cel·la està ocupada
            {
                if (selectedCells.Contains(hoveredCell)) //La nova cel·la hovered està seleccionada
                {
                    try
                    {
                        cells[x, y].GetComponent<MeshRenderer>().material = islandEditor.selectedHoverMaterial;

                    }
                    catch(Exception e)
                    {
                        Debug.Log(e);
                    }
                }
                else if (!hoveredCells.Contains(hoveredCell))
                {
                    hoveredCells = islandScript.GetConstructionByCell(hoveredCell).cells;

                    foreach (Vector2 cell in hoveredCells)
                    {
                        CreateCell(cell, islandEditor.hoverMaterial);
                    }
                }
                wasOtherCellHovered = true;
            }
            else
            {
                hoveredCells = new Vector2[0];
                if (islandScript.regionMap[x, y] > 10 || islandGenerator.regions[islandScript.regionMap[x, y]].name.Contains("Grass")) //Si la cel·la és gespa
                {
                    if (selectedCells.Contains(hoveredCell)) //La nova cel·la hovered està seleccionada
                    {
                        cells[(int)hoveredCell.x, (int)hoveredCell.y].GetComponent<MeshRenderer>().material = islandEditor.selectedHoverMaterial;
                    }
                    else
                    {
                        CreateCell(hoveredCell, islandEditor.hoverMaterial);
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

    private void CreateCell(Vector2 position, Material cellMaterial)
    {
        GameObject newCell = new GameObject("cell");
        MeshRenderer meshRenderer = newCell.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = newCell.AddComponent<MeshFilter>();

        meshRenderer.material = cellMaterial;

        MeshData cellMeshData = MeshGenerator.GenerateCell(position, 0.02f, islandScript.meshData);
        Mesh mesh = cellMeshData.CreateMesh();
        meshFilter.mesh = mesh;

        newCell.transform.parent = gameManager.cellsTransform;
        newCell.transform.position = islandScript.transform.position;

        cells[(int)position.x, (int)position.y] = newCell;
    }

    public void DestroyAllCells()
    {
        for (int i = 0; i < selectedCells.Length; i++)
        {
            Destroy(cells[(int)selectedCells[i].x, (int)selectedCells[i].y].gameObject);
        }
        selectedCells = new Vector2[0];

        for (int i = 0; i < hoveredCells.Length; i++)
        {
            Destroy(cells[(int)hoveredCells[i].x, (int)hoveredCells[i].y].gameObject);
        }
        hoveredCells = new Vector2[0];

        if (wasOtherCellHovered)
        {
            Destroy(cells[(int)hoveredCell.x, (int)hoveredCell.y].gameObject);
            wasOtherCellHovered = false;
        }
    }

    //--------------------------------- ENCLOSURES ---------------------------------------

    private void SingleSelection()
    {
        if (islandScript.isCellTaken(selectedCell))
        {
            ItemScript item = islandScript.GetItemByCell(selectedCell);
            selectedItems = new List<ItemScript>() { item };
            gameManager.canvasScript.SelectItems(item.isScheduledForClearing);
        }
        else
        {
            gameManager.canvasScript.ShowButtons();
        }
    }

    private void MultipleSelection()
    {
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
            gameManager.canvasScript.SelectItems(areScheduledForClearing);
        }
        else
        {
            bool isValidArea = selectedCells[selectedCells.Length - 1].x - selectedCells[0].x >= 2 && selectedCells[selectedCells.Length - 1].y - selectedCells[0].y >= 2;
            if (isValidArea) gameManager.canvasScript.SelectArea();
            else gameManager.canvasScript.ShowButtons();
        }
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
            case EnclosureScript.EnclosureType.Pen: enclosureScript = enclosure.AddComponent<PenScript>(); break;
            case EnclosureScript.EnclosureType.Training: enclosureScript = enclosure.AddComponent<TrainingScript>(); break;
        }
        enclosureScript.constructionType = ConstructionScript.ConstructionType.Enclosure;
        enclosureScript.cells = selectedCells;
        enclosureScript.enclosureType = enclosureType;
        enclosureScript.width = (int)selectedCells[selectedCells.Length - 1].x - (int)selectedCells[0].x + 1;
        enclosureScript.length = (int)selectedCells[selectedCells.Length - 1].y - (int)selectedCells[0].y + 1;
        enclosureScript.islandScript = islandScript;
        enclosureScript.islandEditor = islandEditor;

        GameObject fences = new GameObject("Fences");
        fences.transform.parent = enclosure.transform;
        fences.transform.localPosition = Vector3.zero;
        Vector3[] positions;
        Quaternion[] rotations;
        MeshGenerator.GetFencePositions(selectedCells, islandScript.meshData, out positions, out rotations);
        for (int i = 0; i < positions.Length - 1; i++)
        {
            GameObject fence = Instantiate(islandEditor.fences[UnityEngine.Random.Range(0, islandEditor.fences.Length)], islandScript.transform.position + positions[i], rotations[i], fences.transform);
            if (i == 0) enclosureScript.minPos = fence.transform.localPosition;
            else if (i == positions.Length - 2) enclosureScript.maxPos = fence.transform.localPosition - new Vector3(0, 0, 1);
        }

        if (enclosureType == EnclosureScript.EnclosureType.Pen)
        {
            ((PenScript)enclosureScript).openGate = Instantiate(islandEditor.gateOpen, islandScript.transform.position + positions[positions.Length - 1], rotations[rotations.Length - 1], fences.transform);
            ((PenScript)enclosureScript).openGate.SetActive(false);
            ((PenScript)enclosureScript).closedGate = Instantiate(islandEditor.gateClosed, islandScript.transform.position + positions[positions.Length - 1], rotations[rotations.Length - 1], fences.transform);
        }
        else
        {
            GameObject post = Instantiate(islandEditor.post, islandScript.transform.position + positions[positions.Length - 1], rotations[rotations.Length - 1], fences.transform);
            if (enclosureType == EnclosureScript.EnclosureType.Garden)
            {
                ((GardenScript)enclosureScript).patches = new GameObject("Patches");
                ((GardenScript)enclosureScript).patches.transform.parent = enclosure.transform;
                ((GardenScript)enclosureScript).patches.transform.localPosition = Vector3.zero;
            }
        }

        BoxCollider boxCollider = enclosure.AddComponent<BoxCollider>();
        boxCollider.center = (enclosureScript.minPos + enclosureScript.maxPos) / 2;
        boxCollider.size = new Vector3(enclosureScript.maxPos.x - enclosureScript.minPos.x, 1, enclosureScript.minPos.z - enclosureScript.maxPos.z);
        boxCollider.isTrigger = true;

        enclosureScript.maxPeasants = (enclosureScript.width - 2) * (enclosureScript.length - 2);
        enclosureScript.minPos += islandScript.transform.position;
        enclosureScript.maxPos += islandScript.transform.position;
        enclosureScript.outline = enclosureScript.AddComponent<Outline>();

        enclosureScript.entry = Instantiate(islandEditor.enclosureCenter, islandScript.transform.position + boxCollider.center, islandEditor.enclosureCenter.transform.rotation, enclosure.transform).transform;

        islandScript.AddConstruction(enclosureScript);

        InvertRegions(selectedCells);
        DestroyAllCells();

        gameManager.SelectConstruction(enclosureScript);
    }

    public void RemoveConstruction(ConstructionScript constructionScript)
    {
        InvertRegions(constructionScript.cells);
        islandScript.RemoveConstruction(constructionScript);

        DestroyAllCells();
    }

    private void InvertRegions(Vector2[] cells)
    {
        int minX = (int)cells[0].x, maxX = (int)cells[cells.Length - 1].x;
        int minY = (int)cells[0].y, maxY = (int)cells[cells.Length - 1].y;

        for (int i = minX; i <= maxX; i++)
        {
            for (int j = minY; j <= maxY; j++)
            {
                islandScript.regionMap[i, j] = -islandScript.regionMap[i, j];
            }
        }
    }

    // -------------------------------- BUILDINGS ------------------------------------

    public void ChooseBuilding(BuildingScript.BuildingType type)
    {
        GameObject buildingPrefab;
        switch (type)
        {
            case BuildingScript.BuildingType.Warehouse: buildingPrefab = islandEditor.warehouse; break;
            case BuildingScript.BuildingType.Cabin: buildingPrefab = islandEditor.cabin; break;
            case BuildingScript.BuildingType.Tavern: buildingPrefab = islandEditor.tavern; break;
            case BuildingScript.BuildingType.Alchemist: buildingPrefab = islandEditor.alchemist; break;
            case BuildingScript.BuildingType.Mine: buildingPrefab = islandEditor.mine; break;
            default: buildingPrefab = null; break;
        }

        selectedBuilding = Instantiate(buildingPrefab, islandScript.transform.position, buildingPrefab.transform.rotation, islandScript.constructions.transform).GetComponent<BuildingScript>();

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

}
