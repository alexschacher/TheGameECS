using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

public class S_HandleForceCollisions : JobComponentSystem
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
            cAppliesForceGroup = GetComponentDataFromEntity<C_AppliesForce>(),
            cReactsToForceGroup = GetComponentDataFromEntity<C_ReactsToForce>(),
            cTranslationGroup = GetComponentDataFromEntity<Translation>()
        };
        job.Schedule(stepPhysicsWorld.Simulation, ref buildPhysicsWorld.PhysicsWorld, inputDeps).Complete();
        return inputDeps;
    }

    private struct HandleHolding_Grab_Job : ITriggerEventsJob
    {
        public EntityCommandBuffer commandBuffer;
        [ReadOnly] public ComponentDataFromEntity<C_AppliesForce> cAppliesForceGroup;
        public ComponentDataFromEntity<C_ReactsToForce> cReactsToForceGroup;
        public ComponentDataFromEntity<Translation> cTranslationGroup;

        public void Execute(TriggerEvent triggerEvent)
        {
            Check(triggerEvent.Entities.EntityA, triggerEvent.Entities.EntityB);
            Check(triggerEvent.Entities.EntityB, triggerEvent.Entities.EntityA);
        }

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

                    commandBuffer.SetComponent(forceReactor, new C_ReactsToForce
                    {
                        normalizedLateralDir = dir,
                        forceResistanceFactor = cReactsToForceGroup[forceReactor].forceResistanceFactor,
                        speed = cAppliesForceGroup[forceApplicator].force * cReactsToForceGroup[forceReactor].forceResistanceFactor,
                        timer = cAppliesForceGroup[forceApplicator].forceTime
                    });
                }
            }
        }
    }
}