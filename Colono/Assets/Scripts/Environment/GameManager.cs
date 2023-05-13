using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System;
using Newtonsoft.Json.Linq;
using System.Globalization;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static string gameName;
    public float timePlayed;
    public static int seed;
    public static GameManager Instance { get; private set; }
    public Transform islandsTransform;
    public List<IslandScript> islandList = new List<IslandScript>();

    private IslandGenerator islandGenerator;
    public InventoryEditor inventoryEditor;

    private bool isPlayerNearIsland = false;
    public bool isInIsland = false;

    public float distanceToInteract = 10f;
    public float distanceToCheckIfCanInteract = 50f;
    public float distanceToDoSeaStuff = 80f;
    public float distanceForEnemyToAppear = 100f;
    public float distanceBetweenIslands = 200f;

    public bool[] discoveredCrops;
    
    public IslandScript closestIsland;
    public IslandSelectionScript islandSelectionScript;
    public IslandCellScript islandCellScript;
    public PenScript buildingInterior;

    private void Awake()
    {
        //Time.timeScale = 5;
        Instance = this;
    }

    private void Start()
    {
        islandCellScript = GetComponent<IslandCellScript>();
        islandCellScript.enabled = false;

        islandSelectionScript = GetComponent<IslandSelectionScript>();
        islandSelectionScript.enabled = false;

        islandGenerator = GetComponent<IslandGenerator>();

        /*if(MenuManager.loadGame) LoadGame();
        else StartGame();*/
        StartGame();

        inventoryEditor.shipInventoryScript = ShipScript.Instance.shipInterior.inventoryScript;
    }

    private void Update()
    {
        timePlayed += Time.deltaTime;

        if(Input.GetKeyDown(KeyCode.Escape) && !islandCellScript.enabled)
        {
            CanvasScript.Instance.TogglePauseMenu();
        }

        if (isInIsland) return;

        Vector3 shipPosition = ShipScript.Instance.transform.position;

        float distanceToEnemyShip = HandleEnemyShipPosition(shipPosition);
        FindClosestIsland(shipPosition, distanceToEnemyShip);
        FindClosestPointInClosestIsland(shipPosition);
    }

    private float HandleEnemyShipPosition(Vector3 shipPosition)
    {
        Vector3 enemyShipPosition = EnemyController.Instance.enemyStatus == EnemyController.EnemyStatus.StandBy ? ShipScript.Instance.enemyShipTransform.position : EnemyController.Instance.transform.position;
        float distanceToEnemyShip = Vector3.Distance(shipPosition, enemyShipPosition);

        if (EnemyController.Instance.enemyStatus == EnemyController.EnemyStatus.Trading)
        {
            if (distanceToEnemyShip < distanceToInteract)
            {
                CanvasScript.Instance.CanTrade();
            }
            else if(distanceToEnemyShip < distanceToCheckIfCanInteract)
            {
                CanvasScript.Instance.CannotTrade();
            }
            else
            {
                EnemyController.Instance.HideFromMap();
            }
        }
        else
        {
            CanvasScript.Instance.CannotTrade();

            float minDistance = -1;
            foreach (IslandScript islandScript in islandList)
            {
                float distanceToIsland = Vector3.Distance(islandScript.transform.position, enemyShipPosition);
                if (minDistance == -1 || distanceToIsland < minDistance)
                {
                    minDistance = distanceToIsland;
                }
            }

            if (EnemyController.Instance.enemyStatus == EnemyController.EnemyStatus.StandBy)
            {
                if (minDistance > distanceForEnemyToAppear)
                {
                    EnemyController.Instance.transform.position = enemyShipPosition;
                    EnemyController.Instance.Initialize();
                }
            }
            else // El vaixell enemic està en combat
            {
                if (minDistance < distanceToDoSeaStuff)
                {
                    EnemyController.Instance.enemyStatus = EnemyController.EnemyStatus.Fleeing;
                }
                if (distanceToEnemyShip > distanceToCheckIfCanInteract)
                {
                    EnemyController.Instance.HideFromMap();
                }
            }

        }

        return Vector3.Distance(shipPosition, EnemyController.Instance.transform.position);
    }

    private void FindClosestIsland(Vector3 shipPosition, float distanceToEnemyShip)
    {
        float minDistanceToIsland = -1;
        Vector3 nextIslandPosition = ShipScript.Instance.nextIslandTransform.position;
        float minDistanceToNextIsland = -1;
        IslandScript c = null;

        foreach (IslandScript islandScript in islandList)
        {
            float distanceToIsland = Vector3.Distance(islandScript.transform.position, shipPosition);
            if (c == null || distanceToIsland < minDistanceToIsland)
            {
                minDistanceToIsland = distanceToIsland;
                c = islandScript;
            }

            float distanceToNextIsland = Vector3.Distance(islandScript.transform.position, nextIslandPosition);
            if (minDistanceToNextIsland == -1 || distanceToNextIsland < minDistanceToNextIsland)
            {
                minDistanceToNextIsland = distanceToNextIsland;
            }
        }

        if (minDistanceToNextIsland > distanceBetweenIslands)
        {
            islandGenerator.GenerateIsland(new Vector2(nextIslandPosition.x, nextIslandPosition.z));
        }
        else if(minDistanceToIsland > distanceToDoSeaStuff && distanceToEnemyShip > distanceToCheckIfCanInteract)
        {
            CanvasScript.Instance.CanFish();
        }
        else
        {
            CanvasScript.Instance.CannotFish();
        }

        closestIsland = c;
    }

    private void FindClosestPointInClosestIsland(Vector3 shipPosition)
    {
        if (closestIsland == null || Vector3.Distance(shipPosition, closestIsland.transform.position) > distanceToCheckIfCanInteract)
        {
            PlayerIsFarFromIsland();
            return;
        }

        if (ShipScript.Instance.FindEntryPosition(distanceToInteract))
        {
            PlayerIsNearIsland();
        }
        else
        {
            PlayerIsFarFromIsland();
        }
    }

    private void PlayerIsNearIsland()
    {
        if (!isPlayerNearIsland)
        {
            isPlayerNearIsland = true;
            CanvasScript.Instance.PlayerIsNearIsland(closestIsland);
        }
    }

    private void PlayerIsFarFromIsland()
    {
        if (isPlayerNearIsland)
        {
            CanvasScript.Instance.PlayerIsFarFromIsland();
            isPlayerNearIsland = false;
        }
    }

    public void Dock()
    {
        isInIsland = true;
        islandCellScript.islandScript = closestIsland;
        islandSelectionScript.enabled = true;

        CameraScript.Instance.SetIslandCamera(closestIsland.transform.position);
        CanvasScript.Instance.Dock();

        ShipScript.Instance.Dock(closestIsland);
    }

    public void DisableIslandCellScript()
    {
        islandCellScript.DestroyAllCells();
        islandCellScript.enabled = false;
        islandSelectionScript.enabled = true;
    }

    public void SelectPeasant(PeasantScript peasantScript)
    {
        CameraScript.Instance.SetCameraToObject(peasantScript.position);
        CanvasScript.Instance.ShowPeasantDetails(peasantScript);
    }

    public void UnselectPeasant()
    {
        islandSelectionScript.UnselectPeasant();
        CanvasScript.Instance.HidePeasantDetails();
    }

    public void TopButtonClick(TopButtonScript topButtonScript)
    {
        CanvasScript.Instance.ChooseTopButton();
        islandSelectionScript.enabled = false;
        islandCellScript.enabled = true;
        switch (topButtonScript.function)
        {
            case IslandCellScript.SelectFunction.PlaceBuilding: islandCellScript.ChooseBuilding(topButtonScript.buildingScript); break;
            case IslandCellScript.SelectFunction.CreateEnclosure: islandCellScript.ChooseEnclosure(topButtonScript.enclosureType); break;
            default: islandCellScript.ManageItems(topButtonScript.function); break;
        }
    }

    public bool CheckIfCropIsNew(int cropType)
    {
        if (discoveredCrops[cropType]) return false;

        discoveredCrops[cropType] = true;

        CanvasScript.Instance.CropIsDiscovered(cropType);

        return true;
    }

    public void RemoveConstruction()
    {
        ConstructionScript constructionScript = islandSelectionScript.selectedConstruction;
        constructionScript.FinishUpBusiness();
        closestIsland.RemoveConstruction(constructionScript);
        CanvasScript.Instance.HideConstructionDetails();
    }

    public void SelectConstruction(ConstructionScript constructionScript)
    {
        islandSelectionScript.selectedConstruction = constructionScript;
        CameraScript.Instance.SetCameraToObject(constructionScript.constructionType == ConstructionScript.ConstructionType.Ship?
                                                constructionScript.transform.position : constructionScript.entry.position);
        CanvasScript.Instance.ShowConstructionDetails(constructionScript);
    }

    public void UnselectConstrucion()
    {
        islandSelectionScript.UnselectConstruction();
        CanvasScript.Instance.HideConstructionDetails();
    }

    public void Sail()
    {
        isInIsland = false;

        islandSelectionScript.enabled = false;
        islandCellScript.enabled = false;

        CameraScript.Instance.ResetPlayerCamera();
        CanvasScript.Instance.Sail();

        ShipScript.Instance.Sail();
    }

    public void SaveGame()
    {
        GameInfo gameInfo = new GameInfo();
        gameInfo.timePlayed = timePlayed;
        gameInfo.seed = seed;
        gameInfo.shipScript = ShipScript.Instance;
        gameInfo.islandList = islandList;

        string json = JsonConvert.SerializeObject(gameInfo, Formatting.Indented, MenuManager.serializerSettings);
        File.WriteAllText(MenuManager.gameSavesPath + "/" + gameName + ".json", json);
    }

    public void QuitGame()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadGame()
    {
        GameInfo gameInfo = JsonConvert.DeserializeObject<GameInfo>(File.ReadAllText(MenuManager.gameSavesPath + "/" + gameName + ".json"), MenuManager.serializerSettings);

        timePlayed = gameInfo.timePlayed;
        seed = gameInfo.seed;

        EnemyController.Instance.Initialize(gameInfo.enemyController);
        EnemyShipScript enemyShipScript = EnemyController.Instance.GetComponent<EnemyShipScript>();
        enemyShipScript.Initialize(gameInfo.enemyShipScript);

        ShipScript.Instance.InitializeShip(gameInfo.shipScript);
        closestIsland = islandGenerator.LoadIslands(gameInfo.islandList, gameInfo.shipScript.position);

        foreach (PeasantScript peasantInfo in gameInfo.shipScript.peasantList)
        {
            PeasantScript peasantScript = Instantiate(ResourceScript.Instance.GetPeasantPrefab(peasantInfo.peasantType, peasantInfo.peasantGender),
                peasantInfo.position, Quaternion.Euler(0, peasantInfo.orientation, 0), closestIsland.npcsTransform);
            peasantScript.islandScript = closestIsland;
            peasantScript.InitializePeasant(peasantInfo);
            closestIsland.peasantList.Add(peasantScript);
            if (peasantScript.peasantType == PeasantScript.PeasantType.Adult)
            {
                closestIsland.GetNextPendingTask((PeasantAdultScript)peasantScript);
            }
            else peasantScript.UpdateTask();
        }
    }

    private void StartGame()
    {
        islandGenerator.GenerateIsland(new Vector2(0, 40));

        EnemyController.Instance.Initialize();
        EnemyShipScript enemyShipScript = EnemyController.Instance.GetComponent<EnemyShipScript>();
        enemyShipScript.Initialize();

        discoveredCrops = new bool[Enum.GetValues(typeof(ResourceScript.CropType)).Length];
        for (int i = 0; i < discoveredCrops.Length/2; i++)
        {
            discoveredCrops[i] = true;
        }

        ShipScript.Instance.AddDefaultElements();
    }

    public void GameOver()
    {
        CameraScript.Instance.canMove = false;
    }

    public static object ConvertStringToVector(string sValue)
    {
        // Remove the parentheses
        string sVector = sValue.Substring(1, sValue.Length - 2);

        // split the items
        string[] sArray = sVector.Split(',');

        if (sArray.Length == 2)
        {
            return new Vector2(float.Parse(sArray[0], CultureInfo.InvariantCulture.NumberFormat),
                                float.Parse(sArray[1], CultureInfo.InvariantCulture.NumberFormat));
        }
        else
        {
            return new Vector3(float.Parse(sArray[0], CultureInfo.InvariantCulture.NumberFormat),
                                float.Parse(sArray[1], CultureInfo.InvariantCulture.NumberFormat),
                                float.Parse(sArray[2], CultureInfo.InvariantCulture.NumberFormat));
        }
    }

    public static Dictionary<string, T> CreateDictionaryWithType<T>(Type valueType)
    {
        return new Dictionary<string, T>();
    }
}

