﻿using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct C_IsBillboarded : IComponentData
{
    public float xScale;
    public bool flipTexureFacing;
}
