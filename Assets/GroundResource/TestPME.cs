using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TestPM))]
public class TestPME : Editor{
	public override void OnInspectorGUI(){
		DrawDefaultInspector ();

		TestPM testPM = (TestPM)target;
		if (GUILayout.Button ("Add Force")) {
			testPM.AddForce ();
		}
		if (GUILayout.Button ("Replace")) {
			testPM.Replace ();
		}
	}

}