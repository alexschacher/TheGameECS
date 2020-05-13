using System;
using Unity.Entities;
using Unity.Mathematics;

// Is this component too redundant?

[Serializable]
public struct C_ControlsMovement : IComponentData
{
    public float3 normalizedLateralDir;
    public float speed;
}
