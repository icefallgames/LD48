using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: could be other states like if it's destroyed or not.
public struct ObjectLevelState
{
    public ObjectWithPosition Object;
    public int X;
    public int Y;
    public int Data;
}

public class LevelStateFrame
{
    public ObjectLevelState[] Objects;
    public int YCamera;

    public LevelStateFrame() { }

    public LevelStateFrame(LevelStateFrame orig)
    {
        YCamera = orig.YCamera;
        Objects = (ObjectLevelState[])orig.Objects.Clone();
    }
}



public class LevelState
{
    public LevelState()
    {
        Frames = new List<LevelStateFrame>();
    }

    public void Pop()
    {
        // Remove last one (unless only 1)
        if (Frames.Count > 0)
        {
            Frames.RemoveAt(Frames.Count - 1);
        }
    }

    public void AddFrame()
    {
        if (Frames.Count > 0)
        {
            // Clone and add
            Frames.Add(new LevelStateFrame(Frames[Frames.Count - 1]));
        }
        else
        {
            Frames.Add(new LevelStateFrame());
        }
    }

    public LevelStateFrame Current
    {
        get { return Frames[Frames.Count - 1]; }
    }

    public void SaveStates()
    {
        LevelStateFrame current = Current;
        for (int i = 0; i < current.Objects.Length; i++)
        {
            ObjectWithPosition pos = current.Objects[i].Object;
            current.Objects[i].X = pos.X;
            current.Objects[i].Y = pos.Y;
            current.Objects[i].Data = pos.Data;
        }
    }

    // for going back
    public void PushStates()
    {
        LevelStateFrame current = Current;
        for (int i = 0; i < current.Objects.Length; i++)
        {
            ObjectWithPosition pos = current.Objects[i].Object;
            pos.X = current.Objects[i].X;
            pos.Y = current.Objects[i].Y;
            pos.Data = current.Objects[i].Data;
        }
    }

    public List<LevelStateFrame> Frames;
}
