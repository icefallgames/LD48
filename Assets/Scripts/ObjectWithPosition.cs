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
    public int EvaluationOrder;

    [System.NonSerialized]
    public int X;
    [System.NonSerialized]
    public int Y;
    [System.NonSerialized]
    public int Data; // Could be used for whatever... in this case, cracking a rock?
    [System.NonSerialized]
    public int Direction; // facing left (-1) or right (1). Or 0?

    // These are truly transitionary and don't need to be restored.
    [System.NonSerialized]
    public int XIntermediate;
    [System.NonSerialized]
    public int YIntermediate;

}
