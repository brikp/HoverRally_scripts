using UnityEngine;
using System.Collections;

public class MainMenuGUI : MonoBehaviour {

	public GUIStyle customGuiStyle;
	private LevelParameters levelParameters;
	private string[] menuOptions = { "Single Player", "Multiplayer", "Help", "About", "Exit Game"};
	private int selectedIndex;
	private bool guiEnter;
	private Rect helpRect = new Rect(Screen.width / 2 + 250, Screen.height / 2 - 50, 200, 300);
	private Rect aboutRect = new Rect(Screen.width / 2 + 250, Screen.height / 2 - 50, 200, 300);
	private HelpWindow window;
	private LevelWindow levelWindow;
	private AboutWindow aboutWindow;

	void Start()
	{
		window = gameObject.AddComponent<HelpWindow>() as HelpWindow;
		levelWindow = gameObject.AddComponent<LevelWindow>() as LevelWindow;
		aboutWindow = gameObject.AddComponent<AboutWindow>() as AboutWindow;
		selectedIndex = 0;
		selectedIndex = 0;
		guiEnter = false;
		Time.timeScale = 1.0f;
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
	
	void OnGUI () {

		GUI.Label(new Rect(Screen.width / 2 + 190, Screen.height / 2 - 130, 200, 20), "Hover Rally", customGuiStyle);
		if((!window.getHelpActive())&&(!levelWindow.getLevelActive())&&(!aboutWindow.getAboutActive()))
		{
			if(Event.current.Equals (Event.KeyboardEvent("return"))){
				guiEnter = true;
			}
			GUI.BeginGroup (new Rect (Screen.width / 2 + 250, Screen.height / 2 - 50, 200, 270));
			GUI.Box (new Rect (0,0,200,270), "Main Menu");
			GUI.SetNextControlName (menuOptions[0]);
			if(selectedIndex == 0)
				GUI.color = Color.yellow;
			if((GUI.Button (new Rect (10,40,180,30), menuOptions[0]))||((guiEnter)&&(selectedIndex == 0)))
			{
				guiEnter = false;
				levelWindow.setLevelActive(true);
				selectedIndex = 0;
			}
			GUI.color = Color.white;
			GUI.SetNextControlName (menuOptions[1]);
			if(selectedIndex == 1)
				GUI.color = Color.yellow;
			if((GUI.Button (new Rect (10,2*40,180,30), menuOptions[1]))||((guiEnter)&&(selectedIndex == 1)))
			{
				Debug.Log ("Load multiplayer menu");
				levelParameters.setMultiPlayer(true);
				levelParameters.setSinglePlayer(false);
				Application.LoadLevel("MultiMenu");
				guiEnter = false;
			}
			GUI.color = Color.white;
			GUI.SetNextControlName (menuOptions[2]);
			if(selectedIndex == 2)
				GUI.color = Color.yellow;
			if((GUI.Button (new Rect (10,3*40,180,30), menuOptions[2]))||((guiEnter)&&(selectedIndex == 2)))
			{
				guiEnter = false;
				window.setHelpActive(true);
				selectedIndex = 0;
			}
			GUI.color = Color.white;
			GUI.SetNextControlName (menuOptions[3]);
			if(selectedIndex == 3)
				GUI.color = Color.yellow;
			if((GUI.Button (new Rect (10,4*40,180,30), menuOptions[3]))||((guiEnter)&&(selectedIndex == 3)))
			{
				guiEnter = false;
				aboutWindow.setAboutActive(true);
				selectedIndex = 0;
			}
			GUI.color = Color.white;
			GUI.SetNextControlName (menuOptions[4]);
			if(selectedIndex == 4)
				GUI.color = Color.yellow;
			if((GUI.Button (new Rect (10,5*40 + 20 ,180,30), menuOptions[4]))||((guiEnter)&&(selectedIndex == 4)))
			{
				Application.Quit();
			}
			GUI.color = Color.white;
			GUI.EndGroup ();
			
			GUI.FocusControl (menuOptions[selectedIndex]);
		}
		else if(window.getHelpActive())
		{
			helpRect = GUI.Window(0, helpRect, window.helpWindow, "Help");
		}
		else if(aboutWindow.getAboutActive())
		{
			aboutRect = GUI.Window(0, aboutRect, aboutWindow.aboutWindow, "About");
		}
	}

	private int menuSelection (string[] menuOptions, int selectedIndex, string direction) {
		if (direction == "up") {
			if (selectedIndex == 0) {
				selectedIndex = menuOptions.Length - 1;
			} else {
				selectedIndex -= 1;
			}
		}
		
		if (direction == "down") {
			if (selectedIndex == menuOptions.Length - 1) {
				selectedIndex = 0;
			} else {
				selectedIndex += 1;
			}
		}
		
		return selectedIndex;
	}

	void Update()
	{
		if((!window.getHelpActive())&&(!levelWindow.getLevelActive())&&(!aboutWindow.getAboutActive()))
		{
			if(Input.GetKeyDown(KeyCode.DownArrow))
			{
				selectedIndex = menuSelection (menuOptions,selectedIndex,"down");
			}
			if(Input.GetKeyDown(KeyCode.UpArrow))
			{
				selectedIndex = menuSelection (menuOptions,selectedIndex,"up");
			}
		}
	}

}
