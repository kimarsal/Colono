using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class IslandCellScript : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
{
    public enum SelectFunction { PlantTrees, ClearItems, CancelItemClearing, CreateEnclosure, PlaceBuilding };
    public enum SelectMode { None, Selecting, Building };
    public GameManager gameManager;
    private IslandGenerator islandGenerator;
    public IslandEditor islandEditor { get { return gameManager.islandEditor; } }
    public IslandScript islandScript;

    private GameObject[,] cells;

    public SelectFunction selectFunction;
    public SelectMode selectMode;
    private Vector2 hoveredCell;
    private Vector2 selectedCell;
    private Vector2[] selectedCells = new Vector2[0];
    private bool wasOtherCellHovered = false;
    private bool isSelectionValid = false;

    private EnclosureScript.EnclosureType selectedEnclosureType;
    private BuildingScript selectedBuilding;
    private List<ItemScript> selectedItems;

    private void Start()
    {
        islandGenerator = GetComponent<IslandGenerator>();
        cells = new GameObject[IslandGenerator.mapChunkSize, IslandGenerator.mapChunkSize];
    }

    private void Update()
    {
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            OnPointerMove(null);
        }

        if (selectMode == SelectMode.Building) //Si està col·locant un edifici
        {
            if (Input.GetKeyDown(KeyCode.Comma))
            {
                selectedBuilding.transform.Rotate(new Vector3(0, -90, 0)); //Es rota en sentit antihorari
                selectedBuilding.orientation -= 1;
                if (selectedBuilding.orientation == -1) selectedBuilding.orientation = 3;
                OnPointerMove(null);
            }
            else if (Input.GetKeyDown(KeyCode.Period))
            {
                selectedBuilding.transform.Rotate(new Vector3(0, 90, 0)); //Es rota en sentit horari
                selectedBuilding.orientation = (selectedBuilding.orientation + 1) % 4;
                OnPointerMove(null);
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                Destroy(selectedBuilding.gameObject); //Eliminar l'edifici
                gameManager.canvasScript.ShowDefaultButtons();
                gameManager.DisableIslandCellScript();
            }
        }
        else {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                gameManager.canvasScript.ShowDefaultButtons();
                gameManager.DisableIslandCellScript();
            }
        }
    }

    private void GetPointerCoordinates(PointerEventData eventData, out int x, out int y)
    {
        Vector3 islandPoint;
        if (eventData != null)
        {
            islandPoint = eventData.pointerCurrentRaycast.worldPosition - islandScript.transform.position;
        }
        else
        {
            RaycastHit raycastHit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out raycastHit, 100f);
            islandPoint = raycastHit.point - islandScript.transform.position;
        }
        Vector2 mapPoint = new Vector2(IslandGenerator.mapChunkSize / 2 + islandPoint.x, IslandGenerator.mapChunkSize / 2 - islandPoint.z);
        x = Mathf.FloorToInt(mapPoint.x);
        y = Mathf.FloorToInt(mapPoint.y);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (selectMode == SelectMode.Building) //Si intenta col·locar l'edifici
        {
            if (!isSelectionValid) //Si la posició no és vàlida
            {
                Destroy(selectedBuilding.gameObject); //Eliminar l'edifici
                gameManager.canvasScript.ShowDefaultButtons();
            }
            else
            {
                ChangeSelectedBuildingColor(Color.white);
                selectedBuilding.cells = selectedCells;
                selectedBuilding.position = selectedBuilding.transform.position;
                islandScript.AddConstruction(selectedBuilding); //Col·locar edifici
                gameManager.SelectConstruction(selectedBuilding);

            }
            gameManager.DisableIslandCellScript();
        }

        else
        {
            if (wasOtherCellHovered)
            {
                selectMode = SelectMode.Selecting;
                selectedCell = hoveredCell;

                isSelectionValid = true;

                if (islandScript.regionMap[(int)selectedCell.x, (int)selectedCell.y] < 0 // No està disponible
                    || (islandGenerator.regions[islandScript.regionMap[(int)selectedCell.x, (int)selectedCell.y]].type != Terrain.TerrainType.Field
                    && islandGenerator.regions[islandScript.regionMap[(int)selectedCell.x, (int)selectedCell.y]].type != Terrain.TerrainType.Hill)) //No és gespa
                {
                    isSelectionValid = false;
                }
                else if (islandScript.isCellTaken(selectedCell))
                {
                    selectedItems = new List<ItemScript>() { islandScript.itemDictionary[selectedCell] };

                    if (selectFunction == SelectFunction.PlantTrees) isSelectionValid = false;
                }
                else if (selectFunction != SelectFunction.PlantTrees) isSelectionValid = false;

                cells[(int)selectedCell.x, (int)selectedCell.y].GetComponent<MeshRenderer>().material = islandEditor.selectingFirstCellMaterial;
            }
            else
            {
                gameManager.canvasScript.ShowDefaultButtons();
                gameManager.DisableIslandCellScript();
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isSelectionValid)
        {
            if (selectFunction == SelectFunction.CreateEnclosure)
            {
                EnclosureScript enclosureScript = islandScript.CreateEnclosure(selectedEnclosureType, selectedCells);
                islandScript.AddConstruction(enclosureScript);
                gameManager.SelectConstruction(enclosureScript);
            }
            else if(selectFunction == SelectFunction.PlantTrees)
            {
                islandScript.PlantTrees(selectedCells);
                gameManager.canvasScript.ShowDefaultButtons();
            }
            else
            {
                foreach (ItemScript item in selectedItems)
                {
                    item.ChangeItemClearingState(selectFunction == SelectFunction.ClearItems);
                }
                gameManager.canvasScript.ShowDefaultButtons();
            }
        }
        else
        {
            gameManager.canvasScript.ShowDefaultButtons();
        }

        gameManager.DisableIslandCellScript();
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        int x, y;
        GetPointerCoordinates(eventData, out x, out y);
        
        if (wasOtherCellHovered && hoveredCell.x == x && hoveredCell.y == y) return; //La cel·la és la mateixa a la hovered

        DestroyAllCells();
        hoveredCell = new Vector2(x, y);

        if (selectMode == SelectMode.None)
        {
            if (islandScript.regionMap[x, y] < 0 // No està disponible
                || islandGenerator.regions[islandScript.regionMap[x, y]].type == Terrain.TerrainType.Field
                || islandGenerator.regions[islandScript.regionMap[x, y]].type == Terrain.TerrainType.Hill) //Si la cel·la és gespa
            {
                CreateCell(hoveredCell, islandEditor.hoverMaterial);
                wasOtherCellHovered = true;
            }
            return;
        }

        Vector2 startCell = hoveredCell;
        Vector2 endCell = hoveredCell;
        int vertexIndex = 0; //vèrtex en el qual col·locar l'edifici

        if (selectMode == SelectMode.Building)
        {
            switch (selectedBuilding.orientation) //segons la orientació de l'edifici
            {
                case 0:
                    vertexIndex = 3;
                    startCell -= new Vector2(selectedBuilding.width - 1, selectedBuilding.length - 1);
                    break;
                case 1:
                    vertexIndex = 2;
                    startCell -= new Vector2(0, selectedBuilding.width - 1);
                    endCell += new Vector2(selectedBuilding.length - 1, 0);

                    break;
                case 2:
                    vertexIndex = 0;
                    endCell += new Vector2(selectedBuilding.width - 1, selectedBuilding.length - 1);
                    break;
                case 3:
                    vertexIndex = 1;
                    startCell -= new Vector2(selectedBuilding.length - 1, 0);
                    endCell += new Vector2(0, selectedBuilding.width - 1);
                    break;
            }

            selectedCell = hoveredCell;
        }
        else //if (selectMode == SelectMode.Selecting)
        {
            startCell = new Vector2(selectedCell.x > x ? x : selectedCell.x, selectedCell.y > y ? y : selectedCell.y);
            endCell = new Vector2(selectedCell.x > x ? selectedCell.x : x, selectedCell.y > y ? selectedCell.y : y);
        }

        selectedItems = new List<ItemScript>();
        selectedCells = new Vector2[(int)((endCell.x - startCell.x + 1) * (endCell.y - startCell.y + 1))];
        isSelectionValid = true;

        int index = 0;
        for (int row = (int)startCell.y; row <= endCell.y; row++)
        {
            for (int col = (int)startCell.x; col <= endCell.x; col++)
            {
                selectedCells[index] = new Vector2(col, row);
                if (islandScript.regionMap[col, row] < 0 // No està disponible
                    || (islandGenerator.regions[islandScript.regionMap[col, row]].type != Terrain.TerrainType.Field
                    && islandGenerator.regions[islandScript.regionMap[col, row]].type != Terrain.TerrainType.Hill)) //No és gespa
                {
                    isSelectionValid = false;
                }
                else if (islandScript.isCellTaken(selectedCells[index]))
                {
                    selectedItems.Add(islandScript.itemDictionary[selectedCells[index]]);
                }
                index++;
            }
        }

        // Es comprova la validesa de la selecció segons la funció
        if(selectFunction == SelectFunction.PlantTrees)
        {
            if (selectedItems.Count > 0) isSelectionValid = false;
        }
        if (selectFunction == SelectFunction.ClearItems || selectFunction == SelectFunction.CancelItemClearing)
        {
            if (selectedItems.Count == 0) isSelectionValid = false;
        }
        else if (selectFunction == SelectFunction.CreateEnclosure)
        {
            if (selectedItems.Count > 0 || endCell.x - startCell.x < 2 || endCell.y - startCell.y < 2)
            {
                isSelectionValid = false;
            }
        }
        else if (selectFunction == SelectFunction.PlaceBuilding)
        {
            if (selectedItems.Count > 0) isSelectionValid = false;

            //s'obté la posició de l'edifici segons el vèrtex inicial i l'alçada mitjana de la zona
            Vector3 vertex = MeshGenerator.GetBuildingPosition(selectedCells, hoveredCell, vertexIndex, islandScript.meshData);
            selectedBuilding.transform.position = islandScript.transform.position + vertex;
            ChangeSelectedBuildingColor(isSelectionValid ? Color.gray : Color.red);
        }

        foreach (Vector2 cell in selectedCells)
        {
            CreateCell(cell, isSelectionValid ? islandEditor.selectingMaterial : islandEditor.invalidSelectionMaterial);
        }
        cells[(int)selectedCell.x, (int)selectedCell.y].GetComponent<MeshRenderer>().material = islandEditor.selectingFirstCellMaterial;
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
        foreach (Vector2 cell in selectedCells)
        {
            Destroy(cells[(int)cell.x, (int)cell.y].gameObject);
        }
        selectedCells = new Vector2[0];

        if (wasOtherCellHovered)
        {
            Destroy(cells[(int)hoveredCell.x, (int)hoveredCell.y].gameObject);
            wasOtherCellHovered = false;
        }
    }

    //--------------------------------- ENCLOSURES ---------------------------------------

    public void ChooseEnclosure(EnclosureScript.EnclosureType enclosureType)
    {
        selectFunction = SelectFunction.CreateEnclosure;
        selectedEnclosureType = enclosureType;
        selectMode = SelectMode.None;
    }

    // -------------------------------- BUILDINGS ------------------------------------

    public void ChooseBuilding(BuildingScript.BuildingType buildingType)
    {
        selectedBuilding = Instantiate(islandEditor.GetBuilding(buildingType), islandScript.transform.position, Quaternion.identity, islandScript.constructionsTransform.transform).GetComponent<BuildingScript>();
        selectMode = SelectMode.Building;
        selectFunction = SelectFunction.PlaceBuilding;
    }

    private void ChangeSelectedBuildingColor(Color color)
    {
        foreach (MeshRenderer buildingPieceRenderer in selectedBuilding.transform.GetChild(0).GetComponentsInChildren<MeshRenderer>())
        {
            buildingPieceRenderer.material.color = color;
        }
    }

}
