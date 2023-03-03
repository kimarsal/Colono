using Newtonsoft.Json;
using UnityEngine;

public abstract class EnclosureScript : ConstructionScript, TaskSourceInterface
{
    public enum EnclosureType { Garden, Pen, Training };
    public EnclosureType enclosureType;

    public Vector3 minPos;
    public Vector3 maxPos;

    [JsonIgnore] public override string title { get { return enclosureType.ToString(); } }

    public virtual void InitializeEnclosure(EnclosureScript enclosureScript, IslandScript islandScript)
    {
        this.islandScript = islandScript;
        constructionType = ConstructionType.Enclosure;
        width = (int)cells[cells.Length - 1].x - (int)cells[0].x + 1;
        length = (int)cells[cells.Length - 1].y - (int)cells[0].y + 1;

        GameObject fences = new GameObject("Fences");
        fences.transform.parent = transform;
        fences.transform.localPosition = Vector3.zero;
        Vector3[] positions;
        Quaternion[] rotations;
        MeshGenerator.GetFencePositions(cells, islandScript.meshData, out positions, out rotations);
        for (int i = 0; i < positions.Length - 1; i++)
        {
            GameObject fence = Instantiate(islandEditor.GetRandomFencePrefab(), transform.position + positions[i], rotations[i], fences.transform);
            if (i == 0) minPos = fence.transform.localPosition;
            else if (i == positions.Length - 2) maxPos = fence.transform.localPosition - new Vector3(0, 0, 1);
        }

        if (enclosureType == EnclosureType.Pen)
        {
            GameObject closedGate = Instantiate(islandEditor.gate, transform.position + positions[positions.Length - 1], rotations[rotations.Length - 1], fences.transform);
        }
        else
        {
            GameObject post = Instantiate(islandEditor.post, transform.position + positions[positions.Length - 1], rotations[rotations.Length - 1], fences.transform);
        }
        outline = fences.AddComponent<Outline>();
        outline.enabled = false;

        BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.center = (minPos + maxPos) / 2;
        boxCollider.size = new Vector3(maxPos.x - minPos.x, 1, minPos.z - maxPos.z);
        boxCollider.isTrigger = true;
        gameObject.layer = 8;

        maxPeasants = (width - 2) * (length - 2);
        minPos += transform.position;
        maxPos += transform.position;

        entry = Instantiate(islandEditor.enclosureCenter, transform.position + boxCollider.center, Quaternion.identity, transform).transform;
    }

    public virtual bool GetNextPendingTask(PeasantAdultScript peasantAdultScript)
    {
        return false;
    }

    public override void AddPeasant(PeasantScript peasantScript)
    {
        base.AddPeasant(peasantScript);

        GetNextPendingTask((PeasantAdultScript)peasantScript);
    }

    public override PeasantScript RemovePeasant()
    {
        PeasantAdultScript peasantScript = (PeasantAdultScript)peasantList[0];
        for(int i = 0; i < peasantList.Count; i++)
        {
            PeasantAdultScript peasantAdultScript = (PeasantAdultScript)peasantList[i];
            if(peasantAdultScript.task == null)
            {
                peasantScript = peasantAdultScript;
                break;
            }
        }
        peasantScript.CancelTask();
        peasantList.Remove(peasantScript);
        peasantsOnTheirWay--;

        return peasantScript;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("NPC"))
        {
            peasantsOnTheirWay--;
            UpdateConstructionDetails();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("NPC"))
        {
            peasantsOnTheirWay++;
            UpdateConstructionDetails();
        }
    }

    public override void FinishUpBusiness()
    {
        foreach (PeasantScript peasantScript in peasantList)
        {
            if (peasantScript.peasantType == PeasantScript.PeasantType.Adult)
            {
                ((PeasantAdultScript)peasantScript).CancelTask();
            }
        }
    }
}