[Serializable]
public class GameInfo
{
    public float timePlayed;
    public int seed;
    public PlayerController playerController;
    public EnemyController enemyController;
    public EnemyShipScript enemyShipScript;
    public ShipScript shipScript;
    public List<IslandScript> islandList = new List<IslandScript>();
}

public class VectorConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Vector2);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        string sValue = (string)reader.Value;

        return GameManager.ConvertStringToVector(sValue);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}

public class VectorDictionaryConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Dictionary<,>) &&
               objectType.GetGenericArguments()[0] == typeof(Vector2);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        int startIndex = reader.Path.LastIndexOf('.') + 1;
        string variableName = reader.Path.Substring(startIndex, reader.Path.Length - startIndex);
        /*Type valueType;
        switch (variableName)
        {
            case "itemDictionary": valueType = typeof(ItemScript); break;
            case "patchDictionary": valueType = typeof(PatchScript); break;
            case "cropDictionary": valueType = typeof(ResourceScript.CropType); break;
        }*/

        var jObject = JObject.Load(reader);

        if (variableName == "itemDictionary")
        {
            Dictionary<Vector2, ItemScript> result = new Dictionary<Vector2, ItemScript>();
            foreach (var jProperty in jObject.Properties())
            {
                Vector2 vector2 = (Vector2)GameManager.ConvertStringToVector(jProperty.Name);
                ItemScript value = (ItemScript)jProperty.Value.ToObject(objectType.GetGenericArguments()[1], serializer);
                result.Add(vector2, value);
            }

            return result;
        }
        else if (variableName == "patchDictionary")
        {
            Dictionary<Vector2, PatchScript> result = new Dictionary<Vector2, PatchScript>();
            foreach (var jProperty in jObject.Properties())
            {
                Vector2 vector2 = (Vector2)GameManager.ConvertStringToVector(jProperty.Name);
                PatchScript value = (PatchScript)jProperty.Value.ToObject(objectType.GetGenericArguments()[1], serializer);
                result.Add(vector2, value);
            }

            return result;
        }
        else //cropDictionary
        {
            Dictionary<Vector2, ResourceScript.CropType> result = new Dictionary<Vector2, ResourceScript.CropType>();
            foreach (var jProperty in jObject.Properties())
            {
                Vector2 vector2 = (Vector2)GameManager.ConvertStringToVector(jProperty.Name);
                ResourceScript.CropType value = (ResourceScript.CropType)jProperty.Value.ToObject(objectType.GetGenericArguments()[1], serializer);
                result.Add(vector2, value);
            }

            return result;
        }
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        int startIndex = writer.Path.LastIndexOf('.') + 1;
        string variableName = writer.Path.Substring(startIndex, writer.Path.Length - startIndex);

        writer.WriteStartObject();

        if (variableName == "itemDictionary")
        {
            Dictionary<Vector2, ItemScript> dictionary = (Dictionary<Vector2, ItemScript>)value;
            foreach (KeyValuePair<Vector2, ItemScript> pair in dictionary)
            {
                writer.WritePropertyName(pair.Key.ToString());
                serializer.Serialize(writer, pair.Value);
            }
        }
        else if (variableName == "patchDictionary")
        {
            Dictionary<Vector2, PatchScript> dictionary = (Dictionary<Vector2, PatchScript>)value;
            foreach (KeyValuePair<Vector2, PatchScript> pair in dictionary)
            {
                writer.WritePropertyName(pair.Key.ToString());
                serializer.Serialize(writer, pair.Value);
            }
        }
        else
        {
            Dictionary<Vector2, ResourceScript.CropType> dictionary = (Dictionary<Vector2, ResourceScript.CropType>)value;
            foreach (KeyValuePair<Vector2, ResourceScript.CropType> pair in dictionary)
            {
                writer.WritePropertyName(pair.Key.ToString());
                serializer.Serialize(writer, pair.Value);
            }
        }

        writer.WriteEndObject();
    }
}

