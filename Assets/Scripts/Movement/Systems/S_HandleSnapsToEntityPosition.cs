using Unity.Entities;
using Unity.Physics.Systems;
using Unity.Transforms;

[UpdateAfter(typeof(ExportPhysicsWorld))]
public class S_HandleSnapsToEntityPosition : ComponentSystem
{
     protected override void OnUpdate()
    {
        Entities.ForEach((
            Entity e,
            ref C_Ability_SnapsToEntityPosition cSnaps,
            ref Translation cTrans) =>
        {
            if (EntityManager.Exists(cSnaps.entityToSnapTo))
            {
                cTrans.Value = GetComponentDataFromEntity<Translation>()[cSnaps.entityToSnapTo].Value + cSnaps.offset;
            }
            else
            {
                if (cSnaps.destroyOnParentLoss)
                {
                    EntityManager.DestroyEntity(e);
                }
                else
                {
                    EntityManager.RemoveComponent(e, typeof(C_Ability_SnapsToEntityPosition));
                }
            }
        });
    }
}