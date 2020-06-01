// UVAnimation holds references to all animations in the game, and their UV coordinates and animation speeds.
// To add a new animation: Add an enum entry, as well as an animation definition in the 

using System.Collections.Generic;
using Unity.Mathematics;

public struct UVAnim
{
    public List<float2> coords;
}

public static class UVAnimation
{
    public enum Anim
    {
        Character_Attack,
        Character_Fall,
        Character_Hurt,
        Character_Jump,
        Character_Stand,
        Character_Walk,
        Effect_Death
    }

    public static Dictionary<Anim, UVAnim> anim = new Dictionary<Anim, UVAnim>
    {
        { Anim.Character_Attack, new UVAnim { coords = new List<float2> {
            new float2(2/4f, 1/4f) }}},

        { Anim.Character_Fall, new UVAnim { coords = new List<float2> {
            new float2(1/4f, 1/4f) }}},

        { Anim.Character_Hurt, new UVAnim { coords = new List<float2> {
            new float2(3/4f, 1/4f) }}},

        { Anim.Character_Jump, new UVAnim { coords = new List<float2> {
            new float2(0/4f, 1/4f) }}},

        { Anim.Character_Stand, new UVAnim { coords = new List<float2> {
            new float2(0/4f, 0/4f) }}},

        { Anim.Character_Walk, new UVAnim {coords = new List<float2> {
            new float2(1/4f, 0/4f),
            new float2(2/4f, 0/4f),
            new float2(3/4f, 0/4f),
            new float2(0/4f, 0/4f) }}},

        { Anim.Effect_Death, new UVAnim { coords = new List<float2> {
            new float2(0/4f, 0/4f),
            new float2(1/4f, 0/4f),
            new float2(2/4f, 0/4f),
            new float2(3/4f, 0/4f),
            new float2(0/4f, 1/4f),
            new float2(1/4f, 1/4f),
            new float2(2/4f, 1/4f),
            new float2(3/4f, 1/4f),
            new float2(0/4f, 2/4f) }}}
    };
}