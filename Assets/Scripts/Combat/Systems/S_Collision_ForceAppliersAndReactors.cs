using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

public class S_Collision_ForceAppliersAndReactors : JobComponentSystem
{
    private struct Job_Collision_ForceAppliersAndReactors : ITriggerEventsJob
    {
        private void Check(Entity forceApplicator, Entity forceReactor)
        {
            if (cAppliesForceGroup.HasComponent(forceApplicator) &&
                cReactsToForceGroup.HasComponent(forceReactor))
            {
                if (cAppliesForceGroup[forceApplicator].originator != forceReactor)
                {
                    Vector3 dir = cTranslationGroup[forceReactor].Value - cTranslationGroup[forceApplicator].Value;
                    dir.y = 0f;
                    dir.Normalize();

                    commandBuffer.AddComponent(forceReactor, new C_State_IsReactingToForce
                    {
                        normalizedLateralDir = dir,
                        speed = cAppliesForceGroup[forceApplicator].force * cReactsToForceGroup[forceReactor].forceResistanceFactor,
                        timer = cAppliesForceGroup[forceApplicator].forceTime
                    });
                }
            }
        }

        public EntityCommandBuffer commandBuffer;
        [ReadOnly] public ComponentDataFromEntity<C_Ability_CanApplyForce> cAppliesForceGroup;
        public ComponentDataFromEntity<C_Ability_CanReactToForce> cReactsToForceGroup;
        public ComponentDataFromEntity<Translation> cTranslationGroup;

        public void Execute(TriggerEvent triggerEvent)
        {
            Check(triggerEvent.Entities.EntityA, triggerEvent.Entities.EntityB);
            Check(triggerEvent.Entities.EntityB, triggerEvent.Entities.EntityA);
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
        var job = new Job_Collision_ForceAppliersAndReactors
        {
            commandBuffer = commandBufferSystem.CreateCommandBuffer(),
            cAppliesForceGroup = GetComponentDataFromEntity<C_Ability_CanApplyForce>(),
            cReactsToForceGroup = GetComponentDataFromEntity<C_Ability_CanReactToForce>(),
            cTranslationGroup = GetComponentDataFromEntity<Translation>()
        };
        job.Schedule(stepPhysicsWorld.Simulation, ref buildPhysicsWorld.PhysicsWorld, inputDeps).Complete();
        return inputDeps;
    }
}