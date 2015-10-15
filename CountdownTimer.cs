using UnityEngine;
using System.Collections;

public class CountdownTimer : MonoBehaviour {

	public Texture redLight;
	public Texture yellowLight;
	public Texture greenLight;
	private float startTime;
	private GameController gameController;

	void Awake()
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

	// Use this for initialization
	void Start () {
		gameObject.AddComponent<GUITexture>();
		transform.localScale = new Vector3(0.07f, 0.15f, 0.0f);
		GetComponent<GUITexture>().texture = redLight;
		startTime = 0.0f;

	}
	
	// Update is called once per frame
	// Tu bedzie trzeba jeszcze dodac audio
	void Update () {
		if(gameController.getAllNetworkReady()==true)
		{
			startTime += Time.deltaTime;
			
			if (GetComponent<GUITexture>() && startTime >= 4.0f)
			{
				Destroy(gameObject);
			}
			else if (startTime >= 3.0f)
			{
				GetComponent<GUITexture>().texture = greenLight;
			}
			else if (startTime >= 1.2f)
			{
				GetComponent<GUITexture>().texture = yellowLight;
			}

				
		}

	}
}
