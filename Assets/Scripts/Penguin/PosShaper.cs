using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PosShaper : NetworkBehaviour
{

    [System.Serializable]
    public class RecordUnit
    {
        public Rigidbody rigidbody;
        public Vector3 localPosition;
        public Quaternion localRotation;
    }
    public RecordUnit[] units;
    public Rigidbody root;
    public Quaternion rootHeadUpRot;
    private Quaternion yRot;
    private Quaternion targetRot;
    [Range(0, 1)]
    public float rate;

    public Transform chest;
    public bool flip { get { return chest.forward.y > 0; } }

    public float flipTime;
    public float standTime;

    public float flipRate = 0.932f;
    public float crawlRate = 0.96f;
    public float standRate = 0.974f;

    public float drag = 1;
    [ContextMenu("Record")]
    public void Record()
    {
        Vector3 rEuler = root.rotation.eulerAngles;
        rEuler.y = 0;
        rootHeadUpRot = Quaternion.Euler(rEuler);

        Quaternion inRot = Quaternion.Inverse(root.rotation);
        foreach (RecordUnit ru in units)
        {
            ru.localPosition = inRot * (ru.rigidbody.position - root.position);
            ru.localRotation = inRot * ru.rigidbody.rotation;
        }
    }

    private void Awake()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();

        for (int i = 0; i < colliders.Length; i++)
        {
            for (int j = i + 1; j < colliders.Length; j++)
            {
                Physics.IgnoreCollision(colliders[i], colliders[j], true);
            }
        }
    }

    [ContextMenu("StartShape"), ClientRpc]
    public void RpcStartShape()
    {
        Vector3 rEuler = root.rotation.eulerAngles;
        rEuler.x = rEuler.z = 0;
        yRot = Quaternion.Euler(rEuler);
        targetRot = yRot * rootHeadUpRot;
        if (flip)
        {
            StartCoroutine(Flip());
        }
        else
        {
            StartCoroutine(Stand());
        }
        StartCoroutine(Shape());
    }

    [ContextMenu("EndShape"), ClientRpc]
    public void RpcEndShape()
    {
        foreach (RecordUnit ru in units)
        {
            ru.rigidbody.drag = 0;
        }
        StopAllCoroutines();
    }

    private IEnumerator Flip()
    {
        rate = flipRate;
        float timer = 0;
        while (timer < flipRate)
        {
            yield return 0;
            timer += Time.deltaTime;
            rate = Mathf.Lerp(flipRate, crawlRate, timer / standTime);
        }
        if (!flip)
        {
            StartCoroutine(Stand());
            yield break;
        }
        timer = flipRate;
        while (timer > 0)
        {
            yield return 0;
            timer -= Time.deltaTime;
            rate = Mathf.Lerp(flipRate, crawlRate, timer / standTime);
        }
        if (!flip)
        {
            StartCoroutine(Stand());
            yield break;
        }
        StartCoroutine(Flip());
    }

    private IEnumerator Stand()
    {
        foreach (RecordUnit ru in units)
        {
            ru.rigidbody.drag = drag;
        }
        rate = crawlRate;
        float timer = 0;
        while (timer < standTime)
        {
            yield return 0;
            timer += Time.deltaTime;
            rate = Mathf.Lerp(crawlRate, standRate, timer / standTime);
        }
    }

    private IEnumerator Shape()
    {
        while (true)
        {
            yield return new WaitForFixedUpdate();
            float powRate = Mathf.Pow(rate, 1 / Time.deltaTime);

            root.MoveRotation(Quaternion.Slerp(root.rotation, targetRot, powRate));
            foreach (RecordUnit ru in units)
            {
                //ru.rigidbody.transform.position = (Vector3.Lerp(ru.rigidbody.position, root.position + targetRot * ru.localPosition, powRate));
                ru.rigidbody.transform.rotation = (Quaternion.Lerp(ru.rigidbody.rotation, targetRot * ru.localRotation, powRate));
                //ru.rigidbody.velocity = root.velocity;
                //ru.rigidbody.angularVelocity *= 1 - powRate;
            }
            //root.angularVelocity = Vector3.zero;
        }
    }
}
