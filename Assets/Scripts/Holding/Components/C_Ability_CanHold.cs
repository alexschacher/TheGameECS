using System;
using Unity.Entities;

[Serializable]
public struct C_Ability_CanHold : IComponentData
{
    public bool isAttemptingAction;
}
