using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface TaskSourceInterface
{
    public abstract bool GetNextPendingTask(PeasantAdultScript peasantAdultScript);
}
