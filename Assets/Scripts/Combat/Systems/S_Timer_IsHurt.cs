using Unity.Entities;

public class S_Timer_IsHurt : ComponentSystem
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        Entities.ForEach((
            Entity entity,
            ref C_State_IsHurt cIsHurt) =>
        {
            cIsHurt.timer -= deltaTime;

            if (cIsHurt.timer <= 0)
            {
                PostUpdateCommands.RemoveComponent<C_State_IsHurt>(entity);
            }
        });
    }
}