using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct CompPlayerControlsMovement : IComponentData
{
    public int playerID;
    public float2 moveDirIntention;
}
