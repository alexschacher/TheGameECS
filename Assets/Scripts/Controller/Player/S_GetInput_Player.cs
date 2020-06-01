using Unity.Entities;
using UnityEngine;

public class S_GetInput_Player : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((
            ref C_Controller_Player cPlayerControl,
            ref C_Ability_CanControlLateralMovement cControlMover) =>
            {
                cControlMover.normalizedLateralDir = MovementVectorCameraConverter.ConvertAndNormalizeInputVector(
                    Input.GetAxisRaw("Walk_Vertical_P" + cPlayerControl.playerID),
                    Input.GetAxisRaw("Walk_Horizontal_P" + cPlayerControl.playerID));
            });

        Entities.ForEach((
            ref C_Controller_Player cPlayerControl,
            ref C_Ability_CanJump cJumper) =>
            {
                cJumper.isAttemptingJump = Input.GetButtonDown("Jump_P" + cPlayerControl.playerID);
            });

        Entities.ForEach((
            ref C_Controller_Player cPlayerControl,
            ref C_Ability_CanHold cHolder) =>
            {
                cHolder.isAttemptingAction = Input.GetButtonDown("Hold_P" + cPlayerControl.playerID);
            });

        Entities.ForEach((
            ref C_Controller_Player cPlayerControl,
            ref C_Ability_CanAttack cAttack) =>
            {
                cAttack.isAttemptingAction = Input.GetButtonDown("Attack_P" + cPlayerControl.playerID);
            });
        }
}