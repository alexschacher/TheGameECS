using System;
using Unity.Entities;

[Serializable]
public struct C_CanAttack : IComponentData
{
    public bool isAttemptingAction;
}
