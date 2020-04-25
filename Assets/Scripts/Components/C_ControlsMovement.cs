using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

// Is this component too redundant?

[Serializable]
public struct C_ControlsMovement : IComponentData
{
    public float2 dirIntention;
}
