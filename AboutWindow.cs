using UnityEngine;
using System.Collections;

public class AboutWindow : MonoBehaviour {

	private bool aboutActive = false;
	private string buttonName = "Return to main menu (Esc)";
	
	public void setButtonName(string buttonName)
	{
		this.buttonName = buttonName;
	}
	
	public bool getAboutActive()
	{
		return aboutActive;
	}
	
	public void setAboutActive(bool aboutActive)
	{
		this.aboutActive = aboutActive;
	}
	
	public void aboutWindow(int windowID) {
		GUI.color = Color.yellow;
		GUILayout.Label ("PJATK");
		GUILayout.Label ("Engineering Thesis");
		GUI.color = Color.cyan;
		GUILayout.Label ("Promoter:");
		GUILayout.Label ("Daniel Sadowski");
		GUI.color = Color.green;
		GUILayout.Label ("Programmers:");
		GUILayout.Label ("Janusz Kubik");
		GUILayout.Label ("Bartłomiej Karczmarczyk");
		GUILayout.Label ("");
		GUILayout.Label ("");
		GUILayout.Label ("");
		GUI.color = Color.white;
		if ((GUILayout.Button(buttonName))||(Input.GetKeyDown(KeyCode.Escape)))
			aboutActive = false;
		
	}
}
