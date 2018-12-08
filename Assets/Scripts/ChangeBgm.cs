using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeBgm : MonoBehaviour
{
    public AudioClip clip;
    void Start()
    {
        AudioSource aa = GameObject.Find("TheMatrix").GetComponent<AudioSource>();
        aa.clip = clip;
        aa.Play();
    }
}
