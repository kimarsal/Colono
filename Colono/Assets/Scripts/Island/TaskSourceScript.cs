using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TaskSourceScript : MonoBehaviour
{
    public List<TaskScript> taskList = new List<TaskScript>();

    public virtual bool GetNextPendingTask(PeasantAdultScript peasantAdultScript)
    {
        return peasantAdultScript.CanBeAsignedTask();
    }
}
