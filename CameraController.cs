using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour 
{

	public GameObject player;
	private Vector3 offset;
	private Vector3 startPosition;
	
	void Awake()
	{
		GameObject start = GameObject.FindWithTag ("StartPositions");
		startPosition = start.transform.GetChild (0).position;
		transform.position = new Vector3(startPosition.x, startPosition.y+10, startPosition.z);
	}
	void Start () 
	{
		offset = new Vector3 (0.0f, transform.position.y, 0.0f);
	}


	// Update is called once per frame
	void LateUpdate () 
	{
		if (player != null) 
		{
			Vector3 v = new Vector3 (player.transform.position.x, startPosition.y+10, player.transform.position.z);
			transform.position = v + offset;
		}
	}
}
