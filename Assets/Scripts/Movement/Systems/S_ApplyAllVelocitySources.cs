using Unity.Entities;
using Unity.Physics;

public class S_ApplyAllVelocitySources : ComponentSystem
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
            ref C_Ability_CanMoveVertically cVert) =>
            {
                cVelocity.Linear.y += cVert.velocity * deltaTime;
            });

        // Apply controller movement if not immobilized
        Entities
            .WithNone<C_State_IsImmobilized>()
            .ForEach((
            ref PhysicsVelocity cVelocity,
            ref C_Ability_CanControlLateralMovement cControlMovement) =>
            {
                cVelocity.Linear += cControlMovement.normalizedLateralDir * cControlMovement.speed * deltaTime;
            });

        // Apply force movement
        Entities.ForEach((
            ref PhysicsVelocity cVelocity,
            ref C_State_IsReactingToForce cReactingToForce) =>
            {
                cVelocity.Linear += cReactingToForce.normalizedLateralDir * cReactingToForce.speed * deltaTime;
            });

        // Apply thrown movement
        Entities.ForEach((
            ref PhysicsVelocity cVelocity,
            ref C_State_IsBeingThrown cThrown) =>
            {
                cVelocity.Linear += cThrown.normalizedLateralDir * cThrown.speed * deltaTime;
            });
    }
}