using Unity.Entities;
using Unity.Transforms;
using Unity.Physics;
using UnityEngine;
using Unity.Mathematics;

[UpdateBefore(typeof(SysBillboard))]
public class SysApplyMovement : ComponentSystem
{
    protected override void OnUpdate()
    {
        // Apply CompMover dir and speed to Unity default component PhysicsVelocity

        Entities.ForEach((
            ref PhysicsVelocity cVelocity,
            ref CompMoves cMover) =>
        {
            cVelocity.Linear = cMover.normalizedDir * cMover.speed * Time.DeltaTime;

            // Set facing dir if moving
            if (System.Math.Abs(cMover.normalizedDir.x) > 0 || System.Math.Abs(cMover.normalizedDir.z) > 0)
            {
                cMover.facingDir = cMover.normalizedDir;
            }
        });
    }
}