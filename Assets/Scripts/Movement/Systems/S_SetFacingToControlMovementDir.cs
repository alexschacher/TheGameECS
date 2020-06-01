using Unity.Entities;

public class S_SetFacingToControlMovementDir : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities
            .WithNone<C_State_IsImmobilized>()
            .ForEach((
            ref C_Ability_CanControlLateralMovement cControlsMovement,
            ref C_Ability_FacesDirection cFacingDir) =>
        {
            if (System.Math.Abs(cControlsMovement.normalizedLateralDir.x) > 0 || System.Math.Abs(cControlsMovement.normalizedLateralDir.y) > 0)
            {
                cFacingDir.normalizedLateralDir = cControlsMovement.normalizedLateralDir;
            }
        });
    }
}