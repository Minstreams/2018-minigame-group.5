using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSystem;

public class SpeedUpBoard : MonoBehaviour
{
    public float speed;
    void Start()
    {
        if (!NetworkSystem.IsServer) Destroy(this);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PenguinController penguin = other.GetComponentInParent<PenguinController>();
            Vector3 v = penguin.transform.forward;
            v.y = 0;

            penguin.ConstantSpeed(v.normalized * speed);
        }
    }
}
