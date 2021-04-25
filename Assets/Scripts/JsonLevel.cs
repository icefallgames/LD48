using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class JsonLevelLayer
{
    public int[] data;
}

[System.Serializable]
public class JsonLevel
{
    public int width;
    public int height;
    // First layer is base tilemap
    // Second is fixed objects.
    public JsonLevelLayer[] layers;

    public static JsonLevel CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<JsonLevel>(jsonString);
    }
}
