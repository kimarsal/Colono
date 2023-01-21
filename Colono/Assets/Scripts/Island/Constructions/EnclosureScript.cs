using UnityEngine;

public abstract class EnclosureScript : ConstructionScript
{
    public enum EnclosureType { Garden, Pen, Training };
    public EnclosureType enclosureType;

    public Vector3 minPos;
    public Vector3 maxPos;

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

    public EnclosureInfo GetEnclosureInfo()
    {
        EnclosureInfo enclosureInfo = null;
        switch (enclosureType)
        {
            case EnclosureType.Garden: enclosureInfo = ((GardenScript)this).GetGardenInfo(); break;
            case EnclosureType.Pen: enclosureInfo = ((PenScript)this).GetPenInfo(); break;
            default: enclosureInfo = new EnclosureInfo(); break;
        }
        enclosureInfo.enclosureType = enclosureType;
        enclosureInfo.minPos = new SerializableVector3(minPos);
        enclosureInfo.maxPos = new SerializableVector3(maxPos);
        return enclosureInfo;
    }
}

[System.Serializable]
public class EnclosureInfo : ConstructionInfo
{
    public EnclosureScript.EnclosureType enclosureType;
    public SerializableVector3 minPos;
    public SerializableVector3 maxPos;
}