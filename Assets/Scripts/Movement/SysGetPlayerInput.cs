using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using UnityEngine;

public class SysGetPlayerInput : ComponentSystem
{
    protected override void OnUpdate()
    {
        // Get Player Input and apply to Player Control Components

        Entities.ForEach((
            ref CompPlayerControlsMovement cPlayerControlsMovement) =>
        {
            Vector3 inputVector = MovementVectorCameraConverter.convertMovementVector(
                Input.GetAxisRaw("Walk_Vertical_P1"),
                Input.GetAxisRaw("Walk_Horizontal_P1"));

            cPlayerControlsMovement.moveDirIntention.x = inputVector.x;
            cPlayerControlsMovement.moveDirIntention.y = inputVector.z;
        });
    }
}