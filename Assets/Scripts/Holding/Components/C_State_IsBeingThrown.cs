using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct C_State_IsBeingThrown : IComponentData
{
    public float3 normalizedLateralDir;
    public float speed;
}
