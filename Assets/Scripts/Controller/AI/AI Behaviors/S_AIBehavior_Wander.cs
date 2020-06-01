using Unity.Entities;
using UnityEngine;
using Random = UnityEngine.Random;

[UpdateAfter(typeof(S_HandleAIControl))]
public class S_AIBehavior_Wander : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((
            ref C_AIBehavior_Wander cWander,
            ref C_Ability_CanControlLateralMovement cControlMovement) =>
        {
            cWander.currentStateTimer -= Time.DeltaTime;

            if (cWander.currentStateTimer < 0)
            {
                if (cWander.isCurrentStateMoving)
                {
                    cWander.currentStateTimer = Random.Range(cWander.pauseTimeRange.x, cWander.pauseTimeRange.y);
                    cControlMovement.normalizedLateralDir = new Vector3(0f, 0f, 0f).normalized;
                }
                else
                {
                    cWander.currentStateTimer = Random.Range(cWander.moveTimeRange.x, cWander.moveTimeRange.y);
                    cControlMovement.normalizedLateralDir = new Vector3
                    (
                        Random.Range(-1f, 1f),
                        0f,
                        Random.Range(-1f, 1f)
                    ).normalized;
                }
                cWander.isCurrentStateMoving = !cWander.isCurrentStateMoving;
            }
        });
    }
}