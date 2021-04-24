using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class Game : MonoBehaviour
{

    public MechanicParameters MechanicParameters;

    public Camera Camera;
    public Transform LevelParent;

    public Level[] Levels;

    private Constants constants;
    private PixelPerfectCamera pixelPerfectCamera;

    private Vector3 cameraRootPosition;

    // Start is called before the first frame update
    void Start()
    {
        cameraRootPosition = this.transform.position;

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




        /// TEST!
        /// 
        generatedLevel = GenerateLevel.Generate(ref constants, Levels[0], ref playerObject);
    }

    private GeneratedLevel generatedLevel;
    private GameObject playerObject;



    // Update is called once per frame
    void Update()
    {
        if (!isDoingMove)
        {
            PlayerController pc = playerObject.GetComponent<PlayerController>();
            if (pc.MovePlayer(generatedLevel, ref constants))
            {
                StartCoroutine(DoAMove(pc));
            }
        }
    }

    private bool isDoingMove = false;
    IEnumerator DoAMove(PlayerController pc)
    {
        isDoingMove = true;

        // First lerp the player
        {
            Vector3 playerStartPos = pc.transform.position;
            Vector3 playerEndPos = constants.GetObjectPosition(pc.transform, pc.X, pc.Y);
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
            // Figure out how far the player can move
            Vector3 playerStartPos = pc.transform.position;
            bool playerFell = false;
            while (true)
            {
                int x = pc.X;
                int y = pc.Y + 1;
                if (generatedLevel.CanMove(x, y))
                {
                    playerFell = true;
                    pc.Y = y;
                    //constants.SyncObjectPosition(pc.transform, pc.X, pc.Y);
                }
                else
                {
                    break;
                }
            }
            Vector3 playerEndPos = constants.GetObjectPosition(pc.transform, pc.X, pc.Y);

            // Now move the camera (this might kill the player)
            Vector3 cameraStartPos = Camera.transform.position;
            Vector3 cameraEndPos = cameraStartPos;
            cameraEndPos.y -= constants.CelHeight;
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
                    MechanicParameters.PlayerFallCurve.Evaluate(time / MechanicParameters.LevelFallTime)
                    );

                yield return null;
            }
        }

        isDoingMove = false;
    }
}
