using Unity.Entities;
using UnityEngine;

public class S_AI_Wander : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((
            ref C_IsControlledByAI_Wander cWander,
            ref C_ControlsMovement cControlMovement) =>
        {
            cWander.timer -= Time.DeltaTime;

            if (cWander.timer < 0)
            {
                if (cWander.isStateMoving)
                {
                    cWander.timer = Random.Range(cWander.pauseTimeRange.x, cWander.pauseTimeRange.y);
                    cControlMovement.dirIntention = new Unity.Mathematics.float2(0, 0);
                }
                else
                {
                    cWander.timer = Random.Range(cWander.moveTimeRange.x, cWander.moveTimeRange.y);
                    cControlMovement.dirIntention = new Unity.Mathematics.float2
                    (
                        Random.Range(-1f, 1f),
                        Random.Range(-1f, 1f)
                    );
                }
                cWander.isStateMoving = !cWander.isStateMoving;
            }
        });
    }
}