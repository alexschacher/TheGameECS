using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct C_Ability_FacesDirection : IComponentData
{
    public float3 normalizedLateralDir;
}
