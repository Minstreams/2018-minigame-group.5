using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 观战点
/// </summary>
public class ObserverPoint : MonoBehaviour {

    private void Start()
    {
        GameSystem.LevelSystem.observerPoints.Add(transform);
    }
    private void OnDestroy()
    {
        GameSystem.LevelSystem.observerPoints.Remove(transform);
    }
}
