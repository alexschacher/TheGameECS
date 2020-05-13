using System;
using Unity.Entities;
using Unity.Transforms;

[UpdateBefore(typeof(S_ApplyVelocity))]
public class S_ApplyGravity : ComponentSystem
{
    float gravityFactor = 1500f;
    float maxVelocity = 600f;
    float restingGroundedVelocity = -15f;

    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        Entities.ForEach((
            ref Translation cTrans,
            ref C_HasVerticalMovement cVert,
            ref C_HasGravity cGrav) =>
            {
                if (cVert.velocity < 0 && cVert.previousYValue == cTrans.Value.y) // Become grounded
                {
                    cVert.isGrounded = true;
                    cVert.velocity = restingGroundedVelocity;
                    cVert.groundedTimer = 0;
                }
                if (cVert.previousYValue != cTrans.Value.y || cVert.velocity > 0) // Become ungrounded
                {
                    cVert.isGrounded = false;
                }
                if (cVert.velocity > -maxVelocity && !cVert.isGrounded) // Apply gravity if ungrounded
                {
                    cVert.velocity -= gravityFactor * deltaTime;
                }
                cVert.previousYValue = cTrans.Value.y;
            });
    }
}