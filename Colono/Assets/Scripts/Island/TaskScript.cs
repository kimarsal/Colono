using Newtonsoft.Json;
using UnityEngine;

[JsonObject(MemberSerialization.OptIn)]
public abstract class TaskScript : MonoBehaviour
{
    public TaskSourceInterface taskSourceScript;
    public enum TaskType { Item, Patch }
    [JsonProperty] public Vector3 center;
    [JsonProperty] public TaskType taskType;

    public PeasantAdultScript peasantAdultScript;
    [JsonProperty] public int peasantIndex = -1;
    [JsonProperty] public bool isBeingTakenCareOf;

    public virtual void AssignPeasant(PeasantAdultScript newPeasantAdultScript)
    {
        peasantAdultScript = newPeasantAdultScript;
    }

    public virtual void TaskProgress()
    {
        peasantAdultScript.task = null;
        taskSourceScript.GetNextPendingTask(peasantAdultScript);
        peasantAdultScript = null;
    }

    public virtual void CancelTask()
    {
        isBeingTakenCareOf = false;
    }
}
