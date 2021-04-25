using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpritePerData : MonoBehaviour
{
    ObjectWithPosition pos;
    new SpriteRenderer renderer;
    private void Awake()
    {
        pos = GetComponent<ObjectWithPosition>();
        renderer = GetComponent<SpriteRenderer>();
    }

    public Sprite[] Sprites;

    private void LateUpdate()
    {
        int index = pos.Data;
        index = Mathf.Clamp(index, 0, Sprites.Length - 1);
        renderer.sprite = Sprites[index];
    }
}
