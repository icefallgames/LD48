using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public enum MoveResult
{
    None,
    Death,
    Exit
}

public class Game : MonoBehaviour
{

    public MechanicParameters MechanicParameters;

    public Camera Camera;
    private LevelState levelState;
    public Transform LevelParent;

    public Level[] Levels;

    public GameObject DeathParticles;

    private Constants constants;
    private PixelPerfectCamera pixelPerfectCamera;

    private Vector3 cameraRootPosition;

    // Start is called before the first frame update
    void Start()
    {
        levelState = new LevelState();

        cameraRootPosition = Camera.transform.position;

        pixelPerfectCamera = Camera.GetComponent<PixelPerfectCamera>();
        constants.Width = pixelPerfectCamera.refResolutionX / 16;
        constants.Height = pixelPerfectCamera.refResolutionY / 16;

        float sizeVertical = Camera.orthographicSize * 2.0f;
        float sizeHorizontal = sizeVertical * Camera.aspect;

        // We fill the horizontal (that's an assumption)
        constants.CelWidth = sizeHorizontal / (float)constants.Width;
        constants.CelHeight = constants.CelWidth;

        constants.Parent = LevelParent;

        // Figure out the top left.
        Vector2 center = cameraRootPosition;
        center.x -= (((float)constants.Width * 0.5f) - 0.5f) * constants.CelWidth;
        center.y += (((float)constants.Height * 0.5f) - 0.5f) * constants.CelHeight; // y is flipped
        constants.TopLeft = center;

        ManifestLevel(currentLevel);
    }

    private int currentLevel;

    void ManifestLevel(int index)
    {
        Camera.transform.position = cameraRootPosition;
        levelState.Frames.Clear();
        levelState.AddFrame();
        levelState.Current.YCamera = 0;
        generatedLevel = GenerateLevel.Generate(ref constants, Levels[index], ref playerObject, Camera.transform, levelState.Current);
        levelState.SaveStates();
        lastMoveResult = MoveResult.None;
    }

    private GeneratedLevel generatedLevel;
    private GameObject playerObject;

    private void DestroyLevelObjects()
    {
        // The player
        GameObject.Destroy(playerObject);
        // The level objects
        foreach (Transform child in LevelParent)
        {
            GameObject.Destroy(child.gameObject);
        }
        // the ceilling
        foreach (Transform child in Camera.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        generatedLevel = null;

    }

    private bool isResetting = false;
    private IEnumerator Reset(bool shouldDie)
    {
        isResetting = true;

        if (shouldDie)
        {
            // Blood
            GameObject.Instantiate(DeathParticles, playerObject.transform.position + new Vector3(0, 0, -2), Quaternion.identity, LevelParent);

            yield return new WaitForSeconds(2.0f);
        }

        DestroyLevelObjects();

        yield return new WaitForSeconds(0.5f);
        isResetting = false;

        // Regenerate
        ManifestLevel(currentLevel);
    }

    private IEnumerator NextLevel()
    {
        isResetting = true;

        DestroyLevelObjects();

        yield return new WaitForSeconds(0.5f);
        isResetting = false;

        // Regenerate with new
        currentLevel++;
        ManifestLevel(currentLevel);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isResetting)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                StopCoroutine(lerpCoroutine);
                isLerpingMove = false;
                StartCoroutine(Reset(false));
            }
            else
            {
                PlayerController pc = playerObject.GetComponent<PlayerController>();

                if (pc.PressedMoveKey())
                {
                    if (isLerpingMove)
                    {
                        StopCoroutine(lerpCoroutine);
                        isLerpingMove = false;
                        // Position the objects at their destinations immediately, so the player doesn't need to wait for the lerp to finish.
                        PositionObjects(levelState.Current);
                        HandleMoveResultDelayed(lastMoveResult);
                    }
                }

                if (!isResetting) // Because we may have killed the player ^^
                {
                    if (pc.MovePlayer(generatedLevel, ref constants))
                    {
                        lastMoveResult = ProcessMoveResultsImmediately(pc);
                        lerpCoroutine = StartCoroutine(LerpToNewResults(pc, lastMoveResult));
                    }
                    else if (Input.GetKeyDown(KeyCode.X))
                    {
                        // Go back
                        levelState.Pop();
                        levelState.PushStates();
                        PositionObjects(levelState.Current);
                        lastMoveResult = MoveResult.None;
                    }
                }
            }
        }
    }

    private MoveResult lastMoveResult;
    private Coroutine lerpCoroutine;

    /*
    private bool CheckForDeath()
    {
        ObjectWithPosition objectWithPosition = playerObject.GetComponent<ObjectWithPosition>();
        return (objectWithPosition.Y <= levelState.Current.YCamera) ||
            (objectWithPosition.Y >= GetBottomDeathRow());
    }*/

    private int GetBottomDeathRow()
    {
        return (levelState.Current.YCamera + constants.Height - 1);
    }

    // Ensures objects visual positions are sync'd with their state
    private void PositionObjects(LevelStateFrame frame)
    {
        foreach (ObjectLevelState objectState in frame.Objects)
        {
            ObjectWithPosition pos = objectState.Object;
            pos.transform.position = constants.GetObjectPosition(pos.transform, pos.X, pos.Y);
        }
        Vector3 cameraEndPos = cameraRootPosition + new Vector3(0, -frame.YCamera * constants.CelHeight, 0);
        Camera.transform.position = cameraEndPos;
    }

    private MoveResult ProcessMoveResultsImmediately(PlayerController pc)
    {
        MoveResult result = MoveResult.None;

        // Add a new state frame
        levelState.AddFrame();
        levelState.Current.YCamera++;

        // Player falls, maybe
        ObjectWithPosition playerPos = pc.GetComponent<ObjectWithPosition>();
        while (true)
        {
            int x = playerPos.X;
            int y = playerPos.Y + 1;
            if (generatedLevel.CanMove(x, y))
            {
                playerPos.Y = y;
                if (GetBottomDeathRow() == playerPos.Y)
                {
                    result = MoveResult.Death;
                    break; // They're gonna die.
                }

                if (Constants.Exit == generatedLevel.GetPieceAt(playerPos.X, playerPos.Y))
                {
                    result = MoveResult.Exit;
                    break;
                }
            }
            else
            {
                break;
            }
        }

        if (playerPos.Y <= levelState.Current.YCamera)
        {
            result = MoveResult.Death;
        }

        levelState.SaveStates(); // Does that for the objects only, camera is already updated.

        return result;
    }

    private List<Vector3> startLerpWorker = new List<Vector3>();
    private List<Vector3> endLerpWorker = new List<Vector3>();

    private bool isLerpingMove = false;
    IEnumerator LerpToNewResults(PlayerController pc, MoveResult moveResult)
    {
        ObjectWithPosition playerPos = pc.GetComponent<ObjectWithPosition>();

        isLerpingMove = true;

        LevelStateFrame currentFrame = levelState.Current;

        // First lerp the player and other objects to the left or right.
        startLerpWorker.Clear();
        endLerpWorker.Clear();
        foreach (ObjectLevelState objectState in currentFrame.Objects)
        {
            startLerpWorker.Add(objectState.Object.transform.position);
            endLerpWorker.Add(constants.GetObjectPositionX(objectState.Object.transform, objectState.Object.X));
        }

        {
            float time = 0;
            while (time < MechanicParameters.PlayerMoveTime)
            {
                time += Time.deltaTime;
                time = Mathf.Min(MechanicParameters.PlayerMoveTime, time);

                for (int i = 0; i < startLerpWorker.Count; i++)
                {
                    ObjectWithPosition objectWithPos = currentFrame.Objects[i].Object;
                    objectWithPos.transform.position = Vector3.Lerp(startLerpWorker[i], endLerpWorker[i],
                        MechanicParameters.PlayerMoveCurve.Evaluate(time / MechanicParameters.PlayerMoveTime)
                        );
                }

                yield return null;
            }
        }



        // Now do the camera, and objects verticals
        startLerpWorker.Clear();
        endLerpWorker.Clear();
        foreach (ObjectLevelState objectState in currentFrame.Objects)
        {
            startLerpWorker.Add(objectState.Object.transform.position);
            endLerpWorker.Add(constants.GetObjectPositionY(objectState.Object.transform, objectState.Object.Y));
        }


        {
            // Now move the camera (this might kill the player)
            Vector3 cameraStartPos = Camera.transform.position;
            Vector3 cameraEndPos = cameraRootPosition + new Vector3(0, -currentFrame.YCamera * constants.CelHeight, 0);
            float time = 0;
            while (time < MechanicParameters.LevelFallTime)
            {
                time += Time.deltaTime;
                time = Mathf.Min(MechanicParameters.LevelFallTime, time);
                Camera.transform.position = Vector3.Lerp(cameraStartPos, cameraEndPos,
                    MechanicParameters.LevelFallCurve.Evaluate(time / MechanicParameters.LevelFallTime)
                    );

                for (int i = 0; i < startLerpWorker.Count; i++)
                {
                    ObjectWithPosition objectWithPos = currentFrame.Objects[i].Object;
                    objectWithPos.transform.position = Vector3.Lerp(startLerpWorker[i], endLerpWorker[i],
                        MechanicParameters.PlayerMoveCurve.Evaluate(time / MechanicParameters.PlayerMoveTime)
                        );
                }

                yield return null;
            }

        }

        HandleMoveResultDelayed(moveResult);

        isLerpingMove = false;
    }

    void HandleMoveResultDelayed(MoveResult moveResult)
    {
        switch (moveResult)
        {
            case MoveResult.Death:
                StartCoroutine(Reset(shouldDie: true));
                break;

            case MoveResult.Exit:
                StartCoroutine(NextLevel());
                break;
        }
    }
}
