using Unity.Entities;
using Unity.Mathematics;

public class S_HandleForceTimers : ComponentSystem
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        Entities.ForEach((
            ref C_ReactsToForce cReactsToForce) =>
        {
            if (cReactsToForce.timer <= 0) return;

            cReactsToForce.timer -= deltaTime;

            if (cReactsToForce.timer <= 0)
            {
                cReactsToForce.normalizedLateralDir = float3.zero;
                cReactsToForce.speed = 0f;
                cReactsToForce.timer = 0f;
            }
        });
    }
}