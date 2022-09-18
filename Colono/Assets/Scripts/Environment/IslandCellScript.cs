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
        if(selectMode == SelectMode.Building) //Si est� col�locant un edifici
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
                buildingOrientation = 0; //Resetejar l'orientaci�
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

        if (_collider.Raycast(ray, out hit, 100)) //Si el ratol� �s sobre l'illa
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

        if (selectMode == SelectMode.Building) //Si intenta col�locar l'edifici
        {
            if (!isSelectionValid) //Si la posici� no �s v�lida
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
                islandScript.AddBuilding(selectedBuilding); //Col�locar edifici

                islandScript.ClearArea(selectedCells);
                InvertRegions(selectedCells, false);
                foreach (Vector2 cell in selectedCells)
                {
                    cells[(int)cell.x, (int)cell.y].GetComponent<MeshRenderer>().material = islandEditorScript.selectedMaterial;
                }
                cells[x, y].GetComponent<MeshRenderer>().material = islandEditorScript.selectedHoverMaterial;

                gameManagerScript.SelectBuilding(buildingScript);
            }
            buildingOrientation = 0; //Resetejar l'orientaci�
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

                if (regionMap[x, y] >= 0 && (regionMap[x, y] > 10 //Si �s terreny disponible
                    || islandGeneratorScript.regions[regionMap[x, y]].name == "Grass")) //Si la cel�la �s gespa
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

        if (!wasOtherCellHovered //No hi havia cap cel�la hovered
            || !(hoveredCell.x == x && hoveredCell.y == y)) //La cel�la �s diferent a la hovered
        {
            if(selectMode == SelectMode.Building)
            {
                DestroyAllCells();

                hoveredCell = new Vector2(x, y);
                int vertexIndex = 0; //v�rtex a partir del qual comen�a el requadre
                int i = 0, j = 0; //sentit del pas pels eixos x i y

                switch (buildingOrientation) //segons la orientaci� de l'edifici
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

                if (i == -1)
                {
                    int aux = x + 1; x = endX + 1; endX = aux;
                }
                if (j == -1)
                {
                    int aux = y + 1; y = endY + 1; endY = aux;
                }

                //es guarden les noves cel�les seleccionades a la llista i es comprova quin color hauran de tenir
                int index = 0;
                selectedCells = new Vector2[buildingScript.width * buildingScript.length];
                isSelectionValid = true;
                for (int col = x; col < endX; col++)
                {
                    for (int row = y; row < endY; row++)
                    {
                        selectedCells[index] = new Vector2(col, row);
                        if (regionMap[col, row] < 0 || islandGeneratorScript.regions[regionMap[col, row]].name != "Grass")
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

                //s'obt� la posici� de l'edifici segons el v�rtex inicial i l'al�ada mitjana de la zona
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
                    if (selectedCells.Contains(hoveredCell)) //L'anterior cel�la hovered estava seleccionada
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
                        Destroy(cells[(int)hoveredCell.x, (int)hoveredCell.y].gameObject); //S'elimina l'anterior cel�la hovered
                    }
                }

                hoveredCell = new Vector2(x, y);

                if (regionMap[x, y] < 0) //Si la cel�la est� ocupada
                {
                    if (selectedCells.Contains(hoveredCell)) //La nova cel�la hovered est� seleccionada
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
                    if (regionMap[x, y] > 10 || islandGeneratorScript.regions[regionMap[x, y]].name == "Grass") //Si la cel�la �s gespa
                    {
                        if (selectedCells.Contains(hoveredCell)) //La nova cel�la hovered est� seleccionada
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
                /*}
                catch (Exception e)
                {
                    CreateCell(hoveredCell, islandEditorScript.hoverMaterial);
                    Debug.Log(e); //S'ha intentat accedir a una cel�la eliminada
                    Debug.Log("Error al intentar moure el ratol�");
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

        if (wasOtherCellHovered)
        {
            Destroy(cells[(int)hoveredCell.x, (int)hoveredCell.y].gameObject);
            wasOtherCellHovered = false;
        }
    }

    //--------------------------------- ENCLOSURES ---------------------------------------

    private void SingleSelection()
    {
        if (regionMap[(int)selectedCell.x, (int)selectedCell.y] > 10) //Garden
        {
            selectedEnclosure = islandScript.GetEnclosureByCell(selectedCell);
            gameManagerScript.PatchSelected(selectedEnclosure.GetComponent<EnclosureScript>().isPatchEmpty(selectedCells));
        }
        else
        {
            gameManagerScript.ShowButtons();
        }
    }

    private void MultipleSelection()
    {
        if(regionMap[(int)selectedCell.x, (int)selectedCell.y] > 10) //Garden
        {
            selectedEnclosure = islandScript.GetEnclosureByCell(selectedCell);
            gameManagerScript.PatchSelected(selectedEnclosure.GetComponent<EnclosureScript>().isPatchEmpty(selectedCells));
        }
        else
        {
            bool isValidArea = selectedCells[selectedCells.Length - 1].x - selectedCells[0].x >= 2 && selectedCells[selectedCells.Length - 1].y - selectedCells[0].y >= 2;
            if (isValidArea) gameManagerScript.SelectArea();
            else gameManagerScript.ShowButtons();
        }
    }

    public void CreateEnclosure(EnclosureScript.EnclosureType type)
    {
        GameObject enclosure = new GameObject("Enclosure");
        enclosure.transform.parent = islandScript.constructions.transform;
        enclosure.transform.localPosition = Vector3.zero;
        EnclosureScript enclosureScript = enclosure.AddComponent<EnclosureScript>();
        enclosureScript.cells = selectedCells;
        enclosureScript.type = type;
        enclosureScript.width = (int)selectedCells[selectedCells.Length - 1].x - (int)selectedCells[0].x;
        enclosureScript.length = (int)selectedCells[selectedCells.Length - 1].y - (int)selectedCells[0].y;
        enclosureScript.isEnclosure = true;
        Transform enclosureCenter = Instantiate(islandEditorScript.constructionCenterPrefab,
            transform.position + (MeshGenerator.GetCellCenter(selectedCells[0], meshData) + MeshGenerator.GetCellCenter(selectedCells[selectedCells.Length - 1], meshData)) / 2,
            islandEditorScript.constructionCenterPrefab.transform.rotation,
            enclosure.transform).transform;
        enclosureScript.center = enclosureCenter;
        islandScript.ClearArea(selectedCells);

        GameObject fences = new GameObject("Fences");
        fences.transform.parent = enclosure.transform;
        fences.transform.localPosition = Vector3.zero;
        Vector3[] positions;
        Quaternion[] rotations;
        MeshGenerator.GetFencePositions(selectedCells, meshData, out positions, out rotations);
        for (int i = 0; i < positions.Length - 1; i++)
        {
            GameObject fence = Instantiate(islandEditorScript.fences[UnityEngine.Random.Range(0, islandEditorScript.fences.Length)], transform.position + positions[i], rotations[i], fences.transform);
        }

        if (type == EnclosureScript.EnclosureType.Barn)
        {
            enclosureScript.openGate = Instantiate(islandEditorScript.gateOpen, transform.position + positions[positions.Length - 1], rotations[rotations.Length - 1], fences.transform);
            enclosureScript.openGate.SetActive(false);
            enclosureScript.closedGate = Instantiate(islandEditorScript.gateClosed, transform.position + positions[positions.Length - 1], rotations[rotations.Length - 1], fences.transform);

            GameObject animals = new GameObject("Animals");
            animals.transform.parent = enclosure.transform;
            animals.transform.localPosition = Vector3.zero;
        }
        else
        {
            GameObject post = Instantiate(islandEditorScript.post, transform.position + positions[positions.Length - 1], rotations[rotations.Length - 1], fences.transform);
            GameObject crops = new GameObject("Crops");
            crops.transform.parent = enclosure.transform;
            crops.transform.localPosition = Vector3.zero;
        }
        islandScript.AddEnclosure(enclosure);

        InvertRegions(selectedCells, type == EnclosureScript.EnclosureType.Garden);

        selectedEnclosure = enclosure;
        gameManagerScript.SelectEnclosure(enclosureScript);
    }

    public void Plant()
    {
        EnclosureScript enclosureScript = selectedEnclosure.GetComponent<EnclosureScript>();
        Vector3[] positions;
        MeshGenerator.GetCropPositions(selectedCells, meshData, out positions);
        for (int i = 0; i < positions.Length; i++)
        {
            GameObject crop = Instantiate(islandEditorScript.tomato, transform.position + positions[i], islandEditorScript.tomato.transform.rotation, selectedEnclosure.transform.GetChild(1));
            CropScript cropScript = crop.AddComponent<CropScript>();
            cropScript.cell = selectedCells[i];
            cropScript.type = CropScript.CropType.Tomato;
            enclosureScript.AddCrop(selectedCells[i], cropScript);
        }

        DestroyAllCells();
    }

    public void ClearPatch()
    {
        EnclosureScript enclosureScript = selectedEnclosure.GetComponent<EnclosureScript>();
        foreach (Vector2 cell in selectedCells)
        {
            enclosureScript.RemoveCrop(cell);
        }

        DestroyAllCells();
    }

    public void SelectEnclosure(GameObject enclosure)
    {
        selectedEnclosure = enclosure;
        gameManagerScript.SelectEnclosure(enclosure.GetComponent<EnclosureScript>());
    }

    public void RemoveEnclosure()
    {
        Vector2[] enclosureCells = selectedEnclosure.GetComponent<EnclosureScript>().cells;
        InvertRegions(enclosureCells, selectedEnclosure.GetComponent<EnclosureScript>().type == EnclosureScript.EnclosureType.Garden, true);
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
            case BuildingScript.BuildingType.Residence: buildingPrefab = islandEditorScript.residence; break;
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
