using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SimpleGravity : NetworkBehaviour
{
    private SimpleGravityFoot[] feet;
    private SimpleGravityBody[] bodies;
    public int _feetOnGround = 0;
    public int feetOnGround
    {
        get { return _feetOnGround; }
        set
        {
            _feetOnGround = value;
            if (_feetOnGround == 0)
            {
                speed = 0;
            }
        }
    }

    private float speed = 0;

    private void Awake()
    {
        feet = GetComponentsInChildren<SimpleGravityFoot>();
        bodies = GetComponentsInChildren<SimpleGravityBody>();
    }

    //called when gameObject is diactivated and enactivated again
    public void Init()
    {
        Debug.Log("SimpleGravity Init");
        _feetOnGround = 0;
        foreach (SimpleGravityFoot f in feet)
        {
            f.attachedCollider = 0;
        }
    }

    private void Update()
    {
        if (!GameSystem.NetworkSystem.IsServer) return;
        if (feetOnGround <= 0)
        {
            speed += Physics.gravity.y * Time.deltaTime;
            transform.Translate(Vector3.up * speed * Time.deltaTime);
        }
    }


    private void OnEnable()
    {
        if (!GameSystem.NetworkSystem.IsServer) return;
        speed = 0;
        StopAllCoroutines();
        StartCoroutine(AdjustHeight());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public float adjustHeightUnit = 0.02f;
    private IEnumerator AdjustHeight()
    {
        Debug.Log("Height Adjusting");
        yield return 0;
        while (feetOnGround > 0)
        {
            transform.Translate(Vector3.up * adjustHeightUnit);
            yield return new WaitForFixedUpdate();
        }
        transform.Translate(Vector3.down * adjustHeightUnit);
    }
}
