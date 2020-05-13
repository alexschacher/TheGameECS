using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct C_SnapsToEntityPosition : IComponentData
{
    public Entity entityToSnapTo;
    public float3 offset;
}