using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;

public class S_Collision_EntityScanner : JobComponentSystem
{
    private struct Job_Collision_EntityScanner : ITriggerEventsJob
    {
        private void Check(Entity entityScanner, Entity entityScanned)
        {
            if (cCanBeScannedGroup.HasComponent(entityScanned) &&
                cScannerGroup.HasComponent(entityScanner) &&
                cSnapsToPosition.HasComponent(entityScanner))
            {
                if (cSnapsToPosition[entityScanner].entityToSnapTo != entityScanned)
                {
                    cDynamicBufferGroup[entityScanner].Add(new C_BufferElement_ScannedEntity { entity = entityScanned });
                } 
            }
        }

        [ReadOnly] public ComponentDataFromEntity<C_Ability_CanBeScannedByAI> cCanBeScannedGroup;
        [ReadOnly] public ComponentDataFromEntity<C_Ability_CanScanForEntities> cScannerGroup;
        [ReadOnly] public ComponentDataFromEntity<C_Ability_SnapsToEntityPosition> cSnapsToPosition;
        public BufferFromEntity<C_BufferElement_ScannedEntity> cDynamicBufferGroup;

        public void Execute(TriggerEvent triggerEvent)
        {
            Check(triggerEvent.Entities.EntityA, triggerEvent.Entities.EntityB);
            Check(triggerEvent.Entities.EntityB, triggerEvent.Entities.EntityA);
        }
    }

    private BuildPhysicsWorld buildPhysicsWorld;
    private StepPhysicsWorld stepPhysicsWorld;

    protected override void OnCreate()
    {
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new Job_Collision_EntityScanner
        {
            cCanBeScannedGroup = GetComponentDataFromEntity<C_Ability_CanBeScannedByAI>(),
            cScannerGroup = GetComponentDataFromEntity<C_Ability_CanScanForEntities>(),
            cSnapsToPosition = GetComponentDataFromEntity<C_Ability_SnapsToEntityPosition>(),
            cDynamicBufferGroup = GetBufferFromEntity<C_BufferElement_ScannedEntity>()
        };
        job.Schedule(stepPhysicsWorld.Simulation, ref buildPhysicsWorld.PhysicsWorld, inputDeps).Complete();
        return inputDeps;
    }
}