using System;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(S_Collision_EntityScanner))]
public class S_SelectAndSubmitTargetToAI_EntityScanner : ComponentSystem
{
    private DynamicBuffer<C_BufferElement_ScannedEntity> dynamicBuffer;
    private ComponentDataFromEntity<Translation> cTransGroup;
    private ComponentDataFromEntity<C_Controller_AI> cAIControlGroup;
    private float distanceToEntity;
    private float distanceToClosestEntity;
    private Entity closestEntity;
    private C_Controller_AI aiController;

    protected override void OnUpdate()
    {
        cTransGroup = GetComponentDataFromEntity<Translation>(true);
        cAIControlGroup = GetComponentDataFromEntity<C_Controller_AI>();

        Entities.ForEach((
            Entity e,
            ref Translation cTrans,
            ref C_Ability_CanScanForEntities cScanner,
            ref C_Ability_SnapsToEntityPosition cSnaps) =>
        {

            // Find closest entity
            dynamicBuffer = EntityManager.GetBuffer<C_BufferElement_ScannedEntity>(e).Reinterpret<C_BufferElement_ScannedEntity>();
            distanceToClosestEntity = 10000f;
            closestEntity = e;

            for (int i = 0; i < dynamicBuffer.Length; i++)
            {
                distanceToEntity = Math.Abs(Vector3.Distance(cTransGroup[dynamicBuffer[i].entity].Value, cTrans.Value));
                if (distanceToEntity < distanceToClosestEntity)
                {
                    distanceToClosestEntity = distanceToEntity;
                    closestEntity = dynamicBuffer[i].entity;
                }
            }

            // Send chosen target to AI Controller
            if (EntityManager.Exists(cSnaps.entityToSnapTo))
            {
                aiController = cAIControlGroup[cSnaps.entityToSnapTo];
                aiController.targetEntity = closestEntity;
                aiController.hasTarget = !(closestEntity == e);
                EntityManager.SetComponentData(cSnaps.entityToSnapTo, aiController);
            }
        });
    }
}