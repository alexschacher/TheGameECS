using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct C_Jumps : IComponentData
{
    public float power;
    public bool isAttemptingJump;
}
