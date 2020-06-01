using Unity.Entities;

public class S_Timer_Immobilized : ComponentSystem
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        Entities.ForEach((
            Entity entity,
            ref C_State_IsImmobilized cImmobilized) =>
        {
            cImmobilized.timer -= deltaTime;

            if (cImmobilized.timer <= 0)
            {
                PostUpdateCommands.RemoveComponent<C_State_IsImmobilized>(entity);
            }
        });
    }
}