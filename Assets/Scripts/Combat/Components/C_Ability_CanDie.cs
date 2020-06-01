using System;
using Unity.Entities;

[Serializable]
public struct C_Ability_CanDie : IComponentData
{
    public float deathTimer;
}
