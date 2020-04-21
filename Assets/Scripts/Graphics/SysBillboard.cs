using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class SysBillboard : ComponentSystem
{
    protected override void OnUpdate()
    {
        PointTowardsCamera();
        FlipTextureFacing();
    }

    private void PointTowardsCamera()
    {
        Vector3 rotationMod = new Vector3(0f, 0.3f, 0f);
        Entities.ForEach((
            ref CompBillboard cBillboard,
            ref Rotation cRotation) =>
        {
            cRotation.Value = quaternion.LookRotation(Camera.main.transform.forward + rotationMod, new float3(0, 1, 0));

            // Cancel out Parent Rotation: Code doesnt work. Cant figure it out.
            // Not necessary unless parent objects need to rotate for some reason.
            // LocalToWorld cParentRotation = GetComponentDataFromEntity<LocalToWorld>(true)[cParent.Value];
            // cRotation.Value.value = cRotation.Value.value * Quaternion.Inverse(cParentRotation.Rotation.value);
        });
    }

    private void FlipTextureFacing()
    {
        Entities.ForEach((
            ref CompBillboard cBillboard,
            ref NonUniformScale cScale,
            ref Parent cParent) =>
        {
            CompMoves cParentMoves = GetComponentDataFromEntity<CompMoves>(true)[cParent.Value];
            Vector3 normalizedLateralCamDir = new Vector3(Camera.main.transform.right.x, 0f, Camera.main.transform.right.z).normalized;

            // FIXME: sometimes flickers left and right

            if (Vector3.Dot(cParentMoves.facingDir, normalizedLateralCamDir) > 0f)
            {
                cScale.Value = new float3(1f, 1f, 1f);
            }
            else if (Vector3.Dot(cParentMoves.facingDir, normalizedLateralCamDir) < 0f)
            {
                cScale.Value = new float3(-1f, 1f, 1f);
            }
        });
    }
}