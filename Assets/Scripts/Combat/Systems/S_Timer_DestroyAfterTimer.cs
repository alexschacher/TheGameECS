﻿using Unity.Entities;

public class S_Timer_DestroyAfterTimer : ComponentSystem
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        Entities.ForEach((
            Entity entity,
            ref C_State_IsDestroyedAfterTimer cIsDestroyed) =>
        {
            cIsDestroyed.timer -= deltaTime;

            if (cIsDestroyed.timer <= 0)
            {
                PostUpdateCommands.DestroyEntity(entity);
            }
        });
    }
}