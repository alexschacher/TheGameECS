using Unity.Entities;

public class S_DieIfNoHealth : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities
            .WithNone<C_State_IsDestroyedAfterTimer>()
            .ForEach((
            Entity e,
            ref C_Ability_CanTakeDamage cTakesDamage,
            ref C_Ability_CanDie cCanDie) =>
        {
            if (cTakesDamage.health <= 0)
            {
                // TODO: change to death animation
                EntityManager.AddComponentData(e, new C_State_IsDestroyedAfterTimer { timer = cCanDie.deathTimer });
                EntityManager.AddComponentData(e, new C_State_IsInvulnerable { timer = cCanDie.deathTimer });
                EntityManager.AddComponentData(e, new C_State_IsImmobilized { timer = cCanDie.deathTimer });
                EntityFactory.InstantiateEffectDeath(e);
            }
        });
    }
}