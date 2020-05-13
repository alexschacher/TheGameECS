using Unity.Entities;

public class S_HandleImmobilized : ComponentSystem
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        Entities.ForEach((
            Entity entity,
            ref C_IsImmobilized cImmobilized) =>
        {
            cImmobilized.timer -= deltaTime;

            if (cImmobilized.timer <= 0)
            {
                PostUpdateCommands.RemoveComponent<C_IsImmobilized>(entity);
            }
        });
    }
}