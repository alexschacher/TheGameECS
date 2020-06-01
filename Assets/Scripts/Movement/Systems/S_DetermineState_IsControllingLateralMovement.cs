using Unity.Entities;

public class S_DetermineState_IsControllingLateralMovement : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity e, ref C_Ability_CanControlLateralMovement cControlMovement) =>
        {
            bool isAttempingToMove = !(cControlMovement.normalizedLateralDir.x == 0f && cControlMovement.normalizedLateralDir.z == 0);
            bool canMove = !EntityManager.HasComponent(e, typeof(C_State_IsImmobilized));

            if (isAttempingToMove && canMove)
            {
                cControlMovement.isControllingMovement = true;
            }
            else
            {
                cControlMovement.isControllingMovement = false;
            }

        }).WithStructuralChanges().Run();
    }
}