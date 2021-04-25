using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSound : MonoBehaviour
{
    public AudioClip[] Clips;
    private AudioSource source;

    // Start is called before the first frame update
    void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    public void Play(bool forceNew = false)
    {
        if (forceNew || !source.isPlaying)
        {
            int index = Random.Range(0, Clips.Length);
            source.clip = Clips[index];
            source.Play();
        }
    }
}
