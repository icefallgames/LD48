using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MoveHelper
{
    public static bool CanMove_AndMaybePush(ObjectWithPosition obj, int dx, int dy, GeneratedLevel level, List<ObjectWithPosition> objects, bool[] hasMovedWorker)
    {
        int xDest = obj.X + dx;
        int yDest = obj.Y + dy;
        if (!level.IsUnblocked(xDest, yDest))
        {
            return false;
        }

        // If it's not blocked by physical geometry, maybe it's blocked by an object
        for (int i = 0; i < objects.Count; i++)
        {
            if (!hasMovedWorker[i] && (objects[i] != obj)) // Although we shouldn't be too worried about ourselves?
            {
                if ((objects[i].X == xDest) && (objects[i].Y == yDest)) // It's in our way
                {
                    if (DoObjectsCollide(obj, objects[i]))
                    {
                        // It's blocking us. Can we move it?
                        if (IsPushable(objects[i]) && CanMove_AndMaybePush(objects[i], dx, dy, level, objects, hasMovedWorker))
                        {
                            // Ok then. Move it.
                            objects[i].X += dx;
                            objects[i].Y += dy;
                            hasMovedWorker[i] = true; // Mark it moved.
                        }
                        else
                        {
                            // Nope. So it's blocking us.
                            return false;
                        }
                    }
                }
            }
        }

        return true;
    }

    public static bool IsPushable(ObjectWithPosition obj)
    {
        return obj.GetComponent<Draggable>();
    }

    public static bool DoObjectsCollide(ObjectWithPosition obj1, ObjectWithPosition obj2)
    {
        return (obj1.CollisionLayer & obj2.CollisionLayer) != CollisionLayer.None;
    }
}
