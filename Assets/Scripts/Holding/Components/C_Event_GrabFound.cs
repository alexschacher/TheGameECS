using System;
using Unity.Entities;

[Serializable]
public struct C_Event_GrabFound : IComponentData
{
    public Entity entityToGrab;
}
