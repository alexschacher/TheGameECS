using Unity.Entities;
using Unity.Transforms;
using Unity.Physics;
using UnityEngine;
using Unity.Mathematics;

// Apply CompMover dir and speed to Unity default component PhysicsVelocity and save dir to CompDirFacing

[UpdateBefore(typeof(S_ModifyBillboards))]
public class S_MoveEntities : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((
            ref PhysicsVelocity cVelocity,
            ref C_FacesDirection cDir,
            ref C_Moves cMover) =>
        {
            cVelocity.Linear = cMover.normalizedDir * cMover.speed * Time.DeltaTime;

            // Set facing dir if moving
            if (System.Math.Abs(cMover.normalizedDir.x) > 0 || System.Math.Abs(cMover.normalizedDir.z) > 0)
            {
                cDir.dirFacing = cMover.normalizedDir;
            }
        });
    }
}