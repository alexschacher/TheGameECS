using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(S_CreateEvent_GrabDrop))]
public class S_Collision_EventGrab_CanBeHeld : JobComponentSystem
{
    private struct Job_Collision_EventGrab_CanBeHeld : ITriggerEventsJob
    {
        private void CheckForGrab(Entity entityGrabber, Entity entityCanBeHeld)
        {
            if (group_eventGrabAttempt.HasComponent(entityGrabber) && group_canBeHeld.HasComponent(entityCanBeHeld) && !group_isHeld.HasComponent(entityCanBeHeld))
            {
                commandBuffer.AddComponent(entityGrabber, new C_Event_GrabFound { entityToGrab = entityCanBeHeld });
            }
        }

        #region boilerplate

        public EntityCommandBuffer commandBuffer;
        [ReadOnly] public ComponentDataFromEntity<Translation> group_Trans;
        [ReadOnly] public ComponentDataFromEntity<C_Event_GrabAttempt> group_eventGrabAttempt;
        [ReadOnly] public ComponentDataFromEntity<C_Ability_CanBeHeld> group_canBeHeld;
        [ReadOnly] public ComponentDataFromEntity<C_State_IsHeld> group_isHeld;

        public void Execute(TriggerEvent triggerEvent)
        {
            CheckForGrab(triggerEvent.Entities.EntityA, triggerEvent.Entities.EntityB);
            CheckForGrab(triggerEvent.Entities.EntityB, triggerEvent.Entities.EntityA);
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
        var job = new Job_Collision_EventGrab_CanBeHeld
        {
            commandBuffer = commandBufferSystem.CreateCommandBuffer(),
            group_Trans = GetComponentDataFromEntity<Translation>(),
            group_eventGrabAttempt = GetComponentDataFromEntity<C_Event_GrabAttempt>(),
            group_canBeHeld = GetComponentDataFromEntity<C_Ability_CanBeHeld>(),
            group_isHeld = GetComponentDataFromEntity<C_State_IsHeld>()
        };
        job.Schedule(stepPhysicsWorld.Simulation, ref buildPhysicsWorld.PhysicsWorld, inputDeps).Complete();
        return inputDeps;
    }

    #endregion boilerplate
}