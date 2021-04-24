using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public static class GenerateLevel
{
    public static GeneratedLevel Generate(ref Constants constants, Level level, ref GameObject playerObject)
    {
        GeneratedLevel generatedLevel = new GeneratedLevel();
        generatedLevel.Level = level;

        string data = level.Data.text;
        //string[] rows = data.Split(new[] { '\r', '\n' });
        string[] rows = Regex.Split(data, "\r\n|\r|\n");

        Vector2 celPosition = constants.TopLeft;
        generatedLevel.Height = rows.Length;
        int rowNumber = 0;
        foreach (string row in rows)
        {
            if (generatedLevel.Width == 0)
            {
                generatedLevel.Width = row.Length;
                generatedLevel.Pieces = new char[generatedLevel.Width * generatedLevel.Height];
            }
            celPosition.x = constants.TopLeft.x;
            int colNumber = 0;
            foreach (char c in row)
            {
                generatedLevel.Pieces[rowNumber * generatedLevel.Width + colNumber] = c;

                switch (c)
                {
                    case Constants.WallPiece:
                        GameObject.Instantiate(level.LevelConstants.Wall, celPosition, Quaternion.identity, constants.Parent);
                        break;

                    case Constants.PlayerPiece:
                        if (!playerObject)
                        {
                            playerObject = GameObject.Instantiate(level.LevelConstants.Player, celPosition, Quaternion.identity, constants.Parent);
                        }
                        else
                        {
                            playerObject.transform.position = celPosition;
                        }
                        PlayerController pc = playerObject.GetComponent<PlayerController>();
                        pc.X = colNumber;
                        pc.Y = rowNumber;
                        break;
                }
                colNumber++;
                celPosition.x += constants.CelWidth;
            }
            rowNumber++;
            celPosition.y -= constants.CelHeight;
        }

        return generatedLevel;
    }
}
