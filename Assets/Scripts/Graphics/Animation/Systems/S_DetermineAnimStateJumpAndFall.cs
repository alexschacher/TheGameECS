
/*
using Unity.Entities;

public class S_DetermineAnimStateJumpAndFall : ComponentSystem
{
    private float timeSinceLastGroundedBuffer = 0.1f;

    protected override void OnUpdate()
    {
        Entities.ForEach((
            Entity e,
            DynamicBuffer<C_BufferElement_EntityAnimation> dynamicBuffer,
            ref C_Ability_CanMoveVertically cVertMovement,
            ref C_Ability_CanJump cJump) =>
        {
            for (int i = 0; i < dynamicBuffer.Length; i++)
            {
                if (dynamicBuffer[i].anim == UVAnimation.Anim.Character_Jump)
                {
                    // Try to stop jump anim
                    if (cVertMovement.isGrounded || cVertMovement.velocity < 0)
                    {
                        if (dynamicBuffer[i].isAnimating == true)
                        {
                            C_BufferElement_EntityAnimation bufferElement = dynamicBuffer[i];
                            bufferElement.isAnimating = false;
                            dynamicBuffer.Add(bufferElement);
                            dynamicBuffer.RemoveAt(i);
                        }
                    }
                    
                    // Try to start jump anim
                    else if (cJump.timeSinceLastGrounded > timeSinceLastGroundedBuffer && cVertMovement.velocity > 0)
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


                if (dynamicBuffer[i].anim == UVAnimation.Anim.Character_Fall)
                {
                    // Try to stop fall anim
                    if (cVertMovement.isGrounded || cVertMovement.velocity > 0)
                    {
                        if (dynamicBuffer[i].isAnimating == true)
                        {
                            C_BufferElement_EntityAnimation bufferElement = dynamicBuffer[i];
                            bufferElement.isAnimating = false;
                            dynamicBuffer.Add(bufferElement);
                            dynamicBuffer.RemoveAt(i);
                        }
                    }

                    // Try to start fall anim
                    else if (cJump.timeSinceLastGrounded > timeSinceLastGroundedBuffer && cVertMovement.velocity < 0)
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