using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPoint : MonoBehaviour
{
    void Awake()
    {
        GameSystem.LevelSystem.startPoints.Add(transform);
    }

    void OnDestroy()
    {
        GameSystem.LevelSystem.startPoints.Remove(transform);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        UnityEditor.Handles.color = Color.green;
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, 2);
        UnityEditor.Handles.color = Color.white;
    }
#endif
}
