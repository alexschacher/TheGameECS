using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class S_ResolveInput_Attack : ComponentSystem
{
    private float3 swipeOffset = new float3(0f, 0.3f, 0f);
    private float attackTime = 0.25f;

    protected override void OnUpdate()
    {
        Entities
            .WithNone<C_State_IsImmobilized>()
            .ForEach((
            Entity entity,
            ref Translation cTrans,
            ref C_Ability_FacesDirection cFacingDir,
            ref C_Ability_CanAttack cAttack) =>
        {
            if (cAttack.isAttemptingAction)
            {
                cAttack.isAttemptingAction = false;
                Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, cFacingDir.normalizedLateralDir);
                EntityFactory.InstantiateEffectSwipe(entity, cTrans.Value + swipeOffset, rotation);
                EntityManager.AddComponentData(entity, new C_State_IsImmobilized { timer = attackTime });
                EntityManager.AddComponentData(entity, new C_State_IsAttacking { timer = attackTime });
            } 
        });
    }
}