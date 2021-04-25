using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class Tutorial : MonoBehaviour
{
    public const int MoveTip = 0;
    public const int JumpTip = 1;
    public const int GoBackTip = 2;

    public GameObject[] Tips;
    private bool[] activatedTip;

    private void Awake()
    {
        activatedTip = new bool[Tips.Length];
    }

    public void Reset()
    {
        foreach (GameObject go in Tips)
        {
            go.SetActive(false);
        }
    }

    public void ActivateTip(int tipIndex)
    {
        if (!activatedTip[tipIndex])
        {
            activatedTip[tipIndex] = true;
            Tips[tipIndex].SetActive(true);
        }
    }

    void Update()
    {
        if (Tips[MoveTip].activeSelf)
        {
            if (Input.GetButtonDown("Horizontal"))
            {
                Tips[MoveTip].SetActive(false);
            }
        }
        if (Tips[JumpTip].activeSelf)
        {
            if ((Input.GetButtonDown("Jump") || (Input.GetButtonDown("Vertical") && (Input.GetAxisRaw("Vertical") > 0))))
            {
                Tips[JumpTip].SetActive(false);
            }
        }
        if (Tips[GoBackTip].activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                Tips[GoBackTip].SetActive(false);
            }
        }
    }
}
