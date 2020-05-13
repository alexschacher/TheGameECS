using UnityEngine;
using Unity.Entities;
using Unity.Physics;

public class S_HandleJump : ComponentSystem
{
    float deltaTime;
    float groundedTimeBuffer = 0.1f;
    float jumpTimeBuffer = 0.1f;

    protected override void OnUpdate()
    {
        deltaTime = Time.DeltaTime;

        Entities
            .WithNone<C_IsImmobilized>()
            .ForEach((
            ref C_HasVerticalMovement cVert,
            ref C_CanJump cJumper) =>
        {
            cJumper.timeSinceLastAttemptedJump += deltaTime;
            cJumper.timeSinceLastGrounded += deltaTime;

            if (cJumper.isAttemptingJump) cJumper.timeSinceLastAttemptedJump = 0;
            if (cVert.isGrounded) cJumper.timeSinceLastGrounded = 0;

            if (cJumper.timeSinceLastAttemptedJump < jumpTimeBuffer &&
                cJumper.timeSinceLastGrounded < groundedTimeBuffer)
            {
                cVert.velocity = cJumper.power;
                cJumper.timeSinceLastGrounded += groundedTimeBuffer;
                cJumper.timeSinceLastAttemptedJump += jumpTimeBuffer;
            }
        });
    }
}