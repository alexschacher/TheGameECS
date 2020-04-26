using Unity.Entities;
using Unity.Physics;

// Apply CompMover dir and speed to Unity default component PhysicsVelocity and save dir to CompDirFacing

public class S_MoveEntities : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((
            ref PhysicsVelocity cVelocity,
            ref C_FacesDirection cDir,
            ref C_Moves cMover) =>
        {
            cVelocity.Linear.x = cMover.normalizedDir.x * cMover.speed * Time.DeltaTime;
            cVelocity.Linear.z = cMover.normalizedDir.z * cMover.speed * Time.DeltaTime;

            // Set facing dir if moving
            if (System.Math.Abs(cMover.normalizedDir.x) > 0 || System.Math.Abs(cMover.normalizedDir.z) > 0)
            {
                cDir.dirFacing = cMover.normalizedDir;
            }
        });
    }
}