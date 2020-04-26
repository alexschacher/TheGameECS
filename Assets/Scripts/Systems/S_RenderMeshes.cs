using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class S_RenderMeshes : ComponentSystem
{
    MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
    Vector4[] uv = new Vector4[1];
    int shaderPropertyId = Shader.PropertyToID("_MainTex_UV");

    Vector3 billboardRotationMod = new Vector3(0f, 0.3f, 0f);
    Camera camera = Camera.main;
    Vector3 billboardScale = new Vector3(1f, 1f, 1f);
    Quaternion billboardRotation;
    float3 up = new float3(0, 1, 0);

    float billboardFlipBuffer = 0.5f;

    protected override void OnUpdate()
    {
        camera = Camera.main;
        RenderNonBillboards();
        RenderBillboards();
    }

    private void RenderNonBillboards()
    {
        Entities.WithNone<C_IsBillboarded>()
            .ForEach((
            ref LocalToWorld cLocalToWorld,
            ref Rotation cRot,
            ref C_RendersMesh cRenderMesh) =>
        {
            DrawMesh(cRenderMesh, Matrix4x4.TRS(cLocalToWorld.Position, cRot.Value, Vector3.one));
        });
    }

    private void RenderBillboards()
    {
        Vector3 normalizedLateralCamDir = new Vector3(camera.transform.right.x, 0f, camera.transform.right.z).normalized;

        Entities.ForEach((
            ref LocalToWorld cLocalToWorld,
            ref C_FacesDirection cFacing,
            ref C_IsBillboarded cBillboard,
            ref C_RendersMesh cRenderMesh) =>
        {
            billboardRotation = quaternion.LookRotation(camera.transform.forward + billboardRotationMod, up);
            
            if (cBillboard.flipTexureFacing == true)
            {
                // FIXME: sometimes flickers left and right
                if (Vector3.Dot(cFacing.dirFacing, normalizedLateralCamDir) < billboardFlipBuffer)
                {
                    cBillboard.xScale = -1;
                }
                else if (Vector3.Dot(cFacing.dirFacing, normalizedLateralCamDir) > billboardFlipBuffer)
                {
                    cBillboard.xScale = 1;
                }
            }

            billboardScale.x = cBillboard.xScale;

            DrawMesh(cRenderMesh, Matrix4x4.TRS(cLocalToWorld.Position, billboardRotation, billboardScale));
        });
    }

    private void DrawMesh(C_RendersMesh cRenderMesh, Matrix4x4 transMatrix)
    {
        uv[0] = cRenderMesh.UV;
        materialPropertyBlock.SetVectorArray(shaderPropertyId, uv);

        Graphics.DrawMesh(
            Resource.GetMesh(cRenderMesh.mesh),
            transMatrix,
            Resource.GetMat(cRenderMesh.material),
            0,
            camera,
            0,
            materialPropertyBlock
        );
    }
}