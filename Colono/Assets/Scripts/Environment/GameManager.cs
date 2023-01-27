using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System;

public class GameManager : MonoBehaviour
{
    public ShipScript shipScript;
    public CameraScript cameraScript;
    public CanvasScript canvasScript;
    public InventoryEditor inventoryEditor;
    private ConstructionScript constructionScript;

    private bool isPlayerNearIsland = false;
    public bool isInIsland = false;

    public Transform cellsTransform;
    public IslandScript closestIsland;
    public IslandEditor islandEditor;
    private IslandCellScript islandCellScript;
    private IslandManager islandManager;

    public bool hasSelectedPeasant;

    private void Start()
    {
        islandEditor = GetComponent<IslandEditor>();
        islandCellScript = GetComponent<IslandCellScript>();
        islandCellScript.enabled = false;

        islandManager = GetComponent<IslandManager>();
        shipScript.gameManager = this;
        //LoadGame();
        StartGame();

        inventoryEditor.shipInventoryScript = shipScript.inventoryScript;
    }

    public void PlayerIsNearIsland()
    {
        if (!isPlayerNearIsland)
        {
            isPlayerNearIsland = true;
            canvasScript.PlayerIsNearIsland(closestIsland);
        }
    }

    public void BoardIsland()
    {
        closestIsland.convexColliders.SetActive(false);
        cameraScript.SetIslandCamera(closestIsland.transform.position);
        inventoryEditor.islandInventoryScript = closestIsland.inventoryScript;
        canvasScript.BoardIsland();

        shipScript.islandScript = closestIsland;

        islandCellScript.enabled = true;
        islandCellScript.islandScript = closestIsland;
    }

    public void ChangeSelectedItemsState(bool toClear)
    {
        islandCellScript.ChangeSelectedItemsState(toClear);
        islandCellScript.DestroyAllCells();

        canvasScript.UnselectArea();
    }

    public void SelectShip()
    {
        islandCellScript.DestroyAllCells();

        SelectConstruction(shipScript);
    }

    public void SelectPeasant(PeasantScript peasantScript)
    {
        hasSelectedPeasant = true;
        islandCellScript.DestroyAllCells();

        canvasScript.ShowPeasantDetails(peasantScript);
    }

    public void ChooseBuilding(int type)
    {
        islandCellScript.ChooseBuilding((BuildingScript.BuildingType)type);
        canvasScript.ChooseBuilding();
    }

    public void RemoveConstruction()
    {
        constructionScript.FinishUpBusiness();
        closestIsland.SendAllPeasantsBack(constructionScript);
        islandCellScript.RemoveConstruction(constructionScript);
        canvasScript.HideConstructionDetails();
    }

    public void CreateEnclosure(int enclosureType)
    {
        islandCellScript.CreateEnclosure((EnclosureScript.EnclosureType) enclosureType);
    }

    public void SelectConstruction(ConstructionScript newConstructionScript)
    {
        if(constructionScript != null)
        {
            constructionScript.outline.enabled = false;
        }
        constructionScript = newConstructionScript;
        constructionScript.outline.enabled = true;
        canvasScript.ShowConstructionDetails(constructionScript);
    }

    public void UnselectConstrucion()
    {
        constructionScript.constructionDetailsScript = null;
        constructionScript.outline.enabled = false;
        constructionScript = null;
        islandCellScript.DestroyAllCells();
        canvasScript.HideConstructionDetails();
    }

    public void LeaveIsland()
    {
        cameraScript.ResetPlayerCamera();
        canvasScript.LeaveIsland();

        closestIsland.CancelAllTripsToShip(shipScript);
        closestIsland.convexColliders.SetActive(true);
        islandCellScript.DestroyAllCells();
        islandCellScript.enabled = false;
    }

    public void PlayerIsFarFromIsland()
    {
        if (isPlayerNearIsland)
        {
            canvasScript.PlayerIsFarFromIsland();
            isPlayerNearIsland = false;
        }
    }

    public bool CanSelect()
    {
        return isInIsland && canvasScript.buttonState != CanvasScript.ButtonState.PopUp;
    }

