using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct C_CanDie : IComponentData
{
    //public UVAnimation.Anim deathAnim;
    public float deathTimer;
}
