using Unity.Entities;

[UpdateAfter(typeof(S_ApplyAllVelocitySources))]
public class S_RemoveThrownWhenGrounded : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((
            Entity entity,
            ref C_Ability_CanMoveVertically cVert,
            ref C_State_IsBeingThrown cThrown) =>
        {
            if (cVert.isGrounded)
            {
                PostUpdateCommands.RemoveComponent<C_State_IsBeingThrown>(entity);
            }
        });
    }
}