using System;
using Unity.Entities;

[Serializable]
public struct C_Ability_CanBeHeld : IComponentData
{
    public float throwSpeedResistanceFactor;
}