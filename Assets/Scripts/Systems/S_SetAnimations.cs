using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

// Starts the standing or walking animation based on Velocity

// FIXME: Doesnt work because the parent entity owns velocity and movement, and the child entity owns animation........!!>!>!>!>!>>!ff

public class S_SetAnimations : ComponentSystem
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
                    UVAnimation.StartAnimation(cAnim, UVAnimation.Anim.Character_Stand);
                    Debug.Log("start stand");
                }
            }
            else if (cAnim.currentAnim == UVAnimation.Anim.Character_Stand)
            {
                UVAnimation.StartAnimation(cAnim, UVAnimation.Anim.Character_Walk);
                Debug.Log("start walk");
            }
        });
    }
}