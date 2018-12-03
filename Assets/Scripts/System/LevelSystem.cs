using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSystem.PresentSetting;

namespace GameSystem
{
    /// <summary>
    /// 游戏流程控制系统
    /// </summary>
    public class LevelSystem : SubSystem<LevelSystemSetting>
    {
        public static List<Transform> observerPoints = new List<Transform>();
    }
}