using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct C_AppliesForce : IComponentData
{
    public float force;
    public float forceTime;
    public Entity originator;
}
