using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct C_AIBehavior_Wander : IComponentData
{
    public float currentStateTimer;
    public float2 pauseTimeRange;
    public float2 moveTimeRange;
    public bool isCurrentStateMoving;
}