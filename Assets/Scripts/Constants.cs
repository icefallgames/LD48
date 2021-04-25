using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Constants
{
    public int Width;       // How many columns are visible
    public int Height;
    public float CelWidth;  // In world units
    public float CelHeight; // In world units
    public Vector2 TopLeft;
    public Transform Parent;

    public void SyncObjectPosition(Transform tx, int x, int y)
    {
        Vector2 position = TopLeft;
        position.x += CelWidth * x;
        position.y -= CelHeight * y;
        tx.position = position;
    }
    public Vector3 GetObjectPosition(Transform tx, int x, int y)
    {
        Vector2 position = TopLeft;
        position.x += CelWidth * x;
        position.y -= CelHeight * y;
        return position;
    }

    public Vector3 GetObjectPositionX(Transform tx, int x)
    {
        Vector3 pos = tx.position;
        pos.x = TopLeft.x + CelWidth * x;
        return pos;
    }

    public Vector3 GetObjectPositionY(Transform tx, int y)
    {
        Vector3 pos = tx.position;
        pos.y = TopLeft.y - CelHeight * y;
        return pos;
    }


    public const int WallPiece = 1;
    public const int SupportPiece = 2;
    public const int PlayerPiece = 3;
    public const int Exit = 4;
    public const int DraggableSupport = 5;
}
