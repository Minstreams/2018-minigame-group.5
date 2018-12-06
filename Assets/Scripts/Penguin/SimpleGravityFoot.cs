using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleGravityFoot : MonoBehaviour
{
    private SimpleGravity simpleGravity;
    public int attachedCollider = 0;

    private void Awake()
    {
        simpleGravity = GetComponentInParent<SimpleGravity>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.CompareTag("Player"))
        {
            if (attachedCollider == 0) simpleGravity.feetOnGround++;
            attachedCollider++;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!collision.collider.CompareTag("Player"))
        {
            attachedCollider--;
            if (attachedCollider == 0) simpleGravity.feetOnGround--;
        }
    }
}
