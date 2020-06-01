using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(S_HandleAIControl))]
public class S_AIBehavior_RunFromEntity : ComponentSystem
{
    private ComponentDataFromEntity<Translation> cTransGroup;
    private Vector3 directionToTarget;

    protected override void OnUpdate()
    {
        cTransGroup = GetComponentDataFromEntity<Translation>(true);

        Entities.ForEach((
            ref Translation cTrans,
            ref C_Controller_AI cAIControl,
            ref C_AIBehavior_RunFromEntity cBehavior,
            ref C_Ability_CanControlLateralMovement cControlsMovement) =>
        {
            if (!EntityManager.Exists(cAIControl.targetEntity))
            {
                return;
            }

            directionToTarget = cTrans.Value - cTransGroup[cAIControl.targetEntity].Value;
            cControlsMovement.normalizedLateralDir = directionToTarget.normalized;
        });
    }
}