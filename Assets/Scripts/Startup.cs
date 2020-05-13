using UnityEngine;

public class Startup : MonoBehaviour
{
    private void Start()
    {
        Resource.LoadResources();

        Level level = new Level(20, 5, 20); // 40 x 40 is about the largest map size that will allow for 60fps with the current unoptimized renderer
        LevelGenerator.Generate(level);
        LevelInstantiator.Instantiate(level);
    }
} 