
/*
using Unity.Entities;

public class S_DetermineAnimStateAttack : ComponentSystem
{
    protected override void OnUpdate()
    {
        ComponentDataFromEntity<C_State_IsImmobilized> componentDataFromEntity = GetComponentDataFromEntity<C_State_IsImmobilized>(true);

        Entities.ForEach((
            Entity e,
            DynamicBuffer<C_BufferElement_EntityAnimation> dynamicBuffer,
            ref C_Ability_CanAttack cAttacks) =>
        {
            for (int i = 0; i < dynamicBuffer.Length; i++)
            {
                if (dynamicBuffer[i].anim == UVAnimation.Anim.Character_Attack)
                {
                    if (componentDataFromEntity.Exists(e))
                    {
                        C_State_IsImmobilized cImmobilized = componentDataFromEntity[e];

                        if (dynamicBuffer[i].isAnimating == false)// && cImmobilized.isResultOfAttacking)
                        {
                            C_BufferElement_EntityAnimation bufferElement = dynamicBuffer[i];
                            bufferElement.isAnimating = true;
                            dynamicBuffer.Add(bufferElement);
                            dynamicBuffer.RemoveAt(i);
                        }
                        else if (dynamicBuffer[i].isAnimating == true)// && !cImmobilized.isResultOfAttacking)
                        {
                            C_BufferElement_EntityAnimation bufferElement = dynamicBuffer[i];
                            bufferElement.isAnimating = false;
                            dynamicBuffer.Add(bufferElement);
                            dynamicBuffer.RemoveAt(i);
                        }
                    }
                    else
                    {
                        if (dynamicBuffer[i].isAnimating == true)
                        {
                            C_BufferElement_EntityAnimation bufferElement = dynamicBuffer[i];
                            bufferElement.isAnimating = false;
                            dynamicBuffer.Add(bufferElement);
                            dynamicBuffer.RemoveAt(i);
                        }
                    }
                }
            }
        });
    }
}

*/