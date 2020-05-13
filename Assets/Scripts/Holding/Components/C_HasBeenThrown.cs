﻿using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct C_HasBeenThrown : IComponentData
{
    public float3 normalizedLateralDir;
    public float speed;
}
