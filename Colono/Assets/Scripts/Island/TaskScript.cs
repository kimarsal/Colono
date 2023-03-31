using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TaskScript : MonoBehaviour
{
    public TaskSourceInterface taskSourceScript;
    public enum TaskType { Item, Patch }
    public Vector3 center;
    public TaskType taskType;

    public PeasantAdultScript peasantAdultScript;
    public bool isBeingTakenCareOf;

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
