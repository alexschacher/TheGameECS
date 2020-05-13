using Unity.Entities;
using UnityEngine;

public class S_FlashColor : ComponentSystem
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        Entities.ForEach((
            Entity e,
            ref C_IsFlashingColor cFlashColor,
            ref C_HasMesh cRenderMesh) =>
        {
            cFlashColor.endTimer -= deltaTime;
            cFlashColor.onOffTimer -= deltaTime;

            if (cFlashColor.endTimer <= 0)
            {
                EntityManager.RemoveComponent(e, typeof(C_IsFlashingColor));
                cRenderMesh.color = Color.white;
            }

            if (cFlashColor.onOffTimer <= 0)
            {
                if (cFlashColor.on)
                {
                    cFlashColor.on = false;
                    cFlashColor.onOffTimer = cFlashColor.offTime;
                    cRenderMesh.color = Color.white;
                }
                else
                {
                    cFlashColor.on = true;
                    cFlashColor.onOffTimer = cFlashColor.onTime;
                    cRenderMesh.color = cFlashColor.color;
                }
            }
        });
    }
}