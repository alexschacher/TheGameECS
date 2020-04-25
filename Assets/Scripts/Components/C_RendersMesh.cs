﻿using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct CompRenderMesh : IComponentData
{
    public Resource.Mat material;
    public Resource.Mesh mesh;
    public float4 UV; // should be: 1f, 1f, frame, -row
}