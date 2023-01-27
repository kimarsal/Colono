using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TaskScript : MonoBehaviour
{
    public TaskSourceScript taskSourceScript;
    public enum TaskType { Item, Patch, Pairing }
    public Vector3 center;
    public TaskType taskType;

    public PeasantAdultScript peasantAdultScript;

    public virtual void TaskProgress()
    {
        taskSourceScript.GetNextPendingTask(peasantAdultScript);
        peasantAdultScript = null;
    }

    public abstract void CancelTask();
}
