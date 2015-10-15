using UnityEngine;
using System.Collections;

//destroy every obcject that is out of track
public class DestroyByBoundary : MonoBehaviour 
{

	private GameController gameController;

	void Start ()
	{
		GameObject gameControllerObject = GameObject.FindWithTag ("GameController");
		if (gameControllerObject != null)
		{
			gameController = gameControllerObject.GetComponent <GameController>();
		}
		if (gameController == null)
		{
			Debug.Log ("Cannot find 'GameController' script");
		}
	}
	

	void OnTriggerExit(Collider other)
	{
		if ((other.gameObject.tag == "Player") || (other.gameObject.tag == "Enemy"))
		{
			ParentController parentController = other.gameObject.GetComponent <ParentController>();
			parentController.Damage(100.0f);
		}
		else if((other.gameObject.tag == "Wreck"))
		{
			Debug.Log ("Boundary don't destroy wreck");
		}
		else
		{
			Destroy (other.gameObject);
		}
	}
}
