using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct C_Event_Drop : IComponentData
{
    public bool isThrowing;
    public float3 throwNormalizedLateralDir;
}
