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
            y += (Input.GetAxisRaw("Vertical") > 0) ? -1 : 1;
            triedMove = true;
        }

        if (triedMove)
        {
            if (MoveHelper.CanMove_AndMaybePush(pos, x - pos.X, y - pos.Y, generatedLevel, objects, moveWorker))
            {
                moveSucceeded = true;
                pos.X = x;
                pos.Y = y;
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
