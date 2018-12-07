using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    namespace PresentSetting
    {
        [CreateAssetMenu(fileName = "LevelSystemSetting", menuName = "系统配置文件/Level System Setting")]
        public class LevelSystemSetting : ScriptableObject
        {
            [Header("相机Prefab")]
            public GameObject camPrefab;
            [Range(0, 1)]
            public float camFollowRate = 0.9f;

            public float dieTime = 1f;
            public float dieTimeScale = 0.2f;
            [Header("视野随着转身而旋转的比例")]
            public float viewTurnDragFactor = 0.1f;
            public float viewDragSensitivity = 1f;

            [Header("所有关卡，必须至少有两关")]
            public List<string> levels = new List<string>();
        }
    }
}
