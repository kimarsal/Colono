using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrategyPawn : MonoBehaviour
{
    public float speed;
    private Vector3 nextPos;
    public StrategyManager script;
    public StrategyArea.StrategyAreaEnum area = StrategyArea.StrategyAreaEnum.Settlement;

    void Start()
    {
        script = GameObject.Find("StrategyManager").GetComponent<StrategyManager>();
        nextPos = transform.position;
    }

    void Update()
    {
        if(Vector3.Distance(transform.position, nextPos) < 0.001f)
        {
            GetNextPos();
        }
        transform.position = Vector3.MoveTowards(transform.position, nextPos, speed * Time.deltaTime);
    }

    public void GetNextPos()
    {
        nextPos = script.GetNextPos(area);
    }
}
