using Unity.Entities;
using UnityEngine;

public class S_GetPlayerInput : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((
            ref C_IsControlledByPlayer cPlayerControl,
            ref C_ControlsMovement cControlMover) =>
            {
                cControlMover.normalizedLateralDir = MovementVectorCameraConverter.ConvertAndNormalizeInputVector(
                    Input.GetAxisRaw("Walk_Vertical_P" + cPlayerControl.playerID),
                    Input.GetAxisRaw("Walk_Horizontal_P" + cPlayerControl.playerID));
            });

        Entities.ForEach((
            ref C_IsControlledByPlayer cPlayerControl,
            ref C_CanJump cJumper) =>
            {
                cJumper.isAttemptingJump = Input.GetButtonDown("Jump_P" + cPlayerControl.playerID);
            });

        Entities.ForEach((
            ref C_IsControlledByPlayer cPlayerControl,
            ref C_CanHold cHolder) =>
            {
                cHolder.isAttemptingAction = Input.GetButtonDown("Hold_P" + cPlayerControl.playerID);
            });

        Entities.ForEach((
            ref C_IsControlledByPlayer cPlayerControl,
            ref C_CanAttack cAttack) =>
            {
                cAttack.isAttemptingAction = Input.GetButtonDown("Attack_P" + cPlayerControl.playerID);
            });
        }
}