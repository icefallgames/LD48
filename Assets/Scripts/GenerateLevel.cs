﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public static class GenerateLevel
{
    public static GeneratedLevel Generate(ref Constants constants, Level level, ref GameObject playerObject)
    {
        GeneratedLevel generatedLevel = new GeneratedLevel();
        generatedLevel.Level = level;

        JsonLevel jsonLevel = JsonLevel.CreateFromJSON(level.Data.text);

        string data = level.Data.text;

        Vector2 celPosition = constants.TopLeft;
        generatedLevel.Height = jsonLevel.height;
        generatedLevel.Width = jsonLevel.width;
        generatedLevel.Pieces = jsonLevel.layers[0].data;
        int row = 0;
        int column = 0;
        celPosition.x = constants.TopLeft.x;
        foreach (int tile in jsonLevel.layers[0].data)
        {
            switch (tile)
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
                    pc.X = column;
                    pc.Y = row;
                    break;
            }

            column++;
            if (column >= generatedLevel.Width)
            {
                column = 0;
                celPosition.x = constants.TopLeft.x;
                row++;
                celPosition.y -= constants.CelHeight;
            }
            else
            {
                celPosition.x += constants.CelWidth;
            }
        }
        return generatedLevel;
    }
}
