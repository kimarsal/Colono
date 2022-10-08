using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class EnclosureScript : ConstructionScript
{
    public enum EnclosureType { Garden, Barn, Training };
    public EnclosureType enclosureType;
}
