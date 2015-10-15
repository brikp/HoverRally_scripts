using UnityEngine;
using System.Collections;

public class PickupGenerator : MonoBehaviour 
{

	public GameObject[] possiblePickups;
	public float respawnTime;
	
	private GameObject pickup;
	private bool respawning = false;
	private float nextTime;
	private LevelParameters levelParameters;

	void Awake()
	{
		GameObject levelParametersObject = GameObject.FindWithTag ("LevelParameters");
		if (levelParametersObject != null)
		{
			levelParameters = levelParametersObject.GetComponent <LevelParameters>();
		}
		if (levelParameters == null)
		{
			Debug.Log ("Cannot find 'LevelParameters' script");
		}
	}

	void Start(){
		int i = (int)Random.Range(0, possiblePickups.Length);
		spawnPickup(i);
		respawning = false;
	}

	void Update () 
	{
		if (!pickup && !respawning) 
		{
			nextTime = Time.time + respawnTime;
			respawning = true;
		} 
		if (respawning && Time.time > nextTime) 
		{
			int i = (int)Random.Range(0, possiblePickups.Length);
			spawnPickup(i);
			respawning = false;
		}
	}
	
	void OnDrawGizmos() 
	{
		
	}
	
	void spawnPickup(int pickupIndex) 
	{
		Vector3 pickupPosition = transform.position;
		if(levelParameters.getSinglePlayer()==true)
			pickup = (GameObject)Instantiate(possiblePickups[pickupIndex], pickupPosition, transform.rotation);
		else if (Network.isServer)
			pickup = (GameObject)Network.Instantiate(possiblePickups[pickupIndex], pickupPosition, transform.rotation,0);

	}
	
	void OnTriggerEnter(Collider trigger)
	{
		if (trigger.GetComponent<Collider>().tag == "Player" || trigger.GetComponent<Collider>().tag == "Enemy")
			GetComponent<AudioSource>().Play();
	}
	
}
