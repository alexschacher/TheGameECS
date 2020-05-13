using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct C_CanHold : IComponentData
{
    public bool isAttemptingAction;
}
