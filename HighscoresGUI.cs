using UnityEngine;
using System.Collections;

public class HighscoresGUI : MonoBehaviour {

	public GUIStyle customGuiStyle;

	private LevelParameters levelParameters;
	private NetworkLoader networkLoader;

	void Start()
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

		GameObject networkLoaderObject = GameObject.FindWithTag ("NetworkLoader");
		if (networkLoaderObject != null)
		{
			networkLoader = networkLoaderObject.GetComponent <NetworkLoader>();
		}
		if (networkLoader == null)
		{
			Debug.Log ("Cannot find 'NetworkLoader' script");
		}

	}
	
	void OnGUI () {

		GUI.BeginGroup (new Rect (Screen.width / 2 - 150, Screen.height / 2 - 150, 400, 220));
		GUI.Box (new Rect (0,0,400,220), "High Scores:");
		GUI.Box (new Rect (10,40,380,120), levelParameters.getResults(), customGuiStyle);
		if(GUI.Button (new Rect (100,180,200,30), "Exit to Main Menu (Esc)"))
		{
			if(levelParameters.getMultiPlayer())
				networkLoader.disconnectFromGame();
			Destroy(levelParameters);
			Application.LoadLevel("MainMenu");
		}
		GUI.EndGroup ();
	}

	void Update(){

		if(Input.GetKeyDown (KeyCode.Escape))
		{
			if(levelParameters.getMultiPlayer())
				networkLoader.disconnectFromGame();
			Destroy(levelParameters);
			Application.LoadLevel("MainMenu");
		}

	}
}
