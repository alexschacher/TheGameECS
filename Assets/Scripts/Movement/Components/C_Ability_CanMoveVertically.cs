using System;
using Unity.Entities;

[Serializable]
public struct C_Ability_CanMoveVertically : IComponentData
{
    public float previousYValue;
    public float velocity;
    public float timeSinceLastGrounded;
    public bool isGrounded;
    public bool isRising;
    public bool isFalling;
}