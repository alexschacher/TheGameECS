using Unity.Entities;

public enum AnimationStateDeterminer
{
    Default, IsAttacking, IsFalling, IsRising, IsHurt, IsControllingLateralMovement
}

public class S_ChooseAnimation : ComponentSystem
{
    private int highestPriority, highestPriorityIndex;

    protected override void OnUpdate()
    {
        Entities.ForEach((
            Entity e,
            DynamicBuffer<C_BufferElement_EntityAnimation> dynamicBuffer,
            ref C_Animates cAnim) =>
        {
            // Get index of highest priority animation that is animating
            highestPriority = -1;

            for (int i = 0; i < dynamicBuffer.Length; i++)
            {
                bool isAnimating = false;

                switch (dynamicBuffer[i].determiner)
                {
                    case AnimationStateDeterminer.Default: isAnimating = true; break;
                    case AnimationStateDeterminer.IsAttacking: isAnimating = EntityManager.HasComponent<C_State_IsAttacking>(e); break;
                    case AnimationStateDeterminer.IsHurt: isAnimating = EntityManager.HasComponent<C_State_IsHurt>(e); break;
                    case AnimationStateDeterminer.IsFalling:
                        {
                            if (EntityManager.HasComponent<C_Ability_CanMoveVertically>(e))
                            {
                                isAnimating = GetComponentDataFromEntity<C_Ability_CanMoveVertically>()[e].isFalling;
                            }
                            break;
                        }
                    case AnimationStateDeterminer.IsRising:
                        {
                            if (EntityManager.HasComponent<C_Ability_CanMoveVertically>(e))
                            {
                                isAnimating = GetComponentDataFromEntity<C_Ability_CanMoveVertically>()[e].isRising;
                            }
                            break;
                        }
                    case AnimationStateDeterminer.IsControllingLateralMovement:
                        {
                            if (EntityManager.HasComponent<C_Ability_CanControlLateralMovement>(e))
                            {
                                isAnimating = GetComponentDataFromEntity<C_Ability_CanControlLateralMovement>()[e].isControllingMovement;
                            }
                            break;
                        }
                    default: break;
                }

                if (isAnimating && dynamicBuffer[i].priority > highestPriority)
                {
                    highestPriority = dynamicBuffer[i].priority;
                    highestPriorityIndex = i;
                }
            }

            // Change animation if it is different than current
            if (dynamicBuffer[highestPriorityIndex].anim != cAnim.currentAnim)
            {
                cAnim.currentAnim = dynamicBuffer[highestPriorityIndex].anim;
                cAnim.animSpeed = dynamicBuffer[highestPriorityIndex].speed;
                cAnim.animFrame = 0;
                cAnim.animFrameTimer = 0;
            }
        });
    }
}