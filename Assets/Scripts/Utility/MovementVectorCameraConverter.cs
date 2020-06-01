using UnityEngine;

public static class MovementVectorCameraConverter
{
    public static Vector3 ConvertAndNormalizeInputVector(float verticalInput, float horizontalInput)
    {
        // Camera direction
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward = camForward.normalized;
        camRight = camRight.normalized;

        // Input direction
        Vector3 movementVector = (camForward * verticalInput) + (camRight * horizontalInput);
        movementVector = movementVector.normalized;

        return movementVector;
    }
}
