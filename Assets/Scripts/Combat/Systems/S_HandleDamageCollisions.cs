using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

public class S_HandleDamageCollisions : JobComponentSystem
{
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
        var job = new HandleHolding_Grab_Job
        {
            commandBuffer = commandBufferSystem.CreateCommandBuffer(),
            cInflictsDamageGroup = GetComponentDataFromEntity<C_InflictsDamage>(),
            cTakesDamageGroup = GetComponentDataFromEntity<C_TakesDamage>(),
            cIsInvulnerableGroup = GetComponentDataFromEntity<C_IsInvulnerable>()
        };
        job.Schedule(stepPhysicsWorld.Simulation, ref buildPhysicsWorld.PhysicsWorld, inputDeps).Complete();
        return inputDeps;
    }

    private struct HandleHolding_Grab_Job : ITriggerEventsJob
    {
        public EntityCommandBuffer commandBuffer;
        public ComponentDataFromEntity<C_TakesDamage> cTakesDamageGroup;
        [ReadOnly] public ComponentDataFromEntity<C_InflictsDamage> cInflictsDamageGroup;
        [ReadOnly] public ComponentDataFromEntity<C_IsInvulnerable> cIsInvulnerableGroup;

        public void Execute(TriggerEvent triggerEvent)
        {
            CheckForDamage(triggerEvent.Entities.EntityA, triggerEvent.Entities.EntityB);
            CheckForDamage(triggerEvent.Entities.EntityB, triggerEvent.Entities.EntityA);
        }

        private void CheckForDamage(Entity damageInflictor, Entity damageTaker)
        {
            if (cTakesDamageGroup.HasComponent(damageTaker) &&
                !cIsInvulnerableGroup.HasComponent(damageTaker) &&
                cInflictsDamageGroup.HasComponent(damageInflictor))
            {
                if (cInflictsDamageGroup[damageInflictor].originator != damageTaker)
                {
                    float newHealth = cTakesDamageGroup[damageTaker].health - cInflictsDamageGroup[damageInflictor].damage;

                    commandBuffer.SetComponent(damageTaker, new C_TakesDamage { health = newHealth });
                    commandBuffer.AddComponent(damageTaker, new C_IsInvulnerable { timer = cInflictsDamageGroup[damageInflictor].invulnerableTime });
                    commandBuffer.AddComponent(damageTaker, new C_IsFlashingColor
                    {
                        endTimer = cInflictsDamageGroup[damageInflictor].invulnerableTime,
                        onTime = 0.1f,
                        offTime = 0.1f,
                        color = Color.red
                    });
                }
            }
        }
    }
}