using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct C_IsImmobilized : IComponentData
{
    public float timer;
}
