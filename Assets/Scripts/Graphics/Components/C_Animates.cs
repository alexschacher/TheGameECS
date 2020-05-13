using System;
using Unity.Entities;

[Serializable]
public struct C_Animates : IComponentData
{
    public UVAnimation.Anim currentAnim;
    // public UVAnimation.Anim nextAnim; Not currently used
    public float animSpeed;
    public bool destroyAfterAnim;
    public float animFrameTimer;
    public int animFrame;
}