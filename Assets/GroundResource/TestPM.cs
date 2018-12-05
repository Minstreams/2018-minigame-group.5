using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TestPM : MonoBehaviour {

	private Rigidbody rig;

	private Vector3 oriPos;

	public Vector3 force = Vector3.zero;

	// Use this for initialization
	void Start () {
		rig = GetComponent <Rigidbody> ();
		oriPos = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void AddForce(){
		rig.AddForce (force);
	}

	public void Replace(){
		transform.position = oriPos;
	}
}


