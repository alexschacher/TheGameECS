using Unity.Entities;

public class S_Timer_Invulnerable : ComponentSystem
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        Entities.ForEach((
            Entity entity,
            ref C_State_IsInvulnerable cInvulnerable) =>
        {
            cInvulnerable.timer -= deltaTime;

            if (cInvulnerable.timer <= 0)
            {
                PostUpdateCommands.RemoveComponent<C_State_IsInvulnerable>(entity);
            }
        });
    }
}