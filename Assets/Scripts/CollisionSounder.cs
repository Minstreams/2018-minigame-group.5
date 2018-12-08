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
    public float minVolecity = 4;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnCollisionEnter(Collision collision)
    {
        audioSource.pitch = Mathf.LerpUnclamped(pitchSmooth, pitchRough, collision.collider.material.dynamicFriction);
        float delta = collision.relativeVelocity.magnitude - minVolecity;
        if (delta > 0)
        {
            audioSource.volume = Mathf.Clamp01(minVolume + (1 - minVolume) * (delta) / (maxVolecity - minVolecity));
            audioSource.Play();
            if (debug) Debug.Log("Peng[v:" + collision.relativeVelocity.magnitude + "][Friction:" + collision.collider.material.dynamicFriction + "]");
        }
        else
        {
            if (debug) Debug.Log("Buuu[v:" + collision.relativeVelocity.magnitude + "][Friction:" + collision.collider.material.dynamicFriction + "]");
        }
    }
}
