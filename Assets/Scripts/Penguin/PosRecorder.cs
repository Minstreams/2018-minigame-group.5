using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PosRecorder : MonoBehaviour
{
    [System.Serializable]
    public class RecordUnit
    {
        public Transform transform;
        public Vector3 localPosition;
        public Quaternion localRotation;
    }
    public RecordUnit[] units;

    [ContextMenu("Record")]
    public void Record()
    {
        foreach (RecordUnit ru in units)
        {
            ru.localPosition = ru.transform.localPosition;
            ru.localRotation = ru.transform.localRotation;
        }
    }

    [ContextMenu("Read")]
    public void Read()
    {
        foreach (RecordUnit ru in units)
        {
            ru.transform.localPosition = ru.localPosition;
            ru.transform.localRotation = ru.localRotation;
        }
    }
}
