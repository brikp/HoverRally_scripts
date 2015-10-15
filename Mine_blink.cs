using UnityEngine;
using System.Collections;

public class Mine_blink : MonoBehaviour {

	public Material mine_on;
	public Material mine_off;

	private bool blink;
	private float nextBlink;
	public float blinkTime;
	public float blinkCooldown;

	private MeshRenderer mesh;

	// Use this for initialization
	void Start () {
		blink = false;
		nextBlink = Time.time + blinkCooldown;
		mesh = gameObject.GetComponent<MeshRenderer> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.time >= nextBlink) 
			blink = true;

		if (blink == true) {
			if (Time.time <= nextBlink + blinkTime) {
				mesh.material = mine_on;
			}
			else {
				blink = false;
				mesh.material = mine_off;
				nextBlink = Time.time + blinkCooldown;
			}
		}

	}
}
