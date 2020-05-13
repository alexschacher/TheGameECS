using Unity.Entities;
using Unity.Physics;

public class S_ApplyVelocity : ComponentSystem
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        // Zero out velocity
        Entities.ForEach((
            ref PhysicsVelocity cVelocity) =>
            {
                cVelocity.Linear.x = 0f;
                cVelocity.Linear.y = 0f;
                cVelocity.Linear.z = 0f;
            });

        // Apply vertical movement
        Entities.ForEach((
            ref PhysicsVelocity cVelocity,
            ref C_HasVerticalMovement cVert) =>
            {
                cVelocity.Linear.y += cVert.velocity * deltaTime;
            });

        // Apply controller movement if not immobilized
        Entities
            .WithNone<C_IsImmobilized>()
            .ForEach((
            ref PhysicsVelocity cVelocity,
            ref C_ControlsMovement cControlMovement) =>
            {
                cVelocity.Linear += cControlMovement.normalizedLateralDir * cControlMovement.speed * deltaTime;
            });

        // Apply force movement
        Entities.ForEach((
            ref PhysicsVelocity cVelocity,
            ref C_ReactsToForce cReactsToForce) =>
            {
                cVelocity.Linear += cReactsToForce.normalizedLateralDir * cReactsToForce.speed * deltaTime;
            });

        // Apply thrown movement
        Entities.ForEach((
            ref PhysicsVelocity cVelocity,
            ref C_HasBeenThrown cThrown) =>
            {
                cVelocity.Linear += cThrown.normalizedLateralDir * cThrown.speed * deltaTime;
            });
    }
}