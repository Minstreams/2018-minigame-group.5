using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shinging : MonoBehaviour
{
    public Vector2 scaleRange;
    public float speed;
    public float offset;
    void Update()
    {
        transform.localScale = Vector3.one * Mathf.Lerp(scaleRange.x, scaleRange.y, 0.5f * (Mathf.Sin(Time.time * speed + offset) + 1));
    }
}
