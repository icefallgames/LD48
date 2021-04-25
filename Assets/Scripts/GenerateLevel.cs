using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public static class GenerateLevel
{
    public static GeneratedLevel Generate(ref Constants constants, Level level, ref GameObject playerObject, Transform fixedParent, LevelStateFrame initialFrame)
    {
        List<ObjectWithPosition> objectsWithPosition = new List<ObjectWithPosition>();

        // Make the ceiling (fixed to camera)
        Vector3 ceilingPiecePos = constants.TopLeft;
        Vector3 floorPiecePos = constants.TopLeft;
        ceilingPiecePos.z = -1f; // In front of stuff
        floorPiecePos.z = -1f;
        floorPiecePos.y -= (constants.Height - 1) * constants.CelHeight;
        for (int i = 0; i < constants.Width; i++)
        {
            GameObject.Instantiate(level.LevelConstants.DeathCeiling, ceilingPiecePos, Quaternion.identity, fixedParent);

            GameObject.Instantiate(level.LevelConstants.DeathFloor, floorPiecePos, Quaternion.identity, fixedParent);

            ceilingPiecePos.x += constants.CelWidth;
            floorPiecePos.x += constants.CelWidth;
        }

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
        int celCount = jsonLevel.layers[0].data.Length;
        for (int i = 0; i < celCount; i++)
        {
            int tile = jsonLevel.layers[0].data[i];
            switch (tile)
            {
                case Constants.WallPiece:
                    GameObject.Instantiate(level.LevelConstants.Wall, celPosition, Quaternion.identity, constants.Parent);
                    break;

                case Constants.SupportPiece:
                    GameObject.Instantiate(level.LevelConstants.WallSupport, celPosition, Quaternion.identity, constants.Parent);
                    break;

                case Constants.Exit:
                    GameObject.Instantiate(level.LevelConstants.Exit, celPosition, Quaternion.identity, constants.Parent);
                    break;

            }

            int maybeObject = jsonLevel.layers[1].data[i];
            GameObject createdObject = null;
            switch (maybeObject)
            {
                case Constants.PlayerPiece:
                    {
                        if (!playerObject)
                        {
                            playerObject = GameObject.Instantiate(level.LevelConstants.Player, celPosition, Quaternion.identity, constants.Parent);
                        }
                        else
                        {
                            playerObject.transform.position = celPosition;
                        }
                        createdObject = playerObject;
                    }
                    break;

                case Constants.DraggableSupport:
                    {
                        createdObject = GameObject.Instantiate(level.LevelConstants.DraggableSupport, celPosition, Quaternion.identity, constants.Parent);
                    }
                    break;
            }
            if (createdObject)
            {
                ObjectWithPosition pc = createdObject.GetComponent<ObjectWithPosition>();
                objectsWithPosition.Add(pc);
                pc.X = column;
                pc.Y = row;
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

        initialFrame.Objects = new ObjectLevelState[objectsWithPosition.Count];
        for (int i = 0; i < objectsWithPosition.Count; i++)
        {
            initialFrame.Objects[i].Object = objectsWithPosition[i];
        }

        return generatedLevel;
    }
}
