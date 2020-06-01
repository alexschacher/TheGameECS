using System;
using Unity.Entities;

[Serializable]
public struct C_LevelData : IComponentData
{
    // Level Data belongs to static objects saved and generated within the level grid.
    // Entities that do not have level data are dynamic entities, like characters, items, and effects
    // Spawners for said entities are static objects, possibly with a reference to the dynamic object it spawned

    public EntityID.ID id;
    public int x, y, z;
}