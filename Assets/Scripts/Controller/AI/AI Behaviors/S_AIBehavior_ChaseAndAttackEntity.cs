using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(S_HandleAIControl))]
public class S_AIBehavior_ChaseAndAttackEntity : ComponentSystem
{
    private ComponentDataFromEntity<Translation> cTransGroup;
    private Vector3 directionToTarget;
    private float targetDistance = 0.75f;

    protected override void OnUpdate()
    {
        cTransGroup = GetComponentDataFromEntity<Translation>(true);

        Entities.ForEach((
            ref Translation cTrans,
            ref C_Controller_AI cAIControl,
            ref C_Ability_CanAttack cAttack,
            ref C_AIBehavior_ChaseAndAttackEntity cBehavior,
            ref C_Ability_CanControlLateralMovement cControlsMovement) =>
        {
            if (!EntityManager.Exists(cAIControl.targetEntity))
            {
                return;
            }

            if (Vector3.Distance(cTransGroup[cAIControl.targetEntity].Value, cTrans.Value) > targetDistance)
            {
                
                directionToTarget = cTransGroup[cAIControl.targetEntity].Value - cTrans.Value;
                cControlsMovement.normalizedLateralDir = directionToTarget.normalized;
            }
            else
            {
                cAttack.isAttemptingAction = true;
            }
        });
    }
}