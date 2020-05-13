using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct C_FacesDirection : IComponentData
{
    public float3 normalizedLateralDir;
}
