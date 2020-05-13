using System;
using Unity.Entities;

[Serializable]
public struct C_IsHolding : IComponentData
{
    public Entity heldEntity;
}
