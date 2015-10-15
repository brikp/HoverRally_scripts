using UnityEngine;
using System.Collections;

public class HelpWindow : MonoBehaviour {

	private bool helpActive = false;
	private string buttonName = "Return to main menu (Esc)";

	public void setButtonName(string buttonName)
	{
		this.buttonName = buttonName;
	}

	public bool getHelpActive()
	{
		return helpActive;
	}

	public void setHelpActive(bool helpActive)
	{
		this.helpActive = helpActive;
	}

	public void helpWindow(int windowID) {
		
		GUILayout.Label ("Up arrow - accelerate");
		GUILayout.Label ("Down arrow - brake");
		GUILayout.Label ("Left arrow - turn left");
		GUILayout.Label ("Right arrow - turn right");
		GUILayout.Label ("Space - handbrake");
		GUI.color = Color.red;
		GUILayout.Label ("Ctrl - fire");
		GUI.color = Color.cyan;
		GUILayout.Label ("Z - turbo");
		GUI.color = Color.yellow;
		GUILayout.Label ("X - plant mine");
		GUI.color = Color.white;
		GUILayout.Label ("Tab - statistics");
		GUILayout.Label ("");
		GUI.color = Color.white;
		if ((GUILayout.Button(buttonName))||(Input.GetKeyDown(KeyCode.Escape)))
			helpActive = false;
		
	}

}
