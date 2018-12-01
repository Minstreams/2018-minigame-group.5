using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class DragView : MonoBehaviour
{
    CinemachineFreeLook virtualCamera;

    private void Awake()
    {
        virtualCamera = GetComponent<CinemachineFreeLook>();
    }

    private void Update()
    {
        float x = 0;
        float y = 0;
        if (Input.GetMouseButton(0))
        {
            x = Input.GetAxis("Mouse X");
            y = Input.GetAxis("Mouse Y");
        }
        virtualCamera.m_XAxis.m_InputAxisValue = x;
        virtualCamera.m_YAxis.m_InputAxisValue = y;
    }
}
