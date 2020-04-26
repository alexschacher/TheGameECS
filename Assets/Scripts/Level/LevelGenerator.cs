using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LevelGenerator
{
    public static void Generate(Level level)
    {
        // Stage
        FillLayer(level, EntityID.ID.TerrainGrass, 0);
        FillLayerBorders(level, EntityID.ID.TerrainGrass, 1);

        // Characters
        level.Set(EntityID.ID.CharacterPlayer, 0, 2, 0);
        level.Set(EntityID.ID.CharacterGoblin, 7, 1, 7);

        // Apples
        level.Set(EntityID.ID.ItemApple, 4, 1, 4);
        level.Set(EntityID.ID.ItemApple, 5, 1, 6);
        level.Set(EntityID.ID.ItemApple, 7, 1, 5);
    }

    private static void FillLayer(Level level, EntityID.ID id, int layer)
    {
        int xWidth = level.GetXWidth();
        int zWidth = level.GetZWidth();

        for (int x = 0; x < xWidth; x++)
        {
            for (int z = 0; z < zWidth; z++)
            {
                level.Set(id, x, layer, z);
            }
        }
    }

    private static void FillLayerBorders(Level level, EntityID.ID id, int layer)
    {
        int xWidth = level.GetXWidth();
        int zWidth = level.GetZWidth();

        for (int x = 0; x < xWidth; x++)
        {
            for (int z = 0; z < zWidth; z++)
            {
                if (x == 0 || z == 0 || x == xWidth - 1 || z == zWidth - 1)
                {
                    level.Set(id, x, layer, z);
                }
            }
        }
    }
}