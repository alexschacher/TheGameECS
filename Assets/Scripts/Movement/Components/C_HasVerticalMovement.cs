using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct C_HasVerticalMovement : IComponentData
{
    public float previousYValue;
    public float velocity;
    public bool isGrounded;
    public float groundedTimer;
}
