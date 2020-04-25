using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using UnityEngine;

public class S_GetPlayerInput : ComponentSystem
{
    protected override void OnUpdate()
    {
        // Get Player Input and apply to Player Control Components

        Entities.ForEach((
            ref C_IsControlledByPlayer cPlayerControlsMovement) =>
        {
            Vector3 inputVector = MovementVectorCameraConverter.convertMovementVector(
                Input.GetAxisRaw("Walk_Vertical_P1"),
                Input.GetAxisRaw("Walk_Horizontal_P1"));

            cPlayerControlsMovement.moveDirIntention.x = inputVector.x;
            cPlayerControlsMovement.moveDirIntention.y = inputVector.z;
        });

        // Update CompControlsMovement with Components that would control it, such as Player or AI

        Entities.ForEach((
            ref C_IsControlledByPlayer cPlayerControlsMovement,
            ref C_ControlsMovement cControlMover) =>
        {
            cControlMover.dirIntention = cPlayerControlsMovement.moveDirIntention;
        });
    }
}