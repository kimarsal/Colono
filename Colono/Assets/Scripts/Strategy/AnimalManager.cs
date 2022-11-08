using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AnimalManager : MonoBehaviour
{
    static Vector3 minPos = new Vector3(-50, 0, -50);
    static Vector3 maxPos = new Vector3(50, 0, 50);

    public static Vector3 GetRandomPointWithinRange()
    {
        NavMeshHit hit;
        Vector3 randomPos = new Vector3(Random.Range(minPos.x, maxPos.x), Random.Range(minPos.y, maxPos.y), Random.Range(minPos.z, maxPos.z));
        NavMesh.SamplePosition(randomPos, out hit, 5, NavMesh.AllAreas);
        return hit.position;
    }
}
