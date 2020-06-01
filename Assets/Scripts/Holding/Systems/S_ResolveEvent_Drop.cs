using Unity.Entities;

[UpdateAfter(typeof(S_CreateEvent_GrabDrop))]
public class S_ResolveEvent_Drop : ComponentSystem
{
    private float throwVerticalPower = 300f;
    private float throwLateralPower = 200f;

    protected override void OnUpdate()
    {
        // Process Drop Events
        Entities.ForEach((Entity holder, ref C_Event_Drop cEventDrop, ref C_State_IsHolding cIsHolding) =>
        {
            if (EntityManager.Exists(cIsHolding.heldEntity))
            {
                DropEntity(cIsHolding.heldEntity);

                if (cEventDrop.isThrowing)
                {
                    EntityManager.AddComponentData(cIsHolding.heldEntity, new C_State_IsBeingThrown { normalizedLateralDir = cEventDrop.throwNormalizedLateralDir, speed = throwLateralPower });
                }
            }
            EntityManager.RemoveComponent<C_State_IsHolding>(holder);
            EntityManager.RemoveComponent<C_Event_Drop>(holder);
        });

        // Check held items, if holder doesn't exist, be dropped
        Entities.ForEach((Entity heldEntity, ref C_State_IsHeld cIsHeld) =>
        {
            if (!EntityManager.Exists(cIsHeld.entityHeldBy))
            {
                DropEntity(heldEntity);
            }
        });
    }

    private void DropEntity(Entity e)
    {
        EntityManager.RemoveComponent<C_State_IsHeld>(e);
        EntityManager.RemoveComponent<C_Ability_SnapsToEntityPosition>(e);

        if (EntityManager.HasComponent<C_Ability_CanMoveVertically>(e))
        {
            C_Ability_CanMoveVertically cVert = GetComponentDataFromEntity<C_Ability_CanMoveVertically>()[e];
            cVert.velocity = throwVerticalPower;
            EntityManager.SetComponentData(e, cVert);
        }
    }
}