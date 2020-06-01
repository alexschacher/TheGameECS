using System;
using Unity.Entities;

[Serializable]
public struct C_State_IsHurt : IComponentData
{
    public float timer;
}