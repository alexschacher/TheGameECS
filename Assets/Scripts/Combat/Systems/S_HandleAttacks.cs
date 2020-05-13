using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public class S_HandleAttacks : ComponentSystem
{
    private Quaternion defaultSwipeRotation = Quaternion.Euler(90f, 0f, 0f);
    private float3 swipeOffset = new float3(0f, 0.3f, 0f);
    private float attackTime = 0.25f;

    protected override void OnUpdate()
    {
        Entities
            .WithNone<C_IsImmobilized>()
            .ForEach((
            Entity entity,
            ref Translation cTrans,
            ref C_FacesDirection cFacingDir,
            ref C_CanAttack cAttack) =>
        {
            if (cAttack.isAttemptingAction)
            {
                Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, cFacingDir.normalizedLateralDir) * defaultSwipeRotation;
                EntityFactory.InstantiateEffectSwipe(entity, cTrans.Value + swipeOffset, rotation);
                PostUpdateCommands.AddComponent(entity, new C_IsImmobilized { timer = attackTime });
            } 
        });
    }
}