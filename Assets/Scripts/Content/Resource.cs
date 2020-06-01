﻿// Resource loads Meshes and Materials at the start of the game and holds references to them.
// To add a new asset: Add an enum entry that matches the name of the asset file exactly.

using System.Collections.Generic;
using UnityEngine;

public class Resource
{
    public enum Mat
    {
        Apple,
        Brick,
        EffectDeath,
        EffectSwipe,
        Grass,
        GoblinGreen,
        HumanBald,
        Turnip,
        Tree
    }

    public enum Mesh
    {
        Block_Edge,
        Block_Lane,
        Block_Mid,
        Block_RoundCorner,
        Block_RoundEnd,
        Block_RoundLone,
        Cylinder,
        QuadBillboard,
        QuadFlat,
        Slab,
        Tree
    }

    private static bool showLog = false;
    private static bool hasLoaded = false;
    private static int numberLoaded = 0;
    private static Dictionary<string, Material> mat;
    private static Dictionary<string, UnityEngine.Mesh> mesh;

    public static void LoadResources()
    {
        if (hasLoaded) { return; }
        hasLoaded = true;

        LoadMaterials();
        LoadMeshes();

        Log("Resources Loaded: " + numberLoaded);
    }

    private static void LoadMaterials()
    {
        Material[] materialList = Resources.LoadAll<Material>("Materials");
        mat = new Dictionary<string, Material>(materialList.Length);

        foreach (Material material in materialList)
        {
            mat.Add(material.name, material);
            numberLoaded++;
            Log("Material loaded: " + material.name);
        }
    }

    private static void LoadMeshes()
    {
        UnityEngine.Mesh[] meshList = Resources.LoadAll<UnityEngine.Mesh>("Models");
        mesh = new Dictionary<string, UnityEngine.Mesh>(meshList.Length);

        foreach (UnityEngine.Mesh m in meshList)
        {
            mesh.Add(m.name, m);
            numberLoaded++;
            Log("Mesh loaded: " + m.name);
        }
    }

    private static void Log(string msg)
    {
        if (showLog)
        {
            Debug.Log(msg);
        }
    }

    public static Material GetMat(Mat m)
    {
        return mat[m.ToString()];
    }

    public static UnityEngine.Mesh GetMesh(Mesh m)
    {
        return mesh[m.ToString()];
    }
}