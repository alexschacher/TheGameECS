using System;
using Unity.Entities;

[Serializable]
public struct C_State_IsAttacking : IComponentData
{
    public float timer;
}