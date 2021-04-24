using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "LDJAM Level")]
public class Level : ScriptableObject
{
    public TextAsset Data;
    public LevelConstants LevelConstants;
}
