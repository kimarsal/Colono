using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class IslandCellScript : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
{
    public enum SelectFunction { PlantTrees, ClearItems, CancelItemClearing, CreateEnclosure, PlaceBuilding };
    private enum SelectMode { None, Selecting, Building };
    private IslandGenerator islandGenerator;
    public IslandScript islandScript;

    [SerializeField] private Transform cellsTransform;
    private GameObject[,] cells;

    private SelectFunction selectFunction;
    private SelectMode selectMode;
    private Vector2 hoveredCell;
    private Vector2 selectedCell;
    private Vector2[] selectedCells = new Vector2[0];
    private bool wasOtherCellHovered = false;
    private bool isSelectionValid = false;

    private EnclosureScript.EnclosureType selectedEnclosureType;
    private BuildingScript selectedBuilding;
    private List<ItemScript> selectedItems;

    [SerializeField] private AudioSource thudSource;
    [SerializeField] private ParticleSystem puffPrefab;

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
                CanvasScript.Instance.ShowDefaultButtons();
                GameManager.Instance.DisableIslandCellScript();
            }
        }
        else {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CanvasScript.Instance.ShowDefaultButtons();
                GameManager.Instance.DisableIslandCellScript();
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
                CanvasScript.Instance.ShowDefaultButtons();
            }
            else
            {
                ChangeSelectedBuildingColor(Color.white);
                selectedBuilding.cells = selectedCells;
                selectedBuilding.position = selectedBuilding.transform.position;
                islandScript.UseResource(ResourceScript.ResourceType.Material, (int)ResourceScript.MaterialType.Wood, selectedBuilding.requiredWood);
                islandScript.UseResource(ResourceScript.ResourceType.Material, (int)ResourceScript.MaterialType.Stone, selectedBuilding.requiredStone);
                islandScript.AddConstruction(selectedBuilding); //Col·locar edifici
                GameManager.Instance.islandSelectionScript.enabled = true;
                GameManager.Instance.islandSelectionScript.SelectConstruction(selectedBuilding);

                Instantiate(puffPrefab, selectedBuilding.entry.position, puffPrefab.transform.rotation);
            }
            GameManager.Instance.DisableIslandCellScript();
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
                else
                {
                    if (islandScript.isCellTaken(selectedCell)) selectedItems = new List<ItemScript>() { islandScript.itemDictionary[selectedCell] };
                    else selectedItems = new List<ItemScript>();

                    isSelectionValid = selectedItems.Count == 0 && selectFunction != SelectFunction.ClearItems && selectFunction != SelectFunction.CancelItemClearing
                        || selectedItems.Count > 0 && (selectFunction == SelectFunction.ClearItems || selectFunction == SelectFunction.CancelItemClearing);
                }

                cells[(int)selectedCell.x, (int)selectedCell.y].GetComponent<MeshRenderer>().material = ResourceScript.Instance.selectingFirstCellMaterial;
            }
            else
            {
                CanvasScript.Instance.ShowDefaultButtons();
                GameManager.Instance.DisableIslandCellScript();
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isSelectionValid)
        {
            if (selectFunction == SelectFunction.CreateEnclosure)
            {
                int fenceAmount = (int)((selectedCells[selectedCells.Length - 1].x - selectedCells[0].x + 1) * 2 + (selectedCells[selectedCells.Length - 1].y - selectedCells[0].y - 1) * 2);
                islandScript.UseResource(ResourceScript.ResourceType.Material, (int)ResourceScript.MaterialType.Wood, fenceAmount);
                EnclosureScript enclosureScript = islandScript.CreateEnclosure(selectedEnclosureType, selectedCells);
                islandScript.AddConstruction(enclosureScript);
                GameManager.Instance.islandSelectionScript.enabled = true;
                GameManager.Instance.islandSelectionScript.SelectConstruction(enclosureScript);

                Instantiate(puffPrefab, enclosureScript.entry.position, puffPrefab.transform.rotation);
            }
            else if(selectFunction == SelectFunction.PlantTrees)
            {
                islandScript.UseResource(ResourceScript.ResourceType.Material, (int)ResourceScript.MaterialType.Sprout, selectedCells.Length);
                islandScript.PlantTrees(selectedCells);
                CanvasScript.Instance.ShowDefaultButtons();
            }
            else
            {
                foreach (ItemScript item in selectedItems)
                {
                    item.ChangeItemClearingState(selectFunction == SelectFunction.ClearItems);
                }
                CanvasScript.Instance.ShowDefaultButtons();
            }
        }
        else
        {
            CanvasScript.Instance.ShowDefaultButtons();
        }

        GameManager.Instance.DisableIslandCellScript();
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        int x, y;
        GetPointerCoordinates(eventData, out x, out y);
        
        if ((wasOtherCellHovered || selectMode != SelectMode.None) && hoveredCell.x == x && hoveredCell.y == y) return; //La cel·la és la mateixa a la hovered

        DestroyAllCells();
        hoveredCell = new Vector2(x, y);

        if (selectMode == SelectMode.None)
        {
            if (islandGenerator.regions[islandScript.regionMap[x, y]].type == Terrain.TerrainType.Field
                || islandGenerator.regions[islandScript.regionMap[x, y]].type == Terrain.TerrainType.Hill) //Si la cel·la és gespa
            {
                CreateCell(hoveredCell, ResourceScript.Instance.hoverMaterial);
                wasOtherCellHovered = true;

                thudSource.Play();
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
            int availableSprouts = islandScript.GetResourceAmount(ResourceScript.ResourceType.Material, (int)ResourceScript.MaterialType.Sprout);
            if (selectedItems.Count > 0 || availableSprouts < selectedCells.Length)
            {
                isSelectionValid = false;
            }
        }
        if (selectFunction == SelectFunction.ClearItems || selectFunction == SelectFunction.CancelItemClearing)
        {
            if (selectedItems.Count == 0) isSelectionValid = false;
        }
        else if (selectFunction == SelectFunction.CreateEnclosure)
        {
            int availableWood = islandScript.GetResourceAmount(ResourceScript.ResourceType.Material, (int)ResourceScript.MaterialType.Wood);
            int fenceAmount = (int)((endCell.x - startCell.x) * 2 + (endCell.y - startCell.y - 2) * 2);
            if (selectedItems.Count > 0 || endCell.x - startCell.x < 2 || endCell.y - startCell.y < 2
                || availableWood < fenceAmount)
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
            CreateCell(cell, isSelectionValid ? ResourceScript.Instance.selectingMaterial : ResourceScript.Instance.invalidSelectionMaterial);
        }
        cells[(int)selectedCell.x, (int)selectedCell.y].GetComponent<MeshRenderer>().material = ResourceScript.Instance.selectingFirstCellMaterial;

        thudSource.Play();
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

        newCell.transform.parent = cellsTransform;
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

    public void ChooseBuilding(BuildingScript buildingScript)
    {
        selectedBuilding = Instantiate(buildingScript, islandScript.constructionsTransform.transform);
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

    //

    public void ManageItems(SelectFunction selectFunction)
    {
        this.selectFunction = selectFunction;
        selectMode = SelectMode.None;
    }

}
