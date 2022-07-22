using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class IslandScript : MonoBehaviour
{
    public bool hasBeenDiscovered = false;
    public GameManager gameManagerScript;
    public IslandGenerator islandGeneratorScript;
    public string islandName;
    public MeshData meshData;
    public byte[,] regionMap;
    public Material selectedMaterial;

    private Collider coll;
    private Transform cellsTransform;
    private GameObject[,] cells;
    private Vector2 previousCell;

    private void Start()
    {
        gameManagerScript = GameObject.Find("GameManager").GetComponent<GameManager>();
        islandGeneratorScript = GameObject.Find("GameManager").GetComponent<IslandGenerator>();
        coll = GetComponent<Collider>();
        cells = new GameObject[IslandGenerator.mapChunkSize, IslandGenerator.mapChunkSize];
        cellsTransform = transform.GetChild(1);
    }

    public void PlayerIsNear()
    {
        gameManagerScript.PlayerIsNearIsland(transform.position, islandName, hasBeenDiscovered);
    }

    public void PlayerLeft()
    {
        gameManagerScript.PlayerLeftIsland();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (coll.Raycast(ray, out hit, 100))
            {
                Vector3 islandPoint = hit.point - transform.position;
                Vector2 mapPoint = new Vector2(IslandGenerator.mapChunkSize / 2 + islandPoint.x, IslandGenerator.mapChunkSize / 2 - islandPoint.z);
                int x = Mathf.FloorToInt(mapPoint.x);
                int y = Mathf.FloorToInt(mapPoint.y);
                //Debug.Log(new Vector2(x, y));

                if (x >= 0 && x < IslandGenerator.mapChunkSize && y >= 0 && y < IslandGenerator.mapChunkSize)
                {
                    if(!(previousCell != null && previousCell.x == x && previousCell.y == y))
                    {
                        Destroy(cells[(int)previousCell.x, (int)previousCell.y]);
                        previousCell = new Vector2(x, y);
                        if(islandGeneratorScript.regions[regionMap[x, y]].name == "Grass")
                        {
                            CreateCell();
                        }
                    }
                }

            }
        }
    }

    private void CreateCell()
    {
        GameObject newCell = new GameObject("cell");
        MeshRenderer meshRenderer = newCell.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = newCell.AddComponent<MeshFilter>();
        
        meshRenderer.material = gameManagerScript.selectedMaterial;

        MeshData cellMeshData = MeshGenerator.GenerateCell(previousCell, meshData);
        Mesh mesh = cellMeshData.CreateMesh();
        meshFilter.mesh = mesh;

        newCell.transform.parent = cellsTransform;
    }


}
