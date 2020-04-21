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

        // Create Level Entity
        Entity levelEntity = manager.CreateEntity(typeof(Level));
        manager.SetComponentData(levelEntity, new Level { Size = new int2(levelWidth, levelLength) });

        // Create level
        for (int x = 0; x < levelLength; x++)
        {
            for (int z = 0; z < levelWidth; z++)
            {
                // Fill with ground
                EntityFactory.Instantiate_Terrain_Grass(new float3(x, 0, z));

                // Fill walls (corners will overlap here)
                if (x == 1 || z == 1 || x == levelWidth - 2 || z == levelLength - 2)
                {
                    EntityFactory.Instantiate_Terrain_Brick(new float3(x, 0.5f, z));
                }

                // Fill middle with NPCs

                if (x > 12 && z > 12 && x < levelWidth - 12 && z < levelLength - 12)
                {
                    EntityFactory.Instantiate_Character_NPC(new float3(x, 0.5f, z));
                }
            }
        }

        // Create player
        EntityFactory.Instantiate_Character_Player(new float3(2, 0.5f, 2));
    }
}