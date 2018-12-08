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
        public static float horizontal { private get; set; }
        public static float vertical { private get; set; }
        public static bool slide { private get; set; }
        public static bool brake { private get; set; }
        public static bool func { private get; set; }
        public static bool drop { private get; set; }


        public static bool GetInput(InputCode code)
        {
            switch (code)
            {
                case InputCode.Brake:
                    return brake;
                //return Input.GetKey(Setting.brakeKeyCode);
                case InputCode.Slide:
                    return slide;
                //return Input.GetKey(Setting.slideKeyCode);
                case InputCode.Func:
                    return func;
                case InputCode.Drop:
                    if (drop)
                    {
                        drop = false;
                        return true;
                    }
                    return false;
                default:
                    return false;
            }
        }

        public static float GetHorizontalAxis()
        {
            return horizontal;
        }

        public static float GetVeticalAxis()
        {
            return vertical;
        }
    }

    /// <summary>
    /// enum of all possible input
    /// </summary>
    public enum InputCode
    {
        Brake,
        Slide,
        Func,
        Drop
    }
}
