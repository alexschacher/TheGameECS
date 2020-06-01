
/*
using Unity.Entities;

public class S_DetermineAnimStateHurt : ComponentSystem
{
    protected override void OnUpdate()
    {
        ComponentDataFromEntity<C_State_IsInvulnerable> componentDataFromEntity = GetComponentDataFromEntity<C_State_IsInvulnerable>(true);

        Entities.ForEach((
            Entity e,
            DynamicBuffer<C_BufferElement_EntityAnimation> dynamicBuffer) =>
        {
            for (int i = 0; i < dynamicBuffer.Length; i++)
            {
                if (dynamicBuffer[i].anim == UVAnimation.Anim.Character_Hurt)
                {
                    if (componentDataFromEntity.Exists(e))
                    {
                        C_State_IsInvulnerable cInvulnerable = componentDataFromEntity[e];

                        if (dynamicBuffer[i].isAnimating == false)// && cInvulnerable.isResultOfDamage)
                        {
                            C_BufferElement_EntityAnimation bufferElement = dynamicBuffer[i];
                            bufferElement.isAnimating = true;
                            dynamicBuffer.Add(bufferElement);
                            dynamicBuffer.RemoveAt(i);
                        }
                        else if (dynamicBuffer[i].isAnimating == true)// && !cInvulnerable.isResultOfDamage)
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