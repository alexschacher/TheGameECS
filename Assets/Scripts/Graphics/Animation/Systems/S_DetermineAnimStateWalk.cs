/*
using Unity.Entities;

public class S_DetermineAnimStateWalk : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((
            Entity e,
            DynamicBuffer<C_BufferElement_EntityAnimation> dynamicBuffer,
            ref C_Ability_CanControlLateralMovement cMover) =>
        {
            for (int i = 0; i < dynamicBuffer.Length; i++)
            {
                if (dynamicBuffer[i].anim == UVAnimation.Anim.Character_Walk)
                {
                    if (cMover.normalizedLateralDir.x == 0f && cMover.normalizedLateralDir.z == 0)
                    {
                        if (dynamicBuffer[i].isAnimating == true)
                        {
                            C_BufferElement_EntityAnimation bufferElement = dynamicBuffer[i];
                            bufferElement.isAnimating = false;
                            dynamicBuffer.Add(bufferElement);
                            dynamicBuffer.RemoveAt(i);
                        }
                    }
                    else
                    {
                        if (dynamicBuffer[i].isAnimating == false)
                        {
                            C_BufferElement_EntityAnimation bufferElement = dynamicBuffer[i];
                            bufferElement.isAnimating = true;
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