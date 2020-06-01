using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class S_HandleAIControl : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((
            Entity e,
            ref C_Controller_AI cAIControl,
            ref C_Ability_CanControlLateralMovement cControlsMovement) =>
        {
            // This is the Brain of the AI.
            // Right now it is a very simple and messy way of choosing a behavior,
            // and part of the brain is contained in S_ChooseTarget
            // Right now the behaviors are hardcoded and not derived from AIControl.reactionBehavior and AIControl.idleBehavior

            if (cAIControl.hasTarget)
            {
                cAIControl.currentBehavior = cAIControl.reactionBehavior;

                if (!EntityManager.HasComponent(e, typeof(C_AIBehavior_ChaseAndAttackEntity)))
                {
                    EntityManager.AddComponentData(e, new C_AIBehavior_ChaseAndAttackEntity());

                    if (EntityManager.HasComponent(e, typeof(C_AIBehavior_Wander)))
                    {
                        EntityManager.RemoveComponent(e, typeof(C_AIBehavior_Wander));
                        cControlsMovement.normalizedLateralDir = new Vector3(0f, 0f, 0f).normalized;
                    }
                }
            }
            else
            {
                cAIControl.currentBehavior = cAIControl.idleBehavior;

                if (!EntityManager.HasComponent(e, typeof(C_AIBehavior_Wander)))
                {
                    EntityManager.AddComponentData(e, new C_AIBehavior_Wander
                    {
                        pauseTimeRange = new float2(1f, 2f),
                        moveTimeRange = new float2(2f, 3f)
                    });

                    if (EntityManager.HasComponent(e, typeof(C_AIBehavior_ChaseAndAttackEntity)))
                    {
                        EntityManager.RemoveComponent(e, typeof(C_AIBehavior_ChaseAndAttackEntity));
                    }
                }
            }
        });
    }
} 