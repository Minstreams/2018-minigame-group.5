using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RidRecorder : MonoBehaviour {

    [System.Serializable]
    public class RecordUnit
    {
        public Rigidbody rigidbody;
        public Vector3 position;
        public Quaternion rotation;
    }
    public RecordUnit[] units;

    [ContextMenu("Record")]
    public void Record()
    {
        foreach (RecordUnit ru in units)
        {
            ru.position = ru.rigidbody.position;
            ru.rotation = ru.rigidbody.rotation;
        }
    }

    [ContextMenu("Read")]
    public void Read()
    {
        foreach (RecordUnit ru in units)
        {
            ru.rigidbody.position = ru.position;
            ru.rigidbody.rotation = ru.rotation;
        }
    }
}
