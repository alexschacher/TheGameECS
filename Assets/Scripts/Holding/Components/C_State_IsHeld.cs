using System;
using Unity.Entities;

[Serializable]
public struct C_State_IsHeld : IComponentData
{
    public Entity entityHeldBy;
}
