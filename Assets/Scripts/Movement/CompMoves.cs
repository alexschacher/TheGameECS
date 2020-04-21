using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct CompMoves : IComponentData
{
    public float3 normalizedDir;
    public float3 facingDir;
    public float speed;
}
