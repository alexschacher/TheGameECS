using System;
using Unity.Entities;

[Serializable]
public struct C_Animates : IComponentData
{
    public UVAnimation.Anim currentAnim;
    public float animSpeed;
    public float animFrameTimer;
    public int animFrame;
    public bool destroyAfterAnim;
}