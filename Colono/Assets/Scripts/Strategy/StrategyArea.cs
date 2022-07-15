using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class StrategyArea : MonoBehaviour, IPointerClickHandler
{
    public enum StrategyAreaEnum {Settlement, Forest, Garden, Mine, Ground};

    private StrategyManager script;
    public StrategyAreaEnum area;
    public TextMeshProUGUI areaTitle;
    public GameObject areaUI;
    public TextMeshProUGUI pawnNumText;
    public int pawnNum;

    private void Start()
    {
        script = GameObject.Find("StrategyManager").GetComponent<StrategyManager>();
        areaUI.SetActive(false);
        areaTitle.text = area.ToString();
    }

    private void Update()
    {
        areaUI.transform.position = Camera.main.WorldToScreenPoint(transform.position) + new Vector3(0, 50f, 0);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        script.AreaClick(area);
    }

    public void ToggleUI(bool enabled)
    {
        if (area != StrategyAreaEnum.Ground) areaUI.SetActive(enabled);
    }

    public void ManagePawns(bool adding)
    {
        if(adding && script.GetAvailablePawns() > 0 || !adding && pawnNum > 0)
        {
            script.SendPawnToArea(area, adding);
            pawnNum += adding ? 1 : -1;
            UpdatePawnNumText();
        }
    }

    public void UpdatePawnNumText()
    {
        pawnNumText.text = pawnNum.ToString();
    }
}
