using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine.Rendering;
using Unity.Mathematics;
using Unity.Transforms;

public class Startup : MonoBehaviour
{
    public Mesh blockMesh;
    public Material terrainMaterial;
    public Material characterMaterial;
    public int levelWidth, levelLength;

    private void Start()
    {
        Resource.LoadResources();
        EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;

        // Create level
        for (int x = 0; x < levelLength; x++)
        {
            EntityFactory.Instantiate_Terrain_Grass(new float3(x, 0.5f, 0));

            for (int z = 0; z < levelWidth; z++)
            {
                // Fill ground
                EntityFactory.Instantiate_Terrain_Grass(new float3(x, 0f, z));

                // Fill walls (corners will overlap here)
                if (x == 0 || z == 0 || x == levelWidth - 1 || z == levelLength - 1)
                {
                    EntityFactory.Instantiate_Terrain_Grass(new float3(x, 0.5f, z));
                }
            }
        }

        // Create player
        EntityFactory.Instantiate_Character_Player(new float3(2, 0.5f, 2));

        // Create goblin
        EntityFactory.Instantiate_Character_Goblin(new float3(7, 0.5f, 7));

        // Create apple
        EntityFactory.Instantiate_Apple(new float3(4, 0.5f, 4));
        EntityFactory.Instantiate_Apple(new float3(5, 0.5f, 6));
        EntityFactory.Instantiate_Apple(new float3(7, 0.5f, 5));
    }
}