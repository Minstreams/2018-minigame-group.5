using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 可保存的文件，用于和TheMatrix的存档控制功能交互
/// </summary>
public abstract class SavableObject : ScriptableObject
{
    /// <summary>
    /// 是否已经保存
    /// </summary>
    [System.NonSerialized]
    public bool saved;
}
