using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GameSystem
{
    namespace PresentSetting
    {
        [CreateAssetMenu(fileName = "InputSystemSetting", menuName = "系统配置文件/Input System Setting")]
        public class InputSystemSetting : ScriptableObject
        {
            public KeyCode slideKeyCode;
            public KeyCode funcKeyCode;
        }
    }
}
