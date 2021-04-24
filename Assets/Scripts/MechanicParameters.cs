using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "Mechanic Parameters")]
public class MechanicParameters : ScriptableObject
{
    public float PlayerMoveTime = 0.2f;
    public AnimationCurve PlayerMoveCurve;
    public float LevelFallTime = 0.3f; // Should be same as player fall time?
    public AnimationCurve LevelFallCurve;
    public AnimationCurve PlayerFallCurve;
}
