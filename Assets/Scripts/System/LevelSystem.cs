﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSystem.PresentSetting;
using Cinemachine;

namespace GameSystem
{
    /// <summary>
    /// 游戏流程控制系统
    /// </summary>
    public class LevelSystem : SubSystem<LevelSystemSetting>
    {
        //References
        public static List<Transform> observerPoints = new List<Transform>();
        public static CinemachineFreeLook virtualCamera;
        public static List<PenguinController> PlayerList { get { return NetworkSystem.playerList; } }
        public static PenguinController LocalPlayer { get { return PenguinController.localPenguin; } }
        public static List<Transform> startPoints = new List<Transform>();
        public static GameObject camPoint = null;
        public static Transform camFollowTarget = null;

        private static int startPointIndex = 0;
        public static Vector3 GetNextStartPoint()
        {
            startPointIndex++;
            if (startPointIndex >= startPoints.Count) startPointIndex = 0;
            return startPoints[startPointIndex].position + Vector3.up * 50;
        }


        private static int playerAliveNum = 0;
        //Rpc Functions
        public static void RpcDie(PenguinController penguin)
        {
            Debug.Log("RpcDie " + penguin + "[playerAliveNum:" + playerAliveNum + "]");
            playerAliveNum--;

            StartCoroutine(DieSlowly(penguin));
        }
        private static IEnumerator DieSlowly(PenguinController penguin)
        {
            Time.timeScale = Setting.dieTimeScale;
            yield return new WaitForSeconds(Setting.dieTime);
            Time.timeScale = 1;
            if (NetworkSystem.IsServer)
                penguin.StopAllCoroutines();
            if (penguin.isLocalPlayer)
            {
                //local Player, disable the controlling process, enter Obeserve Mode
                StartObserve();
            }
            penguin.gameObject.SetActive(false);

            if (NetworkSystem.IsServer && playerAliveNum <= 1) StartNewLevel();
        }

        public static void RpcReborn(PenguinController penguin, Vector3 position)
        {
            Debug.Log("RpcReborn " + penguin + "[playerAliveNum:" + playerAliveNum + "]");
            playerAliveNum++;
            if (penguin.isLocalPlayer) camFollowTarget = penguin.camPoint;
            //Reset the position
            penguin.transform.position = position;
        }

        //Drag View Control
        public static void StartDragView()
        {
            camPoint = new GameObject("CamPoint");
            virtualCamera = GameObject.Instantiate(Setting.camPrefab).GetComponent<Cinemachine.CinemachineFreeLook>();
            GameObject.DontDestroyOnLoad(camPoint);
            GameObject.DontDestroyOnLoad(virtualCamera.gameObject);
            virtualCamera.Follow = virtualCamera.LookAt = camPoint.transform;
            dragCoroutine = StartCoroutine(DragView());
        }
        public static void StopDragView()
        {
            StopCoroutine(dragCoroutine);
            dragCoroutine = null;
            GameObject.Destroy(virtualCamera.gameObject);
            GameObject.Destroy(camPoint);
        }
        private static LinkedListNode<Coroutine> dragCoroutine = null;
        private static IEnumerator DragView()
        {
            while (true)
            {
                yield return 0;
                float x = 0;
                float y = 0;
                if (Input.GetMouseButton(0))
                {
                    x = Input.GetAxis("Mouse X") * Setting.viewDragSensitivity;
                    y = Input.GetAxis("Mouse Y") * Setting.viewDragSensitivity;
                }
                virtualCamera.m_XAxis.m_InputAxisValue = Input.GetMouseButton(0) ? x : -Input.GetAxis("Horizontal") * Setting.viewTurnDragFactor;
                virtualCamera.m_YAxis.m_InputAxisValue = y;

                if (camFollowTarget != null)
                {
                    camPoint.transform.Translate(Mathf.Pow(Setting.camFollowRate, 1 / Time.deltaTime) * (camFollowTarget.position - camPoint.transform.position));
                }
            }
        }

        private static void StartObserve()   //观战状态
        {
            camFollowTarget = LevelSystem.observerPoints[0];
        }




        public static void StartNewLevel()
        {
            Debug.Log("StartNewLevel[playerNum:" + PlayerList.Count + "]");
            //TODO:加载场景
            playerAliveNum = 0;
            foreach (PenguinController p in PlayerList) p.RpcReborn(GetNextStartPoint());

        }

    }
}