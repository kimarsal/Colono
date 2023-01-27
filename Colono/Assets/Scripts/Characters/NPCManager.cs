using UnityEngine;
using UnityEngine.AI;

public static class NPCManager
{
    public static bool CheckIfClosest(Vector3 destination, NavMeshAgent navMeshAgent, float minDistance, out float distance)
    {
        NavMeshPath path = new NavMeshPath();
        if (navMeshAgent.CalculatePath(destination, path)) //Si el camí és possible
        {
            distance = Vector3.Distance(navMeshAgent.transform.position, destination);
            if (minDistance == -1 || distance < minDistance) //Si és el més proper
            {
                return true;
            }
        }
        distance = 0;
        return false;
    }

    public static Vector3 GetRandomPoint(Vector3 originPos)
    {
        NavMeshHit hit;
        do
        {
            Vector2 randomPos = Random.insideUnitCircle * IslandGenerator.mapChunkSize / 4;
            NavMesh.SamplePosition(originPos + new Vector3(randomPos.x, 0, randomPos.y), out hit, 5, NavMesh.AllAreas);
        }
        while (hit.position == Vector3.positiveInfinity);
        return hit.position;
    }

    public static Vector3 GetRandomPointWithinRange(Vector3 minPos, Vector3 maxPos)
    {
        Vector3 randomPos = new Vector3(Random.Range(minPos.x, maxPos.x), Random.Range(minPos.y, maxPos.y), Random.Range(minPos.z, maxPos.z));
        return GetClosestPointInNavMesh(randomPos);
    }

    public static Vector3 GetClosestPointInNavMesh(Vector3 pos)
    {
        NavMeshHit hit;
        NavMesh.SamplePosition(pos, out hit, 5, NavMesh.AllAreas);
        return hit.position;
    }
}
