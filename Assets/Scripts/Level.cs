using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct Level : IComponentData
{
    public int2 Size; // The size of our level, in number of grid cells
}
