using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratedLevel
{
    public Level Level;
    public int Width;
    public int Height;
    public int CameraHeight;
    public int[] Pieces;
    public int[] CameraPieces; // This is actually where we should but the stalactites!
    public int[] Water;

    public bool IsWater(int x, int y)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)
        {
            return Water[y * Width + x] != 0;
        }
        return false;
    }

    public int GetWorldPieceAt(int x, int y)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)
        {
            return Pieces[y * Width + x];
        }
        return 0;
    }

    public int GetCameraPieceAt(int x, int y, int yCamera)
    {
        y -= yCamera;
        if (x >= 0 && x < Width && y >= 0 && y < CameraHeight)
        {
            return CameraPieces[y * Width + x];
        }
        return 0;
    }

    // TODO: Expand this with collision flags, etc...
    public bool IsUnblocked(int x, int y)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)
        {
            int piece = Pieces[y * Width + x];
            if (piece != Constants.WallPiece)
            {
                return true;
            }
        }
        return false;
    }
}
