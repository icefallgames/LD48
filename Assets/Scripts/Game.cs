using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class Game : MonoBehaviour
{

    public MechanicParameters MechanicParameters;

    public Camera Camera;
    private int yCamera;
    public Transform LevelParent;

    public Level[] Levels;

    public GameObject DeathParticles;

    private Constants constants;
    private PixelPerfectCamera pixelPerfectCamera;

    private Vector3 cameraRootPosition;

    // Start is called before the first frame update
    void Start()
    {
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
        yCamera = 0;
        generatedLevel = GenerateLevel.Generate(ref constants, Levels[index], ref playerObject, Camera.transform);
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

    // Update is called once per frame
    void Update()
    {
        if (!isDoingMove && !isResetting)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                StartCoroutine(Reset(false));
            }
            else
            {
                PlayerController pc = playerObject.GetComponent<PlayerController>();
                if (pc.MovePlayer(generatedLevel, ref constants))
                {
                    StartCoroutine(DoAMove(pc));
                }
            }
        }
    }

    private bool isDoingMove = false;
    IEnumerator DoAMove(PlayerController pc)
    {
        ObjectWithPosition playerPos = pc.GetComponent<ObjectWithPosition>();

        isDoingMove = true;

        // First lerp the player
        {
            Vector3 playerStartPos = pc.transform.position;
            Vector3 playerEndPos = constants.GetObjectPosition(pc.transform, playerPos.X, playerPos.Y);
            float time = 0;
            while (time < MechanicParameters.PlayerMoveTime)
            {
                time += Time.deltaTime;
                time = Mathf.Min(MechanicParameters.PlayerMoveTime, time);
                pc.transform.position = Vector3.Lerp(playerStartPos, playerEndPos,
                    MechanicParameters.PlayerMoveCurve.Evaluate(time / MechanicParameters.PlayerMoveTime)
                    );
                yield return null;
            }
        }

        {
            // Figure out how far the player can move - for now just one square
            Vector3 playerStartPos = pc.transform.position;
            bool playerFell = false;
            int x = playerPos.X;
            int y = playerPos.Y + 1;
            if (generatedLevel.CanMove(x, y))
            {
                playerFell = true;
                playerPos.Y = y;
                //constants.SyncObjectPosition(pc.transform, pc.X, pc.Y);
            }
            Vector3 playerEndPos = constants.GetObjectPosition(pc.transform, playerPos.X, playerPos.Y);

            // Now move the camera (this might kill the player)
            Vector3 cameraStartPos = Camera.transform.position;
            yCamera++; // Our integer thing
            Vector3 cameraEndPos = cameraRootPosition + new Vector3(0, - yCamera * constants.CelHeight, 0);
            float time = 0;
            while (time < MechanicParameters.LevelFallTime)
            {
                time += Time.deltaTime;
                time = Mathf.Min(MechanicParameters.LevelFallTime, time);
                Camera.transform.position = Vector3.Lerp(cameraStartPos, cameraEndPos,
                    MechanicParameters.LevelFallCurve.Evaluate(time / MechanicParameters.LevelFallTime)
                    );

                // Right now we're moving the player in the same time, but we might just want to do the first move in that time, and then have them fall further if needed?
                pc.transform.position = Vector3.Lerp(playerStartPos, playerEndPos,
                    MechanicParameters.LevelFallCurve.Evaluate(time / MechanicParameters.LevelFallTime)
                    );

                yield return null;
            }

            //Debug.LogFormat("Camera {0}    Player: {1}", yCamera, pc.Y);

            if (!playerFell)
            {
                // If they fell, there's currently no way they could die by ceiling
                if (playerPos.Y <= yCamera)
                {
                    StartCoroutine(Reset(shouldDie: true));
                }
            }
        }



        // Additional player falling
        {
            // Figure out how far the player can move
            Vector3 playerStartPos = pc.transform.position;
            while (true)
            {
                int x = playerPos.X;
                int y = playerPos.Y + 1;
                if (generatedLevel.CanMove(x, y))
                {
                    playerPos.Y = y;
                    //constants.SyncObjectPosition(pc.transform, pc.X, pc.Y);
                }
                else
                {
                    break;
                }
            }
            Vector3 playerEndPos = constants.GetObjectPosition(pc.transform, playerPos.X, playerPos.Y);

            float time = 0;
            while (time < MechanicParameters.LevelFallTime)
            {
                time += Time.deltaTime;

                pc.transform.position = Vector3.Lerp(playerStartPos, playerEndPos,
                    MechanicParameters.PlayerFallCurve.Evaluate(time / MechanicParameters.LevelFallTime)
                    );

                yield return null;
            }
        }


        isDoingMove = false;
    }
}
