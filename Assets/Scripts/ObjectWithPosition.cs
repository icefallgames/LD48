using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectWithPosition : MonoBehaviour
{
    [System.NonSerialized]
    public int X;
    [System.NonSerialized]
    public int Y;
    [System.NonSerialized]
    public int Data; // Could be used for whatever... in this case, cracking a rock?
}
