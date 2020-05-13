using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct C_IsControlledByAI_Wander : IComponentData
{
    public float moveSpeedMod;
    public float2 pauseTimeRange;
    public float2 moveTimeRange;
    public float timer;
    public bool isStateMoving;
}
