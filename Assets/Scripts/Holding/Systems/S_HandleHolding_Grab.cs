using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;

// FIXME: Holding components are not immediately added, which allows a character to pick up all items they are touching in one frame

public class S_HandleHolding_Grab : JobComponentSystem
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
            cCanHoldGroup = GetComponentDataFromEntity<C_CanHold>(),
            cCanBeHeldGroup = GetComponentDataFromEntity<C_CanBeHeld>(),
            cIsHoldingGroup = GetComponentDataFromEntity<C_IsHolding>(),
            cIsHeldGroup = GetComponentDataFromEntity<C_IsHeld>(),
            cIsImmobilizedGroup = GetComponentDataFromEntity<C_IsImmobilized>()
    };
        job.Schedule(stepPhysicsWorld.Simulation, ref buildPhysicsWorld.PhysicsWorld, inputDeps).Complete();
        return inputDeps;
    }

    private struct HandleHolding_Grab_Job : ITriggerEventsJob
    {
        public EntityCommandBuffer commandBuffer;
        public ComponentDataFromEntity<C_CanHold> cCanHoldGroup;
        [ReadOnly] public ComponentDataFromEntity<C_CanBeHeld> cCanBeHeldGroup;
        [ReadOnly] public ComponentDataFromEntity<C_IsHolding> cIsHoldingGroup;
        [ReadOnly] public ComponentDataFromEntity<C_IsHeld> cIsHeldGroup;
        [ReadOnly] public ComponentDataFromEntity<C_IsImmobilized> cIsImmobilizedGroup;

        public void Execute(TriggerEvent triggerEvent)
        {
            CheckForGrab(triggerEvent.Entities.EntityA, triggerEvent.Entities.EntityB);
            CheckForGrab(triggerEvent.Entities.EntityB, triggerEvent.Entities.EntityA);
        }

        private void CheckForGrab(Entity entityHolder, Entity entityCanBeHeld)
        {
            if  (cCanHoldGroup.HasComponent(entityHolder) &&        // If entity A can hold
                !cIsHoldingGroup.HasComponent(entityHolder) &&      // And entity A is not holding something already
                !cIsImmobilizedGroup.HasComponent(entityHolder) &&  // And entity A is not immobilized
                cCanBeHeldGroup.HasComponent(entityCanBeHeld) &&    // And entity B can be held
                !cIsHeldGroup.HasComponent(entityCanBeHeld))        // And entity B is not already being held by someone else
            {
                if (cCanHoldGroup[entityHolder].isAttemptingAction)
                {
                    commandBuffer.AddComponent(entityHolder, new C_IsHolding { heldEntity = entityCanBeHeld });
                    commandBuffer.AddComponent(entityCanBeHeld, new C_IsHeld { entityHeldBy = entityHolder });
                    commandBuffer.AddComponent(entityCanBeHeld, new C_SnapsToEntityPosition { entityToSnapTo = entityHolder, offset = new float3(0, 0.69f, 0)});
                }
            }
        }
    }
}