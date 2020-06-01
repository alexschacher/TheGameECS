using Unity.Entities;

public class S_Timer_IsAttacking : ComponentSystem
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        Entities.ForEach((
            Entity entity,
            ref C_State_IsAttacking cAttacking) =>
        {
            cAttacking.timer -= deltaTime;

            if (cAttacking.timer <= 0)
            {
                PostUpdateCommands.RemoveComponent<C_State_IsAttacking>(entity);
            }
        });
    }
}