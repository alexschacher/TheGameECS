using Unity.Entities;

[InternalBufferCapacity(16)]
public struct C_BufferElement_EntityAnimation : IBufferElementData
{
    public UVAnimation.Anim anim;
    public float speed;
    public int priority;
    public AnimationStateDeterminer determiner;
}