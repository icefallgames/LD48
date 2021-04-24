using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class Game : MonoBehaviour
{
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
        center.y += ((float)constants.Height * 0.5f) * constants.CelHeight; // y is flipped
        constants.TopLeft = center;




        /// TEST!
        /// 
        GenerateLevel.Generate(ref constants, Levels[0]);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
