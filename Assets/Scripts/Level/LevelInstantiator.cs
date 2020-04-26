using Unity.Mathematics;

public static class LevelInstantiator
{
    private static float cellWidth = 1f;
    private static float cellHeight = 0.5f;

    public static void Instantiate(Level level)
    {
        int xWidth = level.GetXWidth();
        int height = level.GetHeight();
        int zWidth = level.GetZWidth();
        float3 pos = float3.zero;

        for (int x = 0; x < xWidth; x++)
        {
            for (int z = 0; z < zWidth; z++)
            {
                for (int y = 0; y < height; y++)
                {
                    pos.x = x * cellWidth;
                    pos.y = y * cellHeight;
                    pos.z = z * cellWidth;
                    EntityFactory.Instantiate(level.Get(x, y, z), pos);
                }
            }
        }
    }
}
