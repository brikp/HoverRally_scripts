using UnityEngine;
using System.Collections;

public class Detonate : MonoBehaviour {

	public GameObject explosion;

	// Use this for initialization
	void Start () {
		Instantiate(explosion, transform.position, transform.rotation);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
