using UnityEngine;

public class Startup : MonoBehaviour
{
    private void Start()
    {
        Resource.LoadResources();

        Level level = new Level(13, 4, 13);
        LevelGenerator.Generate(level);
        LevelInstantiator.Instantiate(level);
    }
}