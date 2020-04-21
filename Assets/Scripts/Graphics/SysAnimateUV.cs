// SysAnimateUV:
// - Runs the timers keeping track of frames on entities with CompAnimatedUV using animation speed
// - Sets UV coords on CompRenderMesh to the correct frame to display
// - Destroys the entitity if it is tagged to destroyAfterAnim in CompAnimatedUV (Not yet implemented)

using Unity.Entities;
using Unity.Mathematics;

public class SysAnimateUV : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((
            Entity e,
            ref CompAnimatedUV cAnim,
            ref CompRenderMesh cRenderMesh) =>
        {
            int numOfFrames = UVAnimation.anim[cAnim.currentAnim].coords.Count;

            cAnim.animFrameTimer += cAnim.animSpeed * Time.DeltaTime; // Advance frame timer based on speed and delta time
            while (cAnim.animFrameTimer >= 1) // Increase 1 frame
            {
                cAnim.animFrameTimer--;
                cAnim.animFrame++;
            }

            if (cAnim.animFrame >= numOfFrames) // If reached end of animation
            {
                cAnim.animFrame -= numOfFrames; // Loop frames back to beginning

                if (cAnim.destroyAfterAnim) // If tagged to destroy entity at animation end
                {
                    // Destroy entity: Not yet implemented
                }
            }

            cRenderMesh.UV = new float4( // Set UV Coords according to current animation and frame
                1f, // UV Width
                1f, // UV Height
                UVAnimation.anim[cAnim.currentAnim].coords[cAnim.animFrame].x, // UV X
                -UVAnimation.anim[cAnim.currentAnim].coords[cAnim.animFrame].y // UV Y (negative)
            );
        });
    }
}