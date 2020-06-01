using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct C_Ability_CanControlLateralMovement : IComponentData
{
    public float3 normalizedLateralDir;
    public float speed;
    public bool isControllingMovement;
}
