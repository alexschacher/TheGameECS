using System;
using Unity.Entities;

[Serializable]
public struct C_IsHeld : IComponentData
{
    public Entity entityHeldBy;
}
