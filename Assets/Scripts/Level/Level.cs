public class Level // Level holds a 3-dimensional array of EntityIDs, as well as getters and setters for that data.
{
    public static Level _instance;
    private EntityID.ID[,,] levelData;

    public Level(int xWidth, int height, int zWidth)
    {
        levelData = new EntityID.ID[xWidth, height, zWidth];

        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Log.LogError("Level.cs: A Level was created while another _instance was already active.");
        }
    }

    public void Set(EntityID.ID id, int x, int y, int z) { levelData[x, y, z] = id; }

    public EntityID.ID Get(int x, int y, int z) { return levelData[x, y, z]; }
    public EntityID.ID GetIfInRangeElseReturnInstead(int x, int y, int z, EntityID.ID returnInstead)
    {
        if (x >= 0 && x < GetXWidth() &&
            y >= 0 && y < GetHeight() &&
            z >= 0 && z < GetZWidth())
        {
            return levelData[x, y, z];
        }
        else
        {
            return returnInstead;
        }
    }

    public int GetXWidth() { return levelData.GetLength(0); }
    public int GetHeight() { return levelData.GetLength(1); }
    public int GetZWidth() { return levelData.GetLength(2); }
}