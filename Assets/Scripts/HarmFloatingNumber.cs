using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarmFloatingNumber : MonoBehaviour
{
    public float height;
    public float rate;
    public float time;
    // Use this for initialization
    void Start()
    {
        StartCoroutine(floating());
    }

    IEnumerator floating()
    {
        TextMesh tm = GetComponent<TextMesh>();
        Vector3 target = transform.position + Vector3.up * height;
        Color c = tm.color;
        float timer = time;
        while (timer > 0)
        {
            yield return 0;
            timer -= Time.deltaTime;
            c.a = timer / time;
            tm.color = c;
            transform.position += rate * (target - transform.position);
        }
        Destroy(gameObject);
    }
}
