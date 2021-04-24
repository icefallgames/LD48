using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This does go on player
public class PlayerController : MonoBehaviour
{
    // These can be out of bounds, I guess.
    [System.NonSerialized]
    public int X;
    [System.NonSerialized]
    public int Y;

    // True if the player actually moved
    public bool MovePlayer(GeneratedLevel generatedLevel, ref Constants constants)
    {
        int x = X;
        int y = Y;
        bool triedMove = false;
        bool moveSucceeded = false;
        if (Input.GetButtonDown("Horizontal"))
        {
            x += (Input.GetAxisRaw("Horizontal") < 0) ? -1 : 1;
            triedMove = true;
        }
        else if (Input.GetButton("Vertical"))
        {
            if (Input.GetAxisRaw("Vertical") > 0) // Basically it's a jump
            {
                triedMove = true;
                y += -1;
            }
        }

        if (triedMove)
        {
            if (generatedLevel.CanMove(x, y))
            {
                moveSucceeded = true;
                X = x;
                Y = y;
                // Don't sync him yet...
            }
            else
            {
                // Error beep
            }
        }
        return moveSucceeded;
    }
}
