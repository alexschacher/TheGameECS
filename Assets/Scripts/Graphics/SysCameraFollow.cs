using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class SysCameraFollow : ComponentSystem
{
    protected override void OnUpdate()
    {
        // Find all entities with CompCameraFollows and send an averaged position to CameraController.

        List<float3> targetPositions = new List<float3>();
        float3 averageTargetPosition = new float3(0f, 0f, 0f);

        Entities.ForEach((
            Entity e,
            ref Translation trans,
            ref CompCameraFollows c) =>
        {
            targetPositions.Add(trans.Value);
        });

        for (int i = 0; i < targetPositions.Count; i++)
        {
            averageTargetPosition += targetPositions[i];
        }

        if (targetPositions.Count > 0)
        {
            averageTargetPosition = averageTargetPosition / targetPositions.Count;
            CameraController.Instance.SetFollowTargetPosition(averageTargetPosition);
        }
    }
}