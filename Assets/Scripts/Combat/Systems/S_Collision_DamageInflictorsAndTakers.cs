using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

public class S_Collision_DamageInflictorsAndTakers : JobComponentSystem
{
    private struct Job_Collision_DamageInflictorsAndTakers : ITriggerEventsJob
    {
        private void CheckForDamage(Entity damageInflictor, Entity damageTaker)
        {
            if (cTakesDamageGroup.HasComponent(damageTaker) &&
                !cIsInvulnerableGroup.HasComponent(damageTaker) &&
                cInflictsDamageGroup.HasComponent(damageInflictor))
            {
                if (cInflictsDamageGroup[damageInflictor].originator != damageTaker)
                {
                    float newHealth = cTakesDamageGroup[damageTaker].health - cInflictsDamageGroup[damageInflictor].damage;

                    commandBuffer.SetComponent(damageTaker, new C_Ability_CanTakeDamage { health = newHealth });
                    commandBuffer.AddComponent(damageTaker, new C_State_IsInvulnerable { timer = cInflictsDamageGroup[damageInflictor].invulnerableTime });
                    commandBuffer.AddComponent(damageTaker, new C_State_IsImmobilized { timer = cInflictsDamageGroup[damageInflictor].invulnerableTime });
                    commandBuffer.AddComponent(damageTaker, new C_State_IsHurt { timer = cInflictsDamageGroup[damageInflictor].invulnerableTime });
                    commandBuffer.AddComponent(damageTaker, new C_State_IsFlashingColor
                    {
                        endTimer = cInflictsDamageGroup[damageInflictor].invulnerableTime,
                        onTime = 0.1f,
                        offTime = 0.1f,
                        color = Color.red
                    });
                }
            }
        }

        public EntityCommandBuffer commandBuffer;
        public ComponentDataFromEntity<C_Ability_CanTakeDamage> cTakesDamageGroup;
        [ReadOnly] public ComponentDataFromEntity<C_Ability_CanInflictDamage> cInflictsDamageGroup;
        [ReadOnly] public ComponentDataFromEntity<C_State_IsInvulnerable> cIsInvulnerableGroup;

        public void Execute(TriggerEvent triggerEvent)
        {
            CheckForDamage(triggerEvent.Entities.EntityA, triggerEvent.Entities.EntityB);
            CheckForDamage(triggerEvent.Entities.EntityB, triggerEvent.Entities.EntityA);
        }
    }

    private BuildPhysicsWorld buildPhysicsWorld;
    private StepPhysicsWorld stepPhysicsWorld;
    private BeginInitializationEntityCommandBufferSystem commandBufferSystem;

    protected override void OnCreate()
    {
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
        commandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new Job_Collision_DamageInflictorsAndTakers
        {
            commandBuffer = commandBufferSystem.CreateCommandBuffer(),
            cInflictsDamageGroup = GetComponentDataFromEntity<C_Ability_CanInflictDamage>(),
            cTakesDamageGroup = GetComponentDataFromEntity<C_Ability_CanTakeDamage>(),
            cIsInvulnerableGroup = GetComponentDataFromEntity<C_State_IsInvulnerable>()
        };
        job.Schedule(stepPhysicsWorld.Simulation, ref buildPhysicsWorld.PhysicsWorld, inputDeps).Complete();
        return inputDeps;
    }
}