    public void SaveGame()
    {
        IslandManager islandManager = GetComponent<IslandManager>();

        GameInfo gameInfo = new GameInfo();
        gameInfo.seed = islandManager.seed;
        gameInfo.playerPosition = new SerializableVector3(shipScript.transform.position);
        gameInfo.playerOrientation = Mathf.RoundToInt(shipScript.transform.rotation.eulerAngles.y);

        gameInfo.peasantList = shipScript.peasantInfoList;
        gameInfo.animalList = shipScript.animalList;
        gameInfo.inventoryScript = shipScript.inventoryScript;

        gameInfo.islandList = new List<IslandInfo>();
        foreach (IslandScript islandScript in islandManager.islandList)
        {
            gameInfo.islandList.Add(islandScript.GetIslandInfo());
        }

        string json = JsonConvert.SerializeObject(gameInfo, Formatting.Indented, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
        File.WriteAllText("gameInfo.json", json);
    }

    public void LoadGame()
    {
        GameInfo gameInfo = JsonConvert.DeserializeObject<GameInfo>(File.ReadAllText("gameInfo.json"), new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });

        shipScript.transform.position = gameInfo.playerPosition.UnityVector;
        shipScript.transform.rotation = Quaternion.Euler(0, gameInfo.playerOrientation, 0);

        shipScript.peasantInfoList = gameInfo.peasantList;
        shipScript.animals = new int[Enum.GetValues(typeof(ResourceScript.AnimalType)).Length];
        shipScript.animalList = new List<AnimalInfo>();
        foreach (AnimalInfo animalInfo in gameInfo.animalList)
        {
            shipScript.AddAnimal(animalInfo);
        }
        shipScript.inventoryScript = gameInfo.inventoryScript;

        islandManager.seed = gameInfo.seed;
        GetComponent<IslandGenerator>().LoadIslands(gameInfo.islandList);
    }

    private void StartGame()
    {
        islandManager.TryToGenerateIsland();
        shipScript.AddDefaultElements();
    }
}

[System.Serializable]
public class GameInfo
{
    public int seed;
    public SerializableVector3 playerPosition;
    public int playerOrientation;
    public List<PeasantInfo> peasantList = new List<PeasantInfo>();
    public List<AnimalInfo> animalList = new List<AnimalInfo>();
    public InventoryScript inventoryScript;
    public List<IslandInfo> islandList = new List<IslandInfo>();
}

[System.Serializable]
public class SerializableVector3
{
    public float x;
    public float y;
    public float z;

    [JsonIgnore]
    public Vector3 UnityVector
    {
        get
        {
            return new Vector3(x, y, z);
        }
    }

    public SerializableVector3(Vector3 v)
    {
        x = v.x;
        y = v.y;
        z = v.z;
    }
}


[System.Serializable]
public class SerializableColor
{
    public float r;
    public float g;
    public float b;

    [JsonIgnore]
    public Color UnityColor
    {
        get
        {
            return new Color(r, g, b);
        }
    }

    public SerializableColor(Color c)
    {
        r = c.r;
        g = c.g;
        b = c.b;
    }
}

[System.Serializable]
public class SerializableVector2
{
    public float x;
    public float y;

    [JsonIgnore]
    public Vector2 UnityVector
    {
        get
        {
            return new Vector2(x, y);
        }
    }

    public SerializableVector2(Vector2 v)
    {
        x = v.x;
        y = v.y;
    }

    public static List<SerializableVector2> GetSerializableList(List<Vector2> vList)
    {
        List<SerializableVector2> list = new List<SerializableVector2>(vList.Count);
        for (int i = 0; i < vList.Count; i++)
        {
            list.Add(new SerializableVector2(vList[i]));
        }
        return list;
    }

    public static List<Vector2> GetSerializableList(List<SerializableVector2> vList)
    {
        List<Vector2> list = new List<Vector2>(vList.Count);
        for (int i = 0; i < vList.Count; i++)
        {
            list.Add(vList[i].UnityVector);
        }
        return list;
    }

    public static SerializableVector2[] GetSerializableArray(Vector2[] vList)
    {
        SerializableVector2[] list = new SerializableVector2[vList.Length];
        for (int i = 0; i < vList.Length; i++)
        {
            list[i] = new SerializableVector2(vList[i]);
        }
        return list;
    }

    public static Vector2[] GetSerializableArray(SerializableVector2[] vList)
    {
        Vector2[] list = new Vector2[vList.Length];
        for (int i = 0; i < vList.Length; i++)
        {
            list[i] = vList[i].UnityVector;
        }
        return list;
    }
}
