using Unity.Entities;

public class S_HandleDeath : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities
            .WithNone<C_IsDestroyedAfterTimer>()
            .ForEach((
            Entity e,
            ref C_TakesDamage cTakesDamage,
            ref C_CanDie cCanDie) =>
        {
            if (cTakesDamage.health <= 0)
            {
                // TODO: change to death animation
                EntityManager.AddComponentData(e, new C_IsDestroyedAfterTimer { timer = cCanDie.deathTimer });
                EntityManager.AddComponentData(e, new C_IsInvulnerable { timer = cCanDie.deathTimer });
                EntityManager.AddComponentData(e, new C_IsImmobilized { timer = cCanDie.deathTimer });
                EntityFactory.InstantiateEffectDeath(e);
            }
        });
    }
}