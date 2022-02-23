using UnityEngine;

[CreateAssetMenu(fileName = "Location", menuName = "ScriptableObjects/Location", order = 1)]
public class Location : ScriptableObject
{
    public int index;
    public Sprite background;

    [Header("Directional Locations")]
    public Location upLocation;
    public Location rightLocation;
    public Location downLocation;
    public Location leftLocation;

    [System.Serializable]
    public struct ItemSpawnLoc
    {
        public Item item;
        public Vector2 spawnLoc;
    }
    [Header("Items")]
    public ItemSpawnLoc[] itemsToSpawn;

    [Header("Quips")]
    public float quipChance = 0.5f;
    public string[] allQuips;
}
