using System;
using Unity.Entities;

[Serializable]
public struct C_State_IsInvulnerable : IComponentData
{
    public float timer;
}