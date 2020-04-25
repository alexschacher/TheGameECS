using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct C_Moves : IComponentData
{
    public float3 normalizedDir;
    public float speed;
}
