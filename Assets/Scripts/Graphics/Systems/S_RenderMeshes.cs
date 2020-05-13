// This system is not very performant, and will significantly slow down the game with too many entities on screen.
// For now, this renderer does enough to allow me to continue development on other game features.
// At some point this system will be replaced with a more robust official render pipeline to be released in a future Unity update.

using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class S_RenderMeshes : ComponentSystem
{
    MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
    int shaderPropertyTex = Shader.PropertyToID("_MainTex_UV");
    int shaderPropertyColor = Shader.PropertyToID("_Color");
    Camera camera;
    Vector4[] uv = new Vector4[1];
    float3 up = new float3(0, 1, 0);
    Color color;
    Quaternion billboardRotation;
    Vector3 billboardRotationMod = new Vector3(0f, 0.3f, 0f);
    Vector3 billboardScale = new Vector3(1f, 1f, 1f);
    float billboardFlipBuffer = 0.5f;

    protected override void OnStartRunning()
    {
        base.OnStartRunning();
        camera = Camera.main;
    }

    protected override void OnUpdate()
    {
        RenderNonBillboards();
        RenderBillboards();
    }

    private void RenderNonBillboards()
    {
        Entities.WithNone<C_IsBillboarded>()
            .ForEach((
            ref LocalToWorld cLocalToWorld,
            ref Rotation cRot,
            ref C_HasMesh cRenderMesh) =>
        {
            DrawMesh(cRenderMesh, Matrix4x4.TRS(cLocalToWorld.Position, cRot.Value, Vector3.one));
        });
    }

    private void RenderBillboards()
    {
        Vector3 normalizedLateralCamDir = new Vector3(camera.transform.right.x, 0f, camera.transform.right.z).normalized;

        Entities.ForEach((
            Entity e,
            ref LocalToWorld cLocalToWorld,
            ref C_FacesDirection cFacing,
            ref C_IsBillboarded cBillboard,
            ref C_HasMesh cRenderMesh) =>
        {
            billboardRotation = quaternion.LookRotation(camera.transform.forward + billboardRotationMod, up);
            billboardScale.x = cBillboard.xScale;

            if (cBillboard.flipTexureFacing == true)
            {
                if (Vector3.Dot(cFacing.normalizedLateralDir, normalizedLateralCamDir) < billboardFlipBuffer)
                {
                    cBillboard.xScale = -1;
                }
                else if (Vector3.Dot(cFacing.normalizedLateralDir, normalizedLateralCamDir) > billboardFlipBuffer)
                {
                    cBillboard.xScale = 1;
                }
            }

            DrawMesh(cRenderMesh, Matrix4x4.TRS(cLocalToWorld.Position, billboardRotation, billboardScale));
        });
    }

    private void DrawMesh(C_HasMesh cRenderMesh, Matrix4x4 transMatrix)
    {
        uv[0] = cRenderMesh.UV;
        materialPropertyBlock.SetVectorArray(shaderPropertyTex, uv);
        materialPropertyBlock.SetColor(shaderPropertyColor, cRenderMesh.color);

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