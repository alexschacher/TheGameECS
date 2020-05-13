using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

[UpdateBefore(typeof(S_ApplyGravity))]
public class S_HandleHolding_Throw : ComponentSystem
{
    private float throwVerticalPower = 300f;
    private float throwLateralPower = 200f;

    protected override void OnUpdate()
    {
        Entities.ForEach((
            Entity e,
            ref C_IsHeld cIsHeld,
            ref C_HasVerticalMovement cVert,
            ref Translation cTrans) =>
        {
            Entity holder = cIsHeld.entityHeldBy;

            if (!EntityManager.Exists(holder)) // If holder doesn't exist, be dropped
            {
                cVert.velocity = throwVerticalPower;
                PostUpdateCommands.RemoveComponent(e, typeof(C_IsHeld));
                PostUpdateCommands.RemoveComponent(e, typeof(C_SnapsToEntityPosition));
                return;
            }

            ComponentDataFromEntity<C_IsImmobilized> immobilizedEntities = GetComponentDataFromEntity<C_IsImmobilized>(true);

            if (GetComponentDataFromEntity<C_CanHold>()[holder].isAttemptingAction || immobilizedEntities.Exists(holder))
            {
                C_ControlsMovement cHolderMover = GetComponentDataFromEntity<C_ControlsMovement>()[holder];
                cVert.velocity = throwVerticalPower;

                if (!cHolderMover.normalizedLateralDir.Equals(float3.zero) &&            // If holder is moving
                !GetComponentDataFromEntity<C_IsImmobilized>().HasComponent(holder))     // And holder is not immobilized
                {
                    PostUpdateCommands.AddComponent(e, new C_HasBeenThrown
                    {
                        normalizedLateralDir = GetComponentDataFromEntity<C_FacesDirection>()[cIsHeld.entityHeldBy].normalizedLateralDir,
                        speed = throwLateralPower
                    });
                }
                PostUpdateCommands.RemoveComponent(cIsHeld.entityHeldBy, typeof(C_IsHolding));
                PostUpdateCommands.RemoveComponent(e, typeof(C_IsHeld));
                PostUpdateCommands.RemoveComponent(e, typeof(C_SnapsToEntityPosition));
            }
        });
    }
}