using Unity.Entities;

[UpdateBefore(typeof(S_Collision_EntityScanner))]
public class S_ClearBuffer_EntityScanner : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((
            Entity e,
            ref C_Ability_CanScanForEntities cScanner) =>
        {
            EntityManager.GetBuffer<C_BufferElement_ScannedEntity>(e).Clear();
        });
    }
}