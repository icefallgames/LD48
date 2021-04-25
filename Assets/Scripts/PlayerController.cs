using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This does go on player
public class PlayerController : MonoBehaviour
{
    private ObjectWithPosition pos;

    private void Awake()
    {
        pos = GetComponent<ObjectWithPosition>();
    }

    public bool PressedMoveKey()
    {
        return Input.GetButtonDown("Horizontal") || Input.GetButtonDown("Vertical");
    }

    // True if the player actually moved
    public bool MovePlayer(GeneratedLevel generatedLevel, ref Constants constants, List<ObjectWithPosition> objects, bool[] moveWorker)
    {
        int x = pos.X;
        int y = pos.Y;
        bool triedMove = false;
        bool moveSucceeded = false;
        if (Input.GetButtonDown("Horizontal"))
        {
            x += (Input.GetAxisRaw("Horizontal") < 0) ? -1 : 1;
            triedMove = true;
        }
        else if (Input.GetButtonDown("Vertical"))
        {
            int upDown = (Input.GetAxisRaw("Vertical") > 0) ? -1 : 1;
            y += upDown;
            if (upDown < 0) // Direction only when going up? Actually let's make water reset direction
            {
                x += pos.Direction; 
            }
            triedMove = true;
        }
        else if (Input.GetButtonDown("Jump"))
        {
            y -= 1;
            x += pos.Direction;
            triedMove = true;
        }

        if (triedMove)
        {
            if (MoveHelper.CanMove_AndMaybePush(pos, x - pos.X, y - pos.Y, generatedLevel, objects, moveWorker))
            {
                moveSucceeded = true;
            }
            else
            {
                if ((x != pos.X) && (y != pos.Y)) // We failed a diagonal jump. Try just straight up.
                {
                    x = pos.X;
                    if (MoveHelper.CanMove_AndMaybePush(pos, x - pos.X, y - pos.Y, generatedLevel, objects, moveWorker))
                    {
                        moveSucceeded = true;
                    }
                }
            }

            if (moveSucceeded)
            {
                pos.Direction = x - pos.X; // Set a left/right direction!
                pos.X = x;
                pos.Y = y;
            }
        }
        return moveSucceeded;
    }
}
