// SysAnimateUV:
// - Runs the timers keeping track of frames on entities with CompAnimatedUV using animation speed
// - Sets UV coords on CompRenderMesh to the correct frame to display
// - Destroys the entitity if it is tagged to destroyAfterAnim in CompAnimatedUV (Not yet implemented)

using Unity.Entities;
using Unity.Mathematics;

public class S_AnimateUVs : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((
            Entity e,
            ref C_Animates cAnim,
            ref CompRenderMesh cRenderMesh) =>
        {
            int numOfFrames = UVAnimation.anim[cAnim.currentAnim].coords.Count;

            cAnim.animFrameTimer += cAnim.animSpeed * Time.DeltaTime;
            while (cAnim.animFrameTimer >= 1)
            {
                cAnim.animFrameTimer--;
                cAnim.animFrame++;
            }

            if (cAnim.animFrame >= numOfFrames)
            {
                cAnim.animFrame -= numOfFrames;

                if (cAnim.destroyAfterAnim)
                {
                    // Destroy entity: Not yet implemented
                }
            }

            cRenderMesh.UV = new float4(
                1f, // UV Width
                1f, // UV Height
                UVAnimation.anim[cAnim.currentAnim].coords[cAnim.animFrame].x, // UV X
                -UVAnimation.anim[cAnim.currentAnim].coords[cAnim.animFrame].y // UV Y (negative)
            );
        });
    }
}