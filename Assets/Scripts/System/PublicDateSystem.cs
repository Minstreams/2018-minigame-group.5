using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSystem.PresentSetting;

namespace GameSystem
{
    /// <summary>
    /// 一些公用数据
    /// </summary>
    public class PublicDateSystem : SubSystem<PublicDataSystemSetting>
    {
        public static PublicDataSystemSetting Data { get { return Setting; } }


        public static Sprite SetRandomHeadImage()
        {
            int index = Random.Range(0, Setting.headImages.Count);
            Setting.userData.imageIndex = index;
            Setting.userData.saved = false;
            return Setting.headImages[index];
        }

        public static Color SetRandomHeadColor()
        {
            Color c = Setting.headColors[Random.Range(0, Setting.headColors.Count)];
            Setting.userData.headColor = c;
            Setting.userData.saved = false;
            return c;
        }

        public static Color SetRandomBodyColor()
        {
            Color c = Setting.bodyColors[Random.Range(0, Setting.bodyColors.Count)];
            Setting.userData.bodyColor = c;
            Setting.userData.saved = false;
            return c;
        }
    }
}
