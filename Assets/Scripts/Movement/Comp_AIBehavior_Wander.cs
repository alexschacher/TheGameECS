using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct Comp_AIBehavior_Wander : IComponentData
{
    public float moveSpeedMod;
    public float2 pauseTimeRange;
    public float2 moveTimeRange;
    public float timer;
    public bool isStateMoving;
}
