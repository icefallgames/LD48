using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratedLevel
{
    public Level Level;
    public int Width;
    public int Height;
    public char[] Pieces;

    public bool CanMove(int x, int y)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)
        {
            char piece = Pieces[y * Width + x];
            if (piece != Constants.WallPiece)
            {
                return true;
            }
        }
        return false;
    }
}
