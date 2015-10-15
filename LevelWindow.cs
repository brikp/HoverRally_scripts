using UnityEngine;
using System.Collections;

public class LevelWindow : MonoBehaviour {

	private bool levelActive = false;
	private string[] supportedLevels  = { "Donkey meadow (Easy)", "Solid rock (Medium)" , "Cooking pot (Hard)" , "Return to main menu" };
	private int selectedIndex;
	private bool guiEnter;
	private LevelParameters levelParameters;
	private bool firstLoop;


	void Start()
	{
		selectedIndex = 0;
		guiEnter = false;
		firstLoop = true;
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

	void OnGUI ()
	{	


		if((levelActive))
		{
			if(!firstLoop)
			{
				if(Event.current.Equals (Event.KeyboardEvent("return"))){
					guiEnter = true;
				}
				GUI.BeginGroup(new Rect (Screen.width / 2 + 250, Screen.height / 2 - 50, 200, 230));
				GUI.Box (new Rect (0, 0, 200, 230),"Tracks:");
				GUI.SetNextControlName (supportedLevels[0]);
				if(selectedIndex == 0)
					GUI.color = Color.yellow;
				if((GUI.Button (new Rect (10,40,180,30),supportedLevels[0]))||((guiEnter)&&(selectedIndex == 0)))
				{
					levelParameters.setSinglePlayer(true);
					levelParameters.setMultiPlayer(false);
					Application.LoadLevel("Track2");
					guiEnter = false;
				}
				GUI.color = Color.white;
				GUI.SetNextControlName (supportedLevels[1]);
				if(selectedIndex == 1)
					GUI.color = Color.yellow;
				if((GUI.Button (new Rect (10,2*40,180,30),supportedLevels[1]))||((guiEnter)&&(selectedIndex == 1)))
				{
					levelParameters.setSinglePlayer(true);
					levelParameters.setMultiPlayer(false);
					Application.LoadLevel("Track3");
					guiEnter = false;
				}
				GUI.color = Color.white;
				GUI.SetNextControlName (supportedLevels[2]);
				if(selectedIndex == 2)
					GUI.color = Color.yellow;
				if((GUI.Button (new Rect (10,3*40,180,30),supportedLevels[2]))||((guiEnter)&&(selectedIndex == 2)))
				{
					levelParameters.setSinglePlayer(true);
					levelParameters.setMultiPlayer(false);
					Application.LoadLevel("Track1");
					guiEnter = false;
				}

				GUI.color = Color.white;
				if(selectedIndex == 3)
					GUI.color = Color.yellow;
				if((GUI.Button (new Rect (10,4*40 + 20,180,30),supportedLevels[3] + " (Esc)"))||((guiEnter)&&(selectedIndex == 3)))
				{
					firstLoop = true;
					levelActive = false;
					guiEnter = false;
					selectedIndex = 0;
				}
				GUI.color = Color.white;
				GUI.EndGroup();
				GUI.FocusControl (supportedLevels[selectedIndex]);
			}
			else
			{
				firstLoop = false;
			}
		}
	}
	
	void Update()
	{
		if (levelActive) {
			if(Input.GetKeyDown(KeyCode.DownArrow))
			{
				selectedIndex = menuSelection (supportedLevels,selectedIndex,"down");
			}
			if(Input.GetKeyDown(KeyCode.UpArrow))
			{
				selectedIndex = menuSelection (supportedLevels,selectedIndex,"up");
			}
		}

		if(Input.GetKeyDown (KeyCode.Escape))
		{
			firstLoop = true;
			levelActive = false;
			guiEnter = false;
			selectedIndex = 0;
		}
	}

	private int menuSelection (string[] supportedLevels, int selectedIndex, string direction) {
		if (direction == "up") {
			if (selectedIndex == 0) {
				selectedIndex = supportedLevels.Length - 1;
			} else {
				selectedIndex -= 1;
			}
		}
		
		if (direction == "down") {
			if (selectedIndex == supportedLevels.Length - 1) {
				selectedIndex = 0;
			} else {
				selectedIndex += 1;
			}
		}
		
		return selectedIndex;
	}

	public void setSelectedIndex(int selectedIndex)
	{
		this.selectedIndex = selectedIndex;
	}
	
	public bool getLevelActive()
	{
		return levelActive;
	}
	
	public void setLevelActive(bool levelActive)
	{
		this.levelActive = levelActive;
	}


}
