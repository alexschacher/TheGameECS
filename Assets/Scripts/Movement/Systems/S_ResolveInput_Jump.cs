using Unity.Entities;

public class S_ResolveInput_Jump : ComponentSystem
{
    float deltaTime;
    float groundedTimeBuffer = 0.1f;
    float jumpTimeBuffer = 0.1f;

    protected override void OnUpdate()
    {
        deltaTime = Time.DeltaTime;

        Entities
            .WithNone<C_State_IsImmobilized>()
            .ForEach((
            ref C_Ability_CanMoveVertically cVert,
            ref C_Ability_CanJump cJumper) =>
        {
            cJumper.timeSinceLastAttemptedJump += deltaTime;

            if (cJumper.isAttemptingJump)
            {
                cJumper.timeSinceLastAttemptedJump = 0;
            }

            if (cVert.isGrounded)
            {
                cJumper.canJump = true;
            }

            if (cJumper.timeSinceLastAttemptedJump < jumpTimeBuffer &&
                cVert.timeSinceLastGrounded < groundedTimeBuffer)
            {
                cVert.velocity = cJumper.power;
                cJumper.canJump = false;
            }
        });
    }
}