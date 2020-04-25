using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

// Points billboards towards camera and flips them based on facing direction

public class S_ModifyBillboards : ComponentSystem
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
            ref C_IsBillboarded cBillboard,
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
            ref C_IsBillboarded cBillboard,
            ref NonUniformScale cScale,
            ref Parent cParent) =>
        {
            if (cBillboard.flipTexureFacing == false) return;

            C_FacesDirection cParentFacing = GetComponentDataFromEntity<C_FacesDirection>(true)[cParent.Value];
            Vector3 normalizedLateralCamDir = new Vector3(Camera.main.transform.right.x, 0f, Camera.main.transform.right.z).normalized;

            // FIXME: sometimes flickers left and right

            if (Vector3.Dot(cParentFacing.dirFacing, normalizedLateralCamDir) > 0f)
            {
                cScale.Value = new float3(1f, 1f, 1f);
            }
            else if (Vector3.Dot(cParentFacing.dirFacing, normalizedLateralCamDir) < 0f)
            {
                cScale.Value = new float3(-1f, 1f, 1f);
            }
        });
    }
}