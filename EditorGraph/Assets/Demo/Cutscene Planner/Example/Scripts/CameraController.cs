using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CameraController : MovementController
{
    public Animator animator { get { return GetComponent<Animator>(); } }
    // Use this for initialization
    void Start () {
        Init();
	}
	
	// Update is called once per frame
	void Update () {
        MovementControllerUpdate();
	}
}
