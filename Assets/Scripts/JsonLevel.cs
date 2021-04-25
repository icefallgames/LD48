using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class JsonLevelLayer
{
    public int[] data;
    public string name;
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

    public int[] GetLayerData(string layerName)
    {
        for (int i = 0; i < layers.Length; i++)
        {
            if (layers[i].name == layerName)
            {
                return layers[i].data;
            }
        }
        return null;
    }
}
