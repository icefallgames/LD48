using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraggableSupport : MonoBehaviour
{
    const int MaxDegradations = 3;

    ObjectWithPosition pos;

    private void Awake()
    {
        pos = GetComponent<ObjectWithPosition>();
    }

    public bool ShouldBlockCameraMovementAndDegrade(GeneratedLevel level)
    {
        bool block = false;
        if (pos.Data < MaxDegradations)
        {
            int x = pos.X;
            int yBelow = pos.Y + 1; // below us

            if (level.GetWorldPieceAt(x, yBelow) == Constants.SupportPiece)
            {
                // Yup, we're blocked
                pos.Data++; // Degrade
                block = true;
            }
        }
        return block;
    }
}
