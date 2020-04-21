using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;

public class SysApplyControlMovers : ComponentSystem
{
    protected override void OnUpdate()
    {
        // Apply ControlMover intended dir to CompMover dir and normalize.
        // TODO: Add a check for a tag component that freezes the ability to control movement. "Paralyzed"

        Entities.ForEach((
            ref CompControlsMovement cControlsMovement,
            ref CompMoves cMoves) =>
        {
            cMoves.normalizedDir = new Vector3(cControlsMovement.dirIntention.x, 0f, cControlsMovement.dirIntention.y).normalized;
        });
    }
}