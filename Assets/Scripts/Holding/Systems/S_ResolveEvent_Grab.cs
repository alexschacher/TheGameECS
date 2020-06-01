using Unity.Entities;
using Unity.Mathematics;

[UpdateAfter(typeof(S_Collision_EventGrab_CanBeHeld))]
public class S_ResolveEvent_Grab : SystemBase
{
    private float3 holdPositionOffset = new float3(0, 0.69f, 0);

    protected override void OnUpdate()
    {
        // Remove Event Grab Attempt
        Entities.WithAll<C_Event_GrabAttempt>().ForEach((Entity grabber) =>
        {
            EntityManager.RemoveComponent<C_Event_GrabAttempt>(grabber);
            //Log.LogMessage("Event Grab Attempt is removed: " + !EntityManager.HasComponent<C_Event_GrabAttempt>(grabber));

        }).WithStructuralChanges().Run();

        // Process Event Grab Found
        Entities.ForEach((Entity grabber, ref C_Event_GrabFound eventGrabFound) =>
        {
            if (EntityManager.Exists(eventGrabFound.entityToGrab))
            {
                if (!EntityManager.HasComponent<C_State_IsHeld>(eventGrabFound.entityToGrab) && !EntityManager.HasComponent<C_State_IsHolding>(grabber))
                {
                    EntityManager.AddComponentData(grabber, new C_State_IsHolding { heldEntity = eventGrabFound.entityToGrab });
                    EntityManager.AddComponentData(eventGrabFound.entityToGrab, new C_State_IsHeld { entityHeldBy = grabber });
                    EntityManager.AddComponentData(eventGrabFound.entityToGrab, new C_Ability_SnapsToEntityPosition { entityToSnapTo = grabber, offset = holdPositionOffset });

                    //Log.LogMessage("Item is grabbed");
                }
            }
            EntityManager.RemoveComponent<C_Event_GrabFound>(grabber);
            //Log.LogMessage("Event Grab Found is removed: " + !EntityManager.HasComponent<C_Event_GrabFound>(grabber));

        }).WithStructuralChanges().Run();
    }
}