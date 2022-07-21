using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrategyManager : MonoBehaviour
{
    public GameObject pawnPrefab;
    public int pawnNum;
    private List<GameObject> pawns;
    private GameObject[] areas;
    private MinMaxPos[] areasMinMaxPos;
    private StrategyArea.StrategyAreaEnum selectedArea = StrategyArea.StrategyAreaEnum.Ground;
    private int areasNum;
    private StrategyArea settlementScript;

    private Camera mainCamera;

    void Start()
    {
        areasNum = System.Enum.GetNames(typeof(StrategyArea.StrategyAreaEnum)).Length;
        areas = new GameObject[areasNum];
        areasMinMaxPos = new MinMaxPos[areasNum];
        foreach (StrategyArea.StrategyAreaEnum area in System.Enum.GetValues(typeof(StrategyArea.StrategyAreaEnum)))
        {
            GameObject go = GameObject.Find(area.ToString());
            areas[(int)area] = go;
            areasMinMaxPos[(int)area] = GetMinMaxPos(go.transform.position, go.transform.lossyScale);
        }

        settlementScript = areas[(int)StrategyArea.StrategyAreaEnum.Settlement].GetComponent<StrategyArea>();
        settlementScript.pawnNum = pawnNum;
        settlementScript.UpdatePawnNumText();

        pawns = new List<GameObject>();
        MinMaxPos settlementMinMax = areasMinMaxPos[(int)StrategyArea.StrategyAreaEnum.Settlement];
        for (int i = 0; i < pawnNum; i++)
        {
            GameObject pawn = Instantiate(pawnPrefab, new Vector3(Random.Range(settlementMinMax.minX, settlementMinMax.maxX), 1f, Random.Range(settlementMinMax.minZ, settlementMinMax.maxZ)), pawnPrefab.transform.rotation);
            pawns.Add(pawn);
        }

        mainCamera = GameObject.Find("MainCamera").GetComponent<Camera>();
    }

    public Vector3 GetNextPos(StrategyArea.StrategyAreaEnum area)
    {
        MinMaxPos minMaxPos = areasMinMaxPos[(int)area];
        return new Vector3(Random.Range(minMaxPos.minX, minMaxPos.maxX), 1f, Random.Range(minMaxPos.minZ, minMaxPos.maxZ));
    }

    public void AreaClick(StrategyArea.StrategyAreaEnum area)
    {
        if(area != selectedArea)
        {
            for(int i = 0; i < areasNum; i++)
            {
                StrategyArea areaScript = areas[i].GetComponent<StrategyArea>();
                if (i == (int)area)
                {
                    areaScript.ToggleUI(true);
                }
                else
                {
                    areaScript.ToggleUI(false);
                }
            }
            selectedArea = area;
        }
    }

    public void SendPawnToArea(StrategyArea.StrategyAreaEnum area, bool going)
    {
        bool found = false;
        for (int i = 0; i < pawns.Count && !found; i++)
        {
            StrategyPawn pawnScript = pawns[i].GetComponent<StrategyPawn>();
            if (going)
            {
                if (pawnScript.area == StrategyArea.StrategyAreaEnum.Settlement)
                {
                    pawnScript.area = area;
                    pawnScript.GetNextPos();
                    found = true;
                }
            }
            else
            {
                if (pawnScript.area == area)
                {
                    pawnScript.area = StrategyArea.StrategyAreaEnum.Settlement;
                    pawnScript.GetNextPos();
                    found = true;
                }
            }
        }

        settlementScript.pawnNum += going ? -1 : 1;
        settlementScript.UpdatePawnNumText();
    }

    public int GetAvailablePawns()
    {
        return areas[(int)StrategyArea.StrategyAreaEnum.Settlement].GetComponent<StrategyArea>().pawnNum;
    }

    MinMaxPos GetMinMaxPos(Vector3 pos, Vector3 scale)
    {
        MinMaxPos minMaxPos;
        minMaxPos.minX = pos.x - scale.x / 2;
        minMaxPos.maxX = pos.x + scale.x / 2;
        minMaxPos.minZ = pos.z - scale.z / 2;
        minMaxPos.maxZ = pos.z + scale.z / 2;
        return minMaxPos;
    }

    struct MinMaxPos
    {
        public float minX;
        public float maxX;
        public float minZ;
        public float maxZ;
    }

    /*private void DetectObject()
    {
        Ray ray = camera.ScreenPointToRay();
    }*/
}
