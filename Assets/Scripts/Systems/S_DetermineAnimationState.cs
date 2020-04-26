using Unity.Entities;
using Unity.Physics;

public class S_DetermineAnimationState : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((
            Entity e,
            ref C_Animates cAnim,
            ref C_ControlsMovement cControlMove,
            ref PhysicsVelocity cVelocity) =>
        {
            if (cVelocity.Linear.x == 0 && cVelocity.Linear.z == 0)
            {
                if (cAnim.currentAnim == UVAnimation.Anim.Character_Walk)
                {
                    cAnim.currentAnim = UVAnimation.Anim.Character_Stand;
                    cAnim.animFrame = 0;
                    cAnim.animFrameTimer = 0;
                    cAnim.animSpeed = UVAnimation.anim[cAnim.currentAnim].defaultSpeed;
                }
            }
            else if (cAnim.currentAnim == UVAnimation.Anim.Character_Stand)
            {
                cAnim.currentAnim = UVAnimation.Anim.Character_Walk;
                cAnim.animFrame = 0;
                cAnim.animFrameTimer = 0;
                cAnim.animSpeed = UVAnimation.anim[cAnim.currentAnim].defaultSpeed;
            }
        });
    }
}