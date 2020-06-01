using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct C_Ability_CanReactToForce : IComponentData
{
    public float forceResistanceFactor;
}