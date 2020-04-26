using UnityEngine;
using Unity.Entities;
using Unity.Physics;

public class S_ControlEntitiesJump : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((
            ref PhysicsVelocity cVelocity,
            ref C_Jumps cJumps) =>
        {
            if (cJumps.isAttemptingJump)
            {
                cJumps.isAttemptingJump = false;

                if (Mathf.Abs(cVelocity.Linear.y) < 0.05f) // Determine if we are grounded and able to jump
                {
                    cVelocity.Linear.y = cJumps.power;
                }
            }
        });
    }
}