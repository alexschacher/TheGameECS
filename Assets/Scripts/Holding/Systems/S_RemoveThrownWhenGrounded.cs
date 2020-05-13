using Unity.Entities;

[UpdateAfter(typeof(S_ApplyVelocity))]
public class S_RemoveThrownWhenGrounded : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((
            Entity entity,
            ref C_HasVerticalMovement cVert,
            ref C_HasBeenThrown cThrown) =>
        {
            if (cVert.isGrounded)
            {
                PostUpdateCommands.RemoveComponent<C_HasBeenThrown>(entity);
            }
        });
    }
}