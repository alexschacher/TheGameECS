using System;
using Unity.Entities;

[Serializable]
public struct C_State_IsHolding : IComponentData
{
    public Entity heldEntity;
}
