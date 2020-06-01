using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct C_Ability_CanInflictDamage : IComponentData
{
    public Entity originator;
    public float damage;
    public float invulnerableTime;
}