public class PeasantListConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return typeof(List<PeasantScript>).IsAssignableFrom(objectType);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        JArray array = JArray.Load(reader);

        List<PeasantScript> list = new List<PeasantScript>();

        foreach (JObject obj in array)
        {
            PeasantScript peasantScript = null;
            int peasantType = (int)obj["peasantType"];
            if(peasantType == (int)PeasantScript.PeasantType.Child)
            {
                peasantScript = obj.ToObject<PeasantChildScript>(serializer);
            }
            else
            {
                peasantScript = obj.ToObject<PeasantAdultScript>(serializer);
            }
            list.Add(peasantScript);
        }

        return list;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        int startIndex = writer.Path.LastIndexOf('[') + 1;
        int endIndex = writer.Path.LastIndexOf(']');
        int lastPoint = writer.Path.LastIndexOf('.');
        int constructionIndex = endIndex != lastPoint - 1 ? -1 : int.Parse(writer.Path.Substring(startIndex, endIndex - startIndex));

        List<PeasantScript> list = (List<PeasantScript>)value;

        JArray array = new JArray();

        foreach (PeasantScript peasantScript in list)
        {
            if (constructionIndex != -1 && peasantScript.islandScript.constructionList[constructionIndex].isService) break;

            if(peasantScript.cabin != null)
            {
                peasantScript.cabinIndex = peasantScript.islandScript.constructionList.IndexOf(peasantScript.cabin);
            }
            else
            {
                peasantScript.cabinIndex = -1;
            }

            if (peasantScript.tavern != null)
            {
                peasantScript.tavernIndex = peasantScript.islandScript.constructionList.IndexOf(peasantScript.tavern);
            }
            else
            {
                peasantScript.tavernIndex = -1;
            }

            JObject peasantScriptObject;
            if (peasantScript.peasantType == PeasantScript.PeasantType.Adult)
            {
                PeasantAdultScript peasantAdultScript = (PeasantAdultScript)peasantScript;
                if(peasantAdultScript.task != null)
                {
                    peasantAdultScript.taskCell = peasantAdultScript.task.cell;
                }

                peasantScriptObject = JObject.FromObject(peasantAdultScript, serializer);
            }
            else
            {
                peasantScriptObject = JObject.FromObject(peasantScript, serializer);
            }

            array.Add(peasantScriptObject);

        }

        array.WriteTo(writer);
    }
}

public class VectorArrayConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return typeof(Vector2[]).IsAssignableFrom(objectType);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        JArray array = JArray.Load(reader);

        Vector2[] vectorArray = new Vector2[array.Count];

        int i = 0;
        foreach (JObject obj in array)
        {
            vectorArray[i] = (Vector2)GameManager.ConvertStringToVector(obj.ToString());
            i++;
        }

        return vectorArray;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        Vector2[] vectorArray = (Vector2[])value;

        JArray array = new JArray();

        foreach (Vector2 vector in vectorArray)
        {
            array.Add(vector.ToString());
        }

        array.WriteTo(writer);
    }
}

public class ColorHandler : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Color);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        ColorUtility.TryParseHtmlString("#" + reader.Value, out Color loadedColor);
        return loadedColor;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        string val = ColorUtility.ToHtmlStringRGB((Color)value);
        writer.WriteValue(val);
    }
}