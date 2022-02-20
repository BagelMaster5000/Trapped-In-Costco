using UnityEngine;

public class Location : ScriptableObject
{
    public Sprite background;

    [Header("Directional Locations")]
    public Location upLocation;
    public Location rightLocation;
    public Location downLocation;
    public Location leftLocation;

    public struct ItemSpawnLoc
    {
        public Item item;
        public Vector2 spawnLoc;
    }
    [Header("Items")]
    public ItemSpawnLoc[] itemsToSpawn;

    [Header("Quips")]
    public string[] allQuips;
}
