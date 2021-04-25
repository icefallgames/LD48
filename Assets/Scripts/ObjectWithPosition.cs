using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum CollisionLayer
{
    None = 0,
    Camera = 0x1,
    World = 0x2,
}

public class ObjectWithPosition : MonoBehaviour
{
    public CollisionLayer CollisionLayer;
    public bool FixedMovement;

    [System.NonSerialized]
    public int X;
    [System.NonSerialized]
    public int Y;
    [System.NonSerialized]
    public int Data; // Could be used for whatever... in this case, cracking a rock?
}
