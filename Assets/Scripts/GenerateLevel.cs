using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public static class GenerateLevel
{
    public static void Generate(ref Constants constants, Level level)
    {
        string data = level.Data.text;
        //string[] rows = data.Split(new[] { '\r', '\n' });
        string[] rows = Regex.Split(data, "\r\n|\r|\n");

        Vector2 celPosition = constants.TopLeft;
        foreach (string row in rows)
        {
            celPosition.x = constants.TopLeft.x;
            foreach (char c in row)
            {
                GameObject.Instantiate(level.LevelConstants.Wall, celPosition, Quaternion.identity, constants.Parent);
                celPosition.x += constants.CelWidth;
            }
            celPosition.y -= constants.CelHeight;
        }
    }
}
