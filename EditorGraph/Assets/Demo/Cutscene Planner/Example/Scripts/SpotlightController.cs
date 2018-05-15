using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class SpotlightController : MovementController {

    public Light lightComponent { get { return GetComponent<Light>(); } }
    // Use this for initialization
    void Start () {
        Init();
	}
	
	// Update is called once per frame
	void Update () {
        MovementControllerUpdate();
	}
}
