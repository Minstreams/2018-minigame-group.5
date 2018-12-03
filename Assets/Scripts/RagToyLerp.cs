using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagToyLerp : MonoBehaviour
{
    [Range(0, 1)]
    public float t = 0;

    public List<Rigidbody> rigidbodies;
    public List<Transform> ragHips;
    public List<Transform> animHips;

    private void FixedUpdate()
    {
        foreach (Rigidbody rigidbody in rigidbodies)
        {
            rigidbody.velocity *= t;
        }
        for (int i = 0; i < ragHips.Count; i++)
        {
            ragHips[i].Translate((animHips[i].position - ragHips[i].position) * (1 - t));
        }
    }
}
