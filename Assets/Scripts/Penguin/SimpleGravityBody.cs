using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleGravityBody : MonoBehaviour
{
    private SimpleGravity simpleGravity;

    private void Awake()
    {
        simpleGravity = GetComponentInParent<SimpleGravity>();
    }

    private void OnCollisionStay(Collision collision)
    {
        if (/*!collision.collider.CompareTag("Player")&&*/ !collision.collider.CompareTag("Item"))
        {
            for (int i = 0; i < collision.contacts.Length; i++)
            {
                Debug.DrawLine(collision.contacts[i].point, collision.contacts[i].point - collision.contacts[i].normal * collision.contacts[i].separation, i == 0 ? Color.cyan : Color.black, 1, false);
            }
            if (simpleGravity.enabled) simpleGravity.transform.Translate(-collision.contacts[0].normal * collision.contacts[0].separation * 0.3f, Space.World);
        }
    }
}
