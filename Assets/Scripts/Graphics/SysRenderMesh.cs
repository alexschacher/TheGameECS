using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class SysRenderMesh : ComponentSystem
{
    MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
    Vector4[] uv = new Vector4[1];
    int shaderPropertyId = Shader.PropertyToID("_MainTex_UV");

    protected override void OnUpdate()
    {
        Entities.WithNone<NonUniformScale>()
            .ForEach((
            ref LocalToWorld cTrans,
            ref Rotation cRot,
            ref CompRenderMesh cRenderMesh) =>
        {
            DrawMesh(cRenderMesh, Matrix4x4.TRS(cTrans.Position, cRot.Value, Vector3.one));
        });

        Entities
            .ForEach((
            ref LocalToWorld cTrans,
            ref Rotation cRot,
            ref NonUniformScale cScale,
            ref CompRenderMesh cRenderMesh) =>
            {
                DrawMesh(cRenderMesh, Matrix4x4.TRS(cTrans.Position, cRot.Value, cScale.Value));
            });
    }

    private void DrawMesh(CompRenderMesh cRenderMesh, Matrix4x4 transMatrix)
    {
        uv[0] = cRenderMesh.UV;
        materialPropertyBlock.SetVectorArray(shaderPropertyId, uv);

        Graphics.DrawMesh(
                Resource.GetMesh(cRenderMesh.mesh),
                transMatrix,
                Resource.GetMat(cRenderMesh.material),
                0,
                Camera.main,
                0,
                materialPropertyBlock
            );
    }
}