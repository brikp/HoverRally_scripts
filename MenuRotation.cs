using UnityEngine;
using System.Collections;

public class MenuRotation : MonoBehaviour {

	private float rotation;
	private float rotationRate;
	// Use this for initialization
	void Start () {
	
		rotation = 270.0f;
		rotationRate = 20.0f;
	}
	
	// Update is called once per frame
	void Update () {
	

		rotation += rotationRate * Time.deltaTime;
		GetComponent<Rigidbody>().rotation = (Quaternion.AngleAxis (rotation, Vector3.up));

	}
}
