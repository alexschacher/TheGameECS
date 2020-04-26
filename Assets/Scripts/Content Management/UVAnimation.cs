// UVAnimation holds references to all animations in the game, and their UV coordinates and animation speeds.
// To add a new animation: Add an enum entry, as well as an animation definition in the 

using System.Collections.Generic;
using Unity.Mathematics;

public struct UVAnim
{
    public List<float2> coords;
    public float defaultSpeed;
}

public static class UVAnimation
{
    public enum Anim
    {
        Character_Stand,
        Character_Walk
    }

    public static Dictionary<Anim, UVAnim> anim = new Dictionary<Anim, UVAnim>
    {
        {
            Anim.Character_Stand,
            new UVAnim
            {
                defaultSpeed = 6f,
                coords = new List<float2>
                {
                    new float2(0/4f, 0/4f)
                }
            }
        },
        {
            Anim.Character_Walk,
            new UVAnim
            {
                defaultSpeed = 6f,
                coords = new List<float2>
                {
                    new float2(0/4f, 0/4f),
                    new float2(1/4f, 0/4f),
                    new float2(2/4f, 0/4f),
                    new float2(3/4f, 0/4f)
                }
            }
        }
    };
}