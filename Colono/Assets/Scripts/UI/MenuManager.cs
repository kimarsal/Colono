using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    private enum MenuScreenEnum { TitleScreen, MainMenu, NewGame, LoadGame }
    private MenuScreenEnum menuScreen;
    public static bool loadGame;
    [SerializeField] private MenuScreenScript[] menuScreenList;
    [SerializeField] private TMP_InputField nameText;
    [SerializeField] private TMP_InputField seedText;
    [SerializeField] private Button startNewGameButton;
    [SerializeField] private Transform gameListTransform;
    [SerializeField] private GameRowScript gameRowPrefab;

    public static string gameSavesPath = "GameSaves";
    public static JsonSerializerSettings serializerSettings;
    private List<string> usedNames = new List<string>();

    private void Start()
    {
        serializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto/*,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore*/
        };

        foreach (MenuScreenScript menuScreenScript in menuScreenList)
        {
            menuScreenScript.gameObject.SetActive(true);
        }

        if (!Directory.Exists(gameSavesPath))
        {
            Directory.CreateDirectory(gameSavesPath);
        }

        FileInfo[] files = new DirectoryInfo(gameSavesPath).GetFiles();
        foreach (FileInfo file in files)
        {
            try
            {
                GameInfo gameInfo = JsonConvert.DeserializeObject<GameInfo>(File.ReadAllText(gameSavesPath + "/" + file.Name), serializerSettings);

                string fileName = file.Name.Substring(0, file.Name.IndexOf('.'));
                usedNames.Add(fileName);

                TimeSpan time = TimeSpan.FromSeconds(gameInfo.timePlayed);
                string time_s = time.ToString(@"hh\:mm\:ss");
                Instantiate(gameRowPrefab, gameListTransform).SetGameRow(this, fileName, time_s);
            }
            catch(Exception ex)
            {
                Debug.Log("File corrupted: " + ex.ToString());
            }
        }
    }

    private void Update()
    {
        if (Input.anyKeyDown && menuScreen == MenuScreenEnum.TitleScreen)
        {
            menuScreenList[(int)MenuScreenEnum.TitleScreen].Move();
            menuScreenList[(int)MenuScreenEnum.MainMenu].Move();
            menuScreen = MenuScreenEnum.MainMenu;
        }
    }

    public void NewGame()
    {
        menuScreenList[(int)MenuScreenEnum.MainMenu].Move();
        menuScreenList[(int)MenuScreenEnum.NewGame].Move();
        menuScreen = MenuScreenEnum.NewGame;

        if (seedText.text == "") RandomSeed();
    }

    public void LoadGame()
    {
        menuScreenList[(int)MenuScreenEnum.MainMenu].Move();
        menuScreenList[(int)MenuScreenEnum.LoadGame].Move();
        menuScreen = MenuScreenEnum.LoadGame;
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    public void BackFromNewGame()
    {
        menuScreenList[(int)MenuScreenEnum.MainMenu].Move(false);
        menuScreenList[(int)MenuScreenEnum.NewGame].Move(false);
        menuScreen = MenuScreenEnum.MainMenu;
    }

    public void BackFromLoadGame()
    {
        menuScreenList[(int)MenuScreenEnum.MainMenu].Move(false);
        menuScreenList[(int)MenuScreenEnum.LoadGame].Move(false);
        menuScreen = MenuScreenEnum.MainMenu;
    }

    public void OnTextChange()
    {
        startNewGameButton.interactable = nameText.text != "" && !usedNames.Contains(nameText.text) && seedText.text != "";
    }

    public void RandomSeed()
    {
        seedText.text = UnityEngine.Random.Range(0, 10000).ToString();
    }

    public void StartNewGame()
    {
        GameManager.gameName = nameText.text;
        GameManager.seed = int.Parse(seedText.text);
        loadGame = false;
        SceneManager.LoadScene("MainScene");
    }

    public void StartPreviousGame(string gameName)
    {
        GameManager.gameName = gameName;
        loadGame = true;
        SceneManager.LoadScene("MainScene");
    }
}
