using Unity.Entities;

public class S_Timer_IsReactingToForce : ComponentSystem
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        Entities.ForEach((
            Entity entity,
            ref C_State_IsReactingToForce cIsReacting) =>
        {
            cIsReacting.timer -= deltaTime;

            if (cIsReacting.timer <= 0)
            {
                PostUpdateCommands.RemoveComponent<C_State_IsReactingToForce>(entity);
            }
        });
    }
}