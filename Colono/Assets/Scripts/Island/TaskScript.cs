using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TaskScript : MonoBehaviour
{
    public enum TaskType { Item, Patch }
    public Vector3 center;
    public TaskType taskType;

    public PeasantAdultScript peasantScript;

    public abstract void TaskProgress();

    public abstract void CancelTask();
}
