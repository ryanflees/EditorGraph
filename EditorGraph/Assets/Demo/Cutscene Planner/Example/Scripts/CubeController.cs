using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Animator), (typeof(Rigidbody))) ]
public class CubeController : MonoBehaviour {

    public Animator animator { get { return GetComponent<Animator>(); } }
    
    [Space]
    public bool enableForce = false;
    public Vector3 force = new Vector3(0, 5, 0);
    public float interval = 2;

    private Rigidbody _rigidbody { get { return GetComponent<Rigidbody>(); } }
    private float _timer;
    private bool waitOneFrame;
	
	// Update is called once per frame
	void Update () {
        if (enableForce)
        {
            animator.enabled = false;
            if (waitOneFrame)
            {
                _rigidbody.velocity = Vector3.zero;
                waitOneFrame = false;
                return;
            }
            if (_timer <= 0)
            {
                _rigidbody.AddForce(force, ForceMode.Impulse);
                _rigidbody.AddTorque(RandTorque(), ForceMode.Impulse);
                _timer = interval;
            }
            else
                _timer -= Time.deltaTime;

        }
        else
            waitOneFrame = true;

    }

    private static Vector3 RandTorque()
    {
        float x = Random.Range(-5, 5);
        float y = Random.Range(-5, 5);
        float z = Random.Range(-5, 5);

        return new Vector3(x, y, z);
    }
}
