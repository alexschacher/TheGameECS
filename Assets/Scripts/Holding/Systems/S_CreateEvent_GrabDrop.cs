using Unity.Entities;

public class S_CreateEvent_GrabDrop : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity e, ref C_Ability_CanHold cCanHold) =>
        {
            bool isAttemptingAction = cCanHold.isAttemptingAction;
            bool isImmobilized = EntityManager.HasComponent<C_State_IsImmobilized>(e);
            bool isHurt = EntityManager.HasComponent<C_State_IsHurt>(e);
            bool isHoldingSomething = EntityManager.HasComponent<C_State_IsHolding>(e);
            bool isControllingMovement = false;
            if (EntityManager.HasComponent<C_Ability_CanControlLateralMovement>(e))
            {
                isControllingMovement = GetComponentDataFromEntity<C_Ability_CanControlLateralMovement>(true)[e].isControllingMovement;
            }

            if (isHoldingSomething)
            {
                if (isAttemptingAction || isImmobilized || isHurt)
                {
                    C_Event_Drop eventDrop = new C_Event_Drop();
                    if (isControllingMovement && !isImmobilized)
                    {
                        if (EntityManager.HasComponent<C_Ability_FacesDirection>(e))
                        {
                            eventDrop.isThrowing = true;
                            eventDrop.throwNormalizedLateralDir = GetComponentDataFromEntity<C_Ability_FacesDirection>(true)[e].normalizedLateralDir;
                        }
                    }
                    EntityManager.AddComponentData(e, eventDrop);
                    //Log.LogMessage("New Event Drop");
                }
            }
            else if (isAttemptingAction)
            {
                EntityManager.AddComponentData(e, new C_Event_GrabAttempt());
                //Log.LogMessage("New Event Grab Attempt");
            }

        }).WithoutBurst().WithStructuralChanges().Run();
    }
}