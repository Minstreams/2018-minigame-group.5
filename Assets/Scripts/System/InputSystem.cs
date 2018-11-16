using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSystem.PresentSetting;

namespace GameSystem
{
    /// <summary>
    /// 封装输入的系统
    /// </summary>
    public class InputSystem : SubSystem<InputSystemSetting>
    {


        public static bool GetInput(InputCode code)
        {
            switch (code)
            {
                case InputCode.Walk:
                    return Input.GetKeyDown(Setting.slideKeyCode);
                case InputCode.BeforeSlide:
                    return Input.GetKeyDown(Setting.slideKeyCode);
                case InputCode.Slide:
                    return Input.GetKeyUp(Setting.slideKeyCode);
                case InputCode.Func:
                    return Input.GetKey(Setting.funcKeyCode);
                default:
                    return false;
            }
        }


    }

    /// <summary>
    /// enum of all possible input
    /// </summary>
    public enum InputCode
    {
        Walk,
        BeforeSlide,
        Slide,
        Func
    }
}
