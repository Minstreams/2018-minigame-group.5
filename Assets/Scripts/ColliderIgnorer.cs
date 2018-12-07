using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ColliderIgnorer : MonoBehaviour
{
    private Collider cc;

    void Start()
    {
        cc = GetComponent<Collider>();
        Bounds bounds = cc.bounds;
        Collider[] colliders =
        Physics.OverlapBox(bounds.center, bounds.extents + Vector3.one * 0.01f);
        foreach (Collider c in colliders)
        {
            if (c != cc) Physics.IgnoreCollision(c, cc);
        }
    }
}
