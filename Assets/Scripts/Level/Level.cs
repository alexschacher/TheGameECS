using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Level holds a 3-dimensional array of EntityIDs, as well as getters and setters for that data.

public class Level
{
    private EntityID.ID[,,] levelData;

    public Level(int xWidth, int height, int zWidth) { levelData = new EntityID.ID[xWidth, height, zWidth]; }

    public void Set(EntityID.ID id, int x, int y, int z) { levelData[x, y, z] = id; }

    public EntityID.ID Get(int x, int y, int z) { return levelData[x, y, z]; }

    public int GetXWidth() { return levelData.GetLength(0); }

    public int GetHeight() { return levelData.GetLength(1); }

    public int GetZWidth() { return levelData.GetLength(2); }
}