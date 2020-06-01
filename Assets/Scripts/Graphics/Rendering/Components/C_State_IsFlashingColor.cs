using System;
using Unity.Entities;
using UnityEngine;

[Serializable]
public struct C_State_IsFlashingColor : IComponentData
{
    public Color color;
    public float onOffTimer;
    public float endTimer;
    public float onTime;
    public float offTime;
    public bool on;
}
