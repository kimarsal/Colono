using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameRowScript : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI gameNameText;
    [SerializeField] private TextMeshProUGUI gameTimeText;
    private MenuManager menuManager;

    public void SetGameRow(MenuManager menuManager, string gameName, string gameTime)
    {
        this.menuManager = menuManager;
        gameNameText.text = gameName;
        gameTimeText.text = gameTime;
    }

    public void SelectGameRow()
    {
        menuManager.StartPreviousGame(gameNameText.text);
    }
}
