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
    private bool isInTitleScreen = true;
    private bool isInTutorialScreen;
    public static bool loadGame;
    [SerializeField] private MenuScreenScript titleScreen;
    [SerializeField] private MenuScreenScript mainMenuScreen;
    [SerializeField] private MenuScreenScript newGameScreen;
    [SerializeField] private MenuScreenScript loadGameScreen;
    [SerializeField] private MenuScreenScript menuBackground;
    [SerializeField] private GameObject tutorialScreen;
    [SerializeField] private GameObject loadingScreen;

    [SerializeField] private TMP_InputField nameText;
    [SerializeField] private TMP_InputField seedText;
    [SerializeField] private Button startNewGameButton;
    [SerializeField] private Transform gameListTransform;
    [SerializeField] private GameRowScript gameRowPrefab;

    public static string gameSavesPath = "GameSaves";
    public static JsonSerializerSettings serializerSettings;
    private List<string> usedNames = new List<string>();

    [SerializeField] private AudioSource soundSource;
    [SerializeField] private AudioClip pageFlipClip;

    private void Start()
    {
        serializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto/*,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore*/
        };

        titleScreen.gameObject.SetActive(true);
        mainMenuScreen.gameObject.SetActive(true);
        newGameScreen.gameObject.SetActive(true);
        loadGameScreen.gameObject.SetActive(true);
        menuBackground.gameObject.SetActive(true);

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
        if (Input.anyKeyDown)
        {
            if (isInTitleScreen)
            {
                isInTitleScreen = false;
                titleScreen.Move();
                mainMenuScreen.Move();
                menuBackground.Move();

                soundSource.PlayOneShot(pageFlipClip);
            }
            else if (isInTutorialScreen)
            {
                isInTutorialScreen = false;
                SkipTutorial();
            }
        }
    }

    public void NewGame()
    {
        mainMenuScreen.Move();
        newGameScreen.Move();

        if (seedText.text == "") RandomSeed();

        soundSource.PlayOneShot(pageFlipClip);
    }

    public void LoadGame()
    {
        mainMenuScreen.Move();
        loadGameScreen.Move();

        soundSource.PlayOneShot(pageFlipClip);
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
        mainMenuScreen.Move(false);
        newGameScreen.Move(false);

        soundSource.PlayOneShot(pageFlipClip);
    }

    public void BackFromLoadGame()
    {
        mainMenuScreen.Move(false);
        loadGameScreen.Move(false);

        soundSource.PlayOneShot(pageFlipClip);
    }

    public void OnTextChange()
    {
        startNewGameButton.interactable = nameText.text != "" && !usedNames.Contains(nameText.text) && seedText.text != "";
    }

    public void RandomSeed()
    {
        seedText.text = UnityEngine.Random.Range(0, 100000).ToString();
    }

    public void StartNewGame()
    {
        GameManager.gameName = nameText.text;
        GameManager.seed = int.Parse(seedText.text);
        loadGame = false;
        tutorialScreen.SetActive(true);
        isInTutorialScreen = true;

        soundSource.PlayOneShot(pageFlipClip);
    }

    public void SkipTutorial()
    {
        soundSource.PlayOneShot(pageFlipClip);

        tutorialScreen.SetActive(false);
        loadingScreen.SetActive(true);
        SceneManager.LoadScene("MainScene");
    }

    public void StartPreviousGame(string gameName)
    {
        soundSource.PlayOneShot(pageFlipClip);

        GameManager.gameName = gameName;
        loadGame = true;
        loadingScreen.SetActive(true);
        SceneManager.LoadScene("MainScene");
    }
}
