using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

public class SysUpdateControlMovers : ComponentSystem
{
    protected override void OnUpdate()
    {
        // Update CompControlsMovement with Components that would control it, such as Player or AI

        Entities.ForEach((
            ref CompPlayerControlsMovement cPlayerControlsMovement,
            ref CompControlsMovement cControlMover) =>
        {
            cControlMover.dirIntention = cPlayerControlsMovement.moveDirIntention;
        });
    }
}