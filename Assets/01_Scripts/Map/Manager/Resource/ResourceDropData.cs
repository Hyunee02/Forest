using UnityEngine;

[System.Serializable]
public class ResourceDropData
{
    public string itemName;

    [Range(0f, 100f)]
    public float dropWeight;
}