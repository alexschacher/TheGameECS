using System;
using Unity.Entities;

[Serializable]
public struct C_State_IsImmobilized : IComponentData
{
    public float timer;
}