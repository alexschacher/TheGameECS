using System;
using Unity.Entities;

[Serializable]
public struct C_Ability_CanJump : IComponentData
{
    public float power;
    public bool isAttemptingJump;
    public float timeSinceLastAttemptedJump;
    public bool canJump;
}
