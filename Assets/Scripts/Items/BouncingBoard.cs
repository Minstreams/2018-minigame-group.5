using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSystem;

public class BouncingBoard : MonoBehaviour
{
    public bool debug = false;

    public Transform parent;
    public float lowSpring;
    public float highSpring;
    public float bounceValue;
    public float resetValue;

    public float extraForce;

    private bool bouncing = false;
    private SpringJoint sj;
    private void Awake()
    {
        sj = GetComponent<SpringJoint>();
        sj.spring = lowSpring;
    }

    private void FixedUpdate()
    {
        if (bouncing)
        {
            if (transform.localPosition.y > resetValue)
            {
                sj.spring = lowSpring;
                bouncing = false;
            }
        }
        else
        {
            if (transform.localPosition.y < bounceValue && attachedPenguin != null)
            {
                if (debug) Debug.Log("Bounce!");
                if (NetworkSystem.IsServer) attachedPenguin.ImpulseSpeed(parent.up * extraForce);
                sj.spring = highSpring;
                bouncing = true;
            }
        }
    }

    private PenguinController attachedPenguin = null;
    private int counter = 0;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            counter++;
            attachedPenguin = collision.collider.GetComponentInParent<PenguinController>();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            counter--;
            if (counter == 0)
                attachedPenguin = null;
        }
    }


}
