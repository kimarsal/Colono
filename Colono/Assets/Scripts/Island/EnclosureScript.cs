using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EnclosureScript : ConstructionScript
{
    public enum EnclosureType { Garden, Barn, Training };
    public EnclosureType enclosureType;
}
