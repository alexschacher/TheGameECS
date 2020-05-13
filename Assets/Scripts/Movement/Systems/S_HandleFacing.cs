using Unity.Entities;

public class S_HandleFacing : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities
            .WithNone<C_IsImmobilized>()
            .ForEach((
            ref C_ControlsMovement cControlsMovement,
            ref C_FacesDirection cFacingDir) =>
        {
            if (System.Math.Abs(cControlsMovement.normalizedLateralDir.x) > 0 || System.Math.Abs(cControlsMovement.normalizedLateralDir.y) > 0)
            {
                cFacingDir.normalizedLateralDir = cControlsMovement.normalizedLateralDir;
            }
        });
    }
}