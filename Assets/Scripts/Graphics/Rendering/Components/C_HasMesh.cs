using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct C_HasMesh : IComponentData
{
    public Resource.Mat material;
    public Resource.Mesh mesh;
    public float4 UV; // should be: 1f, 1f, frame, -row
    public Color color;
}