using Unity.Entities;

[InternalBufferCapacity(16)]
public struct C_BufferElement_ScannedEntity : IBufferElementData
{
    public Entity entity;
}