// Definition of UV coordinates and default speed for various animations as defined in enum

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
            Anim.Character_Stand, new UVAnim
            {
                defaultSpeed = 10f,
                coords = new List<float2>
                {
                    new float2(0/4f, 0/4f),
                    new float2(1/4f, 0/4f),
                    new float2(2/4f, 0/4f),
                    new float2(3/4f, 0/4f)
                }
            }
        },
        {
            Anim.Character_Walk, new UVAnim
            {
                defaultSpeed = 10f,
                coords = new List<float2>
                {
                    new float2(0/4f, 1/4f),
                    new float2(1/4f, 1/4f),
                    new float2(2/4f, 1/4f),
                    new float2(3/4f, 1/4f)
                }
            }
        }
    };
}