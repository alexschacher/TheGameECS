using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct C_IsControlledByPlayer : IComponentData
{
    public int playerID;
    public float2 moveDirIntention;
}
