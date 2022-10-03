using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskScript : MonoBehaviour
{
    public enum TaskType { Item, Patch }
    public Vector3 center;
    public TaskType taskType;

    public PeasantAdultScript peasantScript;
}
