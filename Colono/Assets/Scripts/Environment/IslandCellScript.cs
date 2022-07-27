using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class IslandCellScript : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
{
    public GameManager gameManagerScript;
    public IslandGenerator islandGeneratorScript;
    public IslandEditor islandEditorScript;
    public IslandScript islandScript;
    private Collider _collider;

    public MeshData meshData;
    public byte[,] regionMap;
    private Transform cellsTransform;
    private GameObject[,] cells;

    private Vector2 hoveredCell;
    private Vector2 selectedCell;
    private Vector2[] selectedCells;
    private bool wasOtherCellHovered = false;
    private bool isInSelectMode = false;
    private bool isSelectionValid = false;

    private void Start()
    {
        gameManagerScript = GameObject.Find("GameManager").GetComponent<GameManager>();
        islandGeneratorScript = GameObject.Find("GameManager").GetComponent<IslandGenerator>();
        islandEditorScript = GameObject.Find("GameManager").GetComponent<IslandEditor>();
        _collider = GetComponent<Collider>();
        cells = new GameObject[IslandGenerator.mapChunkSize, IslandGenerator.mapChunkSize];
        cellsTransform = transform.GetChild(1);

        selectedCells = new Vector2[0];
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        int x, y;
        GetPointerCoordinates(out x, out y);
        if (!gameManagerScript.isInIsland || x == -1 && y == -1)
        {
            return;
        }

        gameManagerScript.ZoneUnelected();

        bool canChangeSelectedCell = false;
        if (selectedCells.Length == 0) canChangeSelectedCell = true; //No hi havia cap cel·la seleccionada
        else
        {
            if (!(selectedCell.x == x && selectedCell.y == y)) //La cel·la és diferent a la seleccionada
            {
                canChangeSelectedCell = true;
            }
            else //Es deselecciona la cel·la actual
            {
                cells[x, y].GetComponent<MeshRenderer>().material = islandEditorScript.selectingMaterial;
            }
        }

        if (canChangeSelectedCell)
        {
            for (int i = 0; i < selectedCells.Length; i++)
            {
                if (!(selectedCells[i].x == x && (int)selectedCells[i].y == y))
                {
                    Destroy(cells[(int)selectedCells[i].x, (int)selectedCells[i].y].gameObject);
                }
            }

            if (islandGeneratorScript.regions[regionMap[x, y]].name == "Grass") //Si la cel·la és gespa
            {
                try
                {
                    cells[x, y].GetComponent<MeshRenderer>().material = islandEditorScript.selectingMaterial;
                    isInSelectMode = true;
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

        isInSelectMode = false;
        if (selectedCell.x == x && selectedCell.y == y)
        {
            cells[x, y].GetComponent<MeshRenderer>().material = islandEditorScript.selectedHoverMaterial;
            SingleSelection();
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
            if (isInSelectMode)
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
                        if (islandGeneratorScript.regions[regionMap[i, j]].name != "Grass")
                        {
                            isSelectionValid = false;
                        }
                        index++;
                    }
                }

                for (int i = 0; i < selectedCells.Length; i++)
                {
                    CreateCell(selectedCells[i], isSelectionValid ? islandEditorScript.selectingMaterial : islandEditorScript.invalidSelectionMaterial);
                }
                cells[(int)selectedCell.x, (int)selectedCell.y].GetComponent<MeshRenderer>().material = islandEditorScript.selectingFirstCellMaterial;
            }
            else
            {
                if (selectedCells.Contains(hoveredCell)) //La anterior cel·la hovered estava seleccionada
                {
                    cells[(int)hoveredCell.x, (int)hoveredCell.y].GetComponent<MeshRenderer>().material = islandEditorScript.selectedMaterial;
                }
                else
                {
                    if (wasOtherCellHovered) Destroy(cells[(int)hoveredCell.x, (int)hoveredCell.y].gameObject); //S'elimina l'anterior cel·la hovered
                }

                if (islandGeneratorScript.regions[regionMap[x, y]].name == "Grass") //Si la cel·la és gespa
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
            //Debug.Log(new Vector2(x, y));
        }
    }

    private void SingleSelection()
    {

    }

    private void MultipleSelection()
    {
        gameManagerScript.ZoneSelected();
    }

    public void CreateZone()
    {
        GameObject zone = new GameObject("Zone");
        zone.transform.parent = transform;
        zone.transform.localPosition = Vector3.zero;

        Vector3[] positions;
        Quaternion[] rotations;
        MeshGenerator.GetFencePositions(selectedCells, meshData, out positions, out rotations);
        for (int i = 0; i < positions.Length - 1; i++)
        {
            GameObject fence = Instantiate(islandEditorScript.fences[UnityEngine.Random.Range(0, islandEditorScript.fences.Length)], transform.position + positions[i], rotations[i], zone.transform);
        }

        GameObject post = Instantiate(islandEditorScript.post, transform.position + positions[positions.Length - 1], rotations[rotations.Length - 1], zone.transform);
        islandScript.AddZone(zone);
        
        for (int i = 0; i < selectedCells.Length; i++)
        {
            regionMap[(int)selectedCells[i].x, (int)selectedCells[i].y] = 0;
            Destroy(cells[(int)selectedCells[i].x, (int)selectedCells[i].y].gameObject);
        }
        selectedCells = new Vector2[0];
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

        newCell.transform.parent = cellsTransform;
        newCell.transform.localPosition = Vector3.zero;

        cells[(int)position.x, (int)position.y] = newCell;
    }
}
