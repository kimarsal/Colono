using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IslandCellScript : MonoBehaviour
{
    public GameManager gameManagerScript;
    public IslandGenerator islandGeneratorScript;
    private Collider _collider;

    public MeshData meshData;
    public byte[,] regionMap;
    private Transform cellsTransform;
    private GameObject[,] cells;

    private Vector2 hoveredCell;
    private Vector2 selectedCell;
    private Vector2[] selectedCells;
    private bool wasOtherCellHovered = false;
    private bool wasOtherCellSelected = false;
    private bool isInSelectMode = false;
    private bool isSelectionValid = false;
    private bool isMultiselecting = false;

    private void Start()
    {
        gameManagerScript = GameObject.Find("GameManager").GetComponent<GameManager>();
        islandGeneratorScript = GameObject.Find("GameManager").GetComponent<IslandGenerator>();
        _collider = GetComponent<Collider>();
        cells = new GameObject[IslandGenerator.mapChunkSize, IslandGenerator.mapChunkSize];
        cellsTransform = transform.GetChild(1);

        selectedCells = new Vector2[0];
    }

    private void LateUpdate()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (_collider.Raycast(ray, out hit, 100))
        {
            Vector3 islandPoint = hit.point - transform.position;
            Vector2 mapPoint = new Vector2(IslandGenerator.mapChunkSize / 2 + islandPoint.x, IslandGenerator.mapChunkSize / 2 - islandPoint.z);
            int x = Mathf.FloorToInt(mapPoint.x);
            int y = Mathf.FloorToInt(mapPoint.y);
            //Debug.Log(new Vector2(x, y));

            HandleCells(x, y);
        }
    }

    private void HandleCells(int x, int y)
    {
        if (Input.GetMouseButtonDown(0)) //Ha fet clic
        {
            bool canChangeSelectedCell = false;
            if (!wasOtherCellSelected) canChangeSelectedCell = true; //No hi havia cap cel·la seleccionada
            else
            {
                if (!(selectedCell.x == x && selectedCell.y == y)) //La cel·la és diferent a la seleccionada
                {
                    canChangeSelectedCell = true;
                }
                else //Es deselecciona la cel·la actual
                {
                    cells[x, y].GetComponent<MeshRenderer>().material = gameManagerScript.selectingMaterial;
                }
            }

            if (canChangeSelectedCell)
            {
                for (int i = 0; i < selectedCells.Length; i++)
                {
                    if(!(selectedCells[i].x == x && (int)selectedCells[i].y == y))
                    {
                        Destroy(cells[(int)selectedCells[i].x, (int)selectedCells[i].y].gameObject);
                    }
                }

                if (islandGeneratorScript.regions[regionMap[x, y]].name == "Grass") //Si la cel·la és gespa
                {
                    isInSelectMode = true;
                    isSelectionValid = true;
                    isMultiselecting = false;
                    selectedCell = new Vector2(x, y);
                    cells[x, y].GetComponent<MeshRenderer>().material = gameManagerScript.selectingMaterial;
                    selectedCells = new Vector2[] { new Vector2(x, y) };
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
        else if (Input.GetMouseButtonUp(0)) //Ha deixat de fer clic
        {
            isInSelectMode = false;
            if(selectedCell.x == x && selectedCell.y == y)
            {
                /*if(isMultiselecting)
                {
                    cells[x, y].GetComponent<MeshRenderer>().material = gameManagerScript.hoverMaterial;
                    wasOtherCellSelected = false;
                }
                else
                {
                    //CreateCell(selectedCell, gameManagerScript.selectedHoverMaterial);
                    cells[x, y].GetComponent<MeshRenderer>().material = gameManagerScript.selectedHoverMaterial;
                    wasOtherCellSelected = true;
                }*/
                cells[x, y].GetComponent<MeshRenderer>().material = gameManagerScript.selectedHoverMaterial;
                wasOtherCellSelected = true;
            }
            else
            {
                if (isSelectionValid)
                {
                    for (int i = 0; i < selectedCells.Length; i++)
                    {
                        cells[(int)selectedCells[i].x, (int)selectedCells[i].y].GetComponent<MeshRenderer>().material = gameManagerScript.selectedMaterial;
                    }
                    wasOtherCellSelected = true;
                }
                else
                {
                    for (int i = 0; i < selectedCells.Length; i++)
                    {
                        Destroy(cells[(int)selectedCells[i].x, (int)selectedCells[i].y].gameObject);
                    }
                    wasOtherCellSelected = false;
                }
            }
        }
        else //No ha fet clic
        {
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
                    for(int i = 0; i < selectedCells.Length; i++)
                    {
                        Destroy(cells[(int)selectedCells[i].x, (int)selectedCells[i].y].gameObject);
                    }

                    hoveredCell = new Vector2(x, y);
                    isMultiselecting = true;
                    
                    isSelectionValid = true;
                    int numColumns = (int)Mathf.Abs(selectedCell.x - x);
                    int numRows = (int)Mathf.Abs(selectedCell.y - y);
                    selectedCells = new Vector2[(numColumns+1) * (numRows+1)];

                    int i_init = (int)(selectedCell.x > x ? x : selectedCell.x);
                    int i_fi = (int)(selectedCell.x > x ? selectedCell.x : x);
                    int j_init = (int)(selectedCell.y > y ? y : selectedCell.y);
                    int j_fi = (int)(selectedCell.y > y ? selectedCell.y : y);

                    int index = 0;
                    for (int i = i_init; i <= i_fi; i++){
                        for (int j = j_init; j <= j_fi; j++)
                        {
                            selectedCells[index] = new Vector2(i, j);
                            if(islandGeneratorScript.regions[regionMap[i, j]].name != "Grass")
                            {
                                isSelectionValid = false;
                            }
                            index++;
                        }
                    }

                    for(int i = 0; i < selectedCells.Length; i++)
                    {
                        CreateCell(selectedCells[i], isSelectionValid ? gameManagerScript.selectingMaterial : gameManagerScript.invalidSelectionMaterial);
                    }
                    cells[(int)selectedCell.x, (int)selectedCell.y].GetComponent<MeshRenderer>().material = gameManagerScript.selectingFirstCellMaterial;
                }
                else
                {
                    if (wasOtherCellSelected && selectedCells.Contains(hoveredCell)) //La anterior cel·la hovered estava seleccionada
                    {
                        cells[(int)hoveredCell.x, (int)hoveredCell.y].GetComponent<MeshRenderer>().material = gameManagerScript.selectedMaterial;
                    }
                    else
                    {
                        if (wasOtherCellHovered) Destroy(cells[(int)hoveredCell.x, (int)hoveredCell.y].gameObject); //S'elimina l'anterior cel·la hovered
                    }

                    if (islandGeneratorScript.regions[regionMap[x, y]].name == "Grass") //Si la cel·la és gespa
                    {
                        hoveredCell = new Vector2(x, y);
                        if (wasOtherCellSelected && selectedCells.Contains(new Vector2(x, y))) //La cel·la hovered està seleccionada
                        {
                            cells[(int)hoveredCell.x, (int)hoveredCell.y].GetComponent<MeshRenderer>().material = gameManagerScript.selectedHoverMaterial;
                        }
                        else
                        {
                            CreateCell(hoveredCell, gameManagerScript.hoverMaterial);
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

        MeshData cellMeshData = MeshGenerator.GenerateCell(position, meshData);
        Mesh mesh = cellMeshData.CreateMesh();
        meshFilter.mesh = mesh;

        newCell.transform.parent = cellsTransform;
        newCell.transform.localPosition = Vector3.zero;

        cells[(int)position.x, (int)position.y] = newCell;
    }
}
