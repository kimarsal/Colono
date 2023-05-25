using Newtonsoft.Json;
using UnityEngine;

[JsonObject(MemberSerialization.OptIn)]
public abstract class TaskScript : MonoBehaviour
{
    public enum TaskType { Item, Patch }
    [JsonProperty] public TaskType taskType;
    [JsonProperty] [JsonConverter(typeof(VectorConverter))] public Vector3 center;
    public Vector2 cell;

    public TaskSourceInterface taskSourceScript;
    public PeasantAdultScript peasantAdultScript;
    [JsonProperty] public bool isBeingTakenCareOf;

    public abstract void TaskProgress();

    public abstract void CancelTask();
}
