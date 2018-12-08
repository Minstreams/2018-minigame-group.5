using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CollisionSounder : MonoBehaviour
{
    public bool debug = false;
    AudioSource audioSource;
    public float pitchRough = 0.5f;
    public float pitchSmooth = 2f;
    public float minVolume = 0.5f;
    public float maxVolecity = 10;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnCollisionEnter(Collision collision)
    {
        audioSource.pitch = Mathf.LerpUnclamped(pitchSmooth, pitchRough, collision.collider.material.dynamicFriction);
        audioSource.volume = Mathf.Clamp01(minVolume + (1 - minVolume) * collision.relativeVelocity.magnitude / maxVolecity);
        if (debug) Debug.Log("Peng[v:" + collision.relativeVelocity.magnitude + "][Friction:" + collision.collider.material.dynamicFriction + "]");
        audioSource.Play();
    }
}
