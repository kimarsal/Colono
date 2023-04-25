using Newtonsoft.Json;
using System.Collections;
using UnityEngine;
using static PeasantScript;

public class CabinScript : BuildingScript
{
    public override EditorScript editorScript { get { return CanvasScript.Instance.cabinEditor; } }

    public override void AddPeasant(PeasantScript peasantScript)
    {
        peasantList.Add(peasantScript);
        UpdateConstructionDetails();
    }

    public override PeasantScript PeasantHasArrived(PeasantScript peasantScript)
    {
        PeasantScript newPeasantScript = base.PeasantHasArrived(peasantScript);
        StartCoroutine(RestPeasantCoroutine(newPeasantScript));
        return null;
    }

    private IEnumerator RestPeasantCoroutine(PeasantScript peasantScript)
    {
        yield return new WaitForSeconds(5);

        if(peasantScript != null)
        {
            peasantList.Remove(peasantScript);
            peasantScript.transform.parent = islandScript.npcsTransform;
            peasantScript.navMeshAgent.Warp(entry.position);
            peasantScript.isInBuilding = false;
            peasantScript.exhaustion = 0;
            peasantScript.cabin = null;

            if (peasantScript.peasantType == PeasantType.Adult)
            {
                PeasantAdultScript peasantAdultScript = (PeasantAdultScript)peasantScript;
                peasantAdultScript.taskSourceInterface.GetNextPendingTask(peasantAdultScript);
                if(peasantAdultScript.peasantGender == PeasantGender.Female && peasantAdultScript.childrenAmount < 3 && peasantAdultScript.hunger < 1)
                {
                    PeasantGender peasantGender = (PeasantGender)Random.Range(0, 2);
                    PeasantChildScript peasantChildScript = Instantiate(ResourceScript.Instance.GetPeasantPrefab(PeasantType.Child, peasantGender),
                    peasantAdultScript.transform.position, peasantAdultScript.transform.rotation, islandScript.npcsTransform).GetComponent<PeasantChildScript>();

                    peasantChildScript.islandScript = islandScript;
                    peasantChildScript.isNative = false;
                    peasantChildScript.headType = Random.Range(0, 2);
                    peasantChildScript._SKINCOLOR = ResourceScript.Instance.GetRandomSkinColor();
                    peasantChildScript._HAIRCOLOR = ResourceScript.Instance.GetRandomHairColor();
                    peasantChildScript._CLOTH3COLOR = Random.ColorHSV();
                    peasantChildScript._CLOTH4COLOR = Random.ColorHSV();
                    peasantChildScript._OTHERCOLOR = Random.ColorHSV();

                    islandScript.peasantList.Add(peasantChildScript);
                    peasantChildScript.UpdateTask();

                    peasantAdultScript.childrenAmount++;
                }
            }
            else peasantScript.UpdateTask();

            peasantsInside--;
        }
    }

    public override PeasantScript RemovePeasant()
    {
        PeasantScript peasantScript = base.RemovePeasant();
        peasantScript.cabin = null;
        return peasantScript;
    }
}