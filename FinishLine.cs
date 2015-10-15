using UnityEngine;
using System.Collections;

public class FinishLine : MonoBehaviour {

	public AudioClip finishMusic;
	public AudioClip countdown;
	private bool playedFinish;
	private bool playedCount;
	private GameController gameController;
	

	// Use this for initialization
	void Start () {
		playedFinish = false;
		playedCount = false;
		GameObject gameControllerObject = GameObject.FindWithTag ("GameController");
		if (gameControllerObject != null)
		{
			gameController = gameControllerObject.GetComponent <GameController>();
		}
	}
	
	// Update is called once per frame
	void Update () {
		if((gameController.getAllNetworkReady()==true)&&(!playedCount))
		{
			GetComponent<AudioSource>().clip = countdown;
			GetComponent<AudioSource>().Play ();
			playedCount = true;

		}
		if (gameController.getEndOfRace() && !playedFinish)
		{
			GetComponent<AudioSource>().clip = finishMusic;
			GetComponent<AudioSource>().loop = false;
			//audio.Play();
			playedFinish = true;
		}
	}
}
