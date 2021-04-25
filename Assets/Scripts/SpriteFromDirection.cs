using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteFromDirection : MonoBehaviour
{
    ObjectWithPosition pos;
    new SpriteRenderer renderer;
    private void Awake()
    {
        pos = GetComponent<ObjectWithPosition>();
        renderer = GetComponent<SpriteRenderer>();
    }

    public Sprite Left;
    public Sprite Center;
    public Sprite Right;

    private void LateUpdate()
    {
        if (pos.Direction == 0)
        {
            renderer.sprite = Center;
        }
        else if (pos.Direction < 0)
        {
            renderer.sprite = Left;
        }
        else
        {
            renderer.sprite = Right;
        }
    }
}
