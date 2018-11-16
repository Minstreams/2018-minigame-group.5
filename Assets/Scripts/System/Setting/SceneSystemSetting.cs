using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameSystem
{
    namespace PresentSetting
    {
        [CreateAssetMenu(fileName = "SceneSystemSetting", menuName = "系统配置文件/Scene System Setting")]
        public class SceneSystemSetting : ScriptableObject
        {
            public string loadingScene;
        }
    }
}
