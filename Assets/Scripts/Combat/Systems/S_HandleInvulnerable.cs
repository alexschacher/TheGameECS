using Unity.Entities;

public class S_HandleInvulnerable : ComponentSystem
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        Entities.ForEach((
            Entity entity,
            ref C_IsInvulnerable cInvulnerable) =>
        {
            cInvulnerable.timer -= deltaTime;

            if (cInvulnerable.timer <= 0)
            {
                PostUpdateCommands.RemoveComponent<C_IsInvulnerable>(entity);
            }
        });
    }
}