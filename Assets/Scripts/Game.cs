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
    public Tutorial Tutorial;
    public Level[] Levels;
    public GameObject Title;

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
        levelState.Clear();
        levelState.AddFrame();
        levelState.Current.YCamera = 0;
        generatedLevel = GenerateLevel.Generate(ref constants, Levels[index], ref playerObject, Camera.transform, levelState.Current, levelState.Objects);
        levelState.SaveStates();
        lastMoveResult = MoveResult.None;
    }

    private GeneratedLevel generatedLevel;
    private GameObject playerObject;

    private void DestroyLevelObjects()
    {
        Tutorial.Reset();

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
            GameObject deathParticles = GameObject.Instantiate(DeathParticles, playerObject.transform.position + new Vector3(0, 0, -2), Quaternion.identity, LevelParent);
            float time = 0;
            while (time < 2.0f)
            {
                yield return null;
                if (Input.GetKeyDown(KeyCode.X))
                {
                    // Bail - hacky, but ok for now
                    GameObject.Destroy(deathParticles);
                    isResetting = false;
                    GoBack();
                    yield break;
                }
                time += Time.deltaTime;
            }
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

    bool[] moveWorker;
    void ClearMoveWorker()
    {
        if (moveWorker == null || levelState.Objects.Count != moveWorker.Length)
        {
            moveWorker = new bool[levelState.Objects.Count];
        }
        for (int i = 0; i < moveWorker.Length; i++)
        {
            moveWorker[i] = false;
        }
    }

    static KeyCode[] codes = new KeyCode[] { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4 };

    // Update is called once per frame
    void Update()
    {
        if (!isResetting)
        {
#if UNITY_EDITOR
            int index = 0;
            foreach (KeyCode kc in codes)
            {
                if (Input.GetKeyDown(kc))
                {
                    currentLevel = index;
                    StartCoroutine(Reset(false));
                }
                index++;
            }
#endif

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
                    ClearMoveWorker();
                    if (pc.MovePlayer(generatedLevel, ref constants, levelState.Objects, moveWorker))
                    {
                        Title.SetActive(false);

                        //Debug.Log("player at: " + levelState.Current.YCamera);

                        lastMoveResult = ProcessMoveResultsImmediately(pc);
                        lerpCoroutine = StartCoroutine(LerpToNewResults(pc, lastMoveResult));
                    }
                    else if (Input.GetKeyDown(KeyCode.X))
                    {
                        GoBack();
                    }
                }
            }


            EvaluateTips();
        }
    }

    private void GoBack()
    {
        // Go back
        levelState.Pop();
        levelState.PushStates();
        PositionObjects(levelState.Current);
        lastMoveResult = MoveResult.None;
    }

    private void EvaluateTips()
    {
        if (currentLevel == 0)
        {
            Tutorial.ActivateTip(Tutorial.MoveTip);

            if (levelState.Current.YCamera > 21)
            {
                Tutorial.ActivateTip(Tutorial.JumpTip);
            }

            if (levelState.Current.YCamera > 42)
            {
                Tutorial.ActivateTip(Tutorial.GoBackTip);
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

    private void DescendCamera(LevelStateFrame frame)
    {
        // TODO: check if any are hitting supports.
        bool shouldBlock = false;
        foreach (ObjectLevelState objectState in frame.Objects)
        {
            DraggableSupport draggableSupport = objectState.Object.GetComponent<DraggableSupport>();
            if (draggableSupport)
            {
                if (draggableSupport.ShouldBlockCameraMovementAndDegrade(generatedLevel))
                {
                    shouldBlock = true; // Keep evaluating even if should block is already trye.
                }
            }
        }

        if (!shouldBlock)
        {
            foreach (ObjectLevelState objectState in frame.Objects)
            {
                if (objectState.Object.GetComponent<FixedToCamera>())
                {
                    objectState.Object.Y++; // We'll save states later.
                }
            }

            frame.YCamera++;
        }
    }

    // Handles falling.
    private MoveResult ProcessMoveResultsImmediately(PlayerController pc)
    {
        MoveResult result = MoveResult.None;

        // Add a new state frame
        levelState.AddFrame();

        // Can we descend the camera?
        DescendCamera(levelState.Current);

        // Player falls, maybe - unless they hit water?
        // This is a bit tricky, because we also need to push things down.
        ObjectWithPosition thePlayer = pc.GetComponent<ObjectWithPosition>();
        bool somethingMoved = true;
        bool bail = false;

        foreach (ObjectWithPosition pos in levelState.Objects)
        {
            // Store the transitionary states so we can do nice lerping.
            pos.XIntermediate = pos.X;
            pos.YIntermediate = pos.Y;
        }

        while (somethingMoved && !bail)
        {
            somethingMoved = false;
            ClearMoveWorker();

            for (int i = 0; i < levelState.Objects.Count; i++)
            {
                ObjectWithPosition pos = levelState.Objects[i];

                Gravity gravity = pos.GetComponent<Gravity>();
                int direction = gravity ? gravity.Direction : 0;
                bool isWater = generatedLevel.IsWater(pos.X, pos.Y);
                if ((!isWater) && direction != 0) // Things float in water.
                {
                    int x = pos.X;
                    int y = pos.Y + direction;
                    //if (generatedLevel.IsBlocked(x, y))
                    if (MoveHelper.CanMove_AndMaybePush(pos, 0, 1, generatedLevel, levelState.Objects, moveWorker))
                    {
                        pos.Y = y;
                        somethingMoved = true;
                    }
                }
                if (isWater)
                {
                    pos.Direction = 0; // Resets direction
                }

                if (thePlayer == pos)
                {
                    if ((GetBottomDeathRow() == pos.Y) || (pos.Y <= levelState.Current.YCamera))
                    {
                        result = MoveResult.Death;
                        bail = true;
                        break; // They're gonna die.
                    }

                    if (Constants.Exit == generatedLevel.GetWorldPieceAt(pos.X, pos.Y))
                    {
                        result = MoveResult.Exit;
                        bail = true;
                        break;
                    }
                }

            }
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
            //endLerpWorker.Add(constants.GetObjectPositionX(objectState.Object.transform, objectState.Object.X));
            endLerpWorker.Add(constants.GetObjectPosition(objectState.Object.transform, objectState.Object.XIntermediate, objectState.Object.YIntermediate));
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
