using Unity.Entities;
using Unity.Transforms;

[UpdateBefore(typeof(S_ApplyAllVelocitySources))]
public class S_DetermineGrounded_ApplyGravity : SystemBase
{
    private float gravityFactor = 1500f;
    private float maxVelocity = 600f;
    private float restingGroundedVelocity = -15f;
    private float timeSinceLastGroundedBuffer = 0.1f; // Buffer time to determine rising or falling state, as there is some glitchiness when entities walk around on flat ground. This is hacky, need to find a better physics solution.

    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        Entities.WithAll<C_Ability_AffectedByGravity>().ForEach((ref C_Ability_CanMoveVertically cVert, in Translation cTrans) =>
        {
            bool yValueHasChanged = cVert.previousYValue != cTrans.Value.y; 

            // Determine Grounded State
            if (cVert.velocity < 0 && !yValueHasChanged)
            {
                cVert.isGrounded = true;
                cVert.velocity = restingGroundedVelocity;
                cVert.timeSinceLastGrounded = 0;
            }
            else if (yValueHasChanged || cVert.velocity > 0)
            {
                cVert.isGrounded = false;
                cVert.timeSinceLastGrounded += deltaTime;
            }

            // Determine Rising State
            if (cVert.isGrounded || cVert.velocity < 0)
            {
                cVert.isRising = false;
            }
            else if (cVert.timeSinceLastGrounded > timeSinceLastGroundedBuffer && cVert.velocity > 0)
            {
                cVert.isRising = true;
            }

            // Determine Falling State
            if (cVert.isGrounded || cVert.velocity > 0)
            {
                cVert.isFalling = false;
            }
            else if (cVert.timeSinceLastGrounded > timeSinceLastGroundedBuffer && cVert.velocity < 0)
            {
                cVert.isFalling = true;
            }

            // Apply Gravity if Ungrounded
            if (cVert.velocity > -maxVelocity && !cVert.isGrounded)
            {
                cVert.velocity -= gravityFactor * deltaTime;
            }

            // Save previous Y
            cVert.previousYValue = cTrans.Value.y;

        }).WithoutBurst().Run();
    }
}