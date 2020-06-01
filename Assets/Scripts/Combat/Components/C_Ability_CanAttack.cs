using System;
using Unity.Entities;

[Serializable]
public struct C_Ability_CanAttack : IComponentData
{
    public bool isAttemptingAction;
}
