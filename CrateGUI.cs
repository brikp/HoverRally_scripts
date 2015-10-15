using UnityEngine;
using System.Collections;

public class CrateGUI : MonoBehaviour {

	public Texture crateTexture;
	private float timer;
	
	void OnGUI()
	{
		if(timer<3.5f)
		{
			GUI.DrawTexture(new Rect(Screen.width / 2, 45, 80, 80), crateTexture, ScaleMode.ScaleToFit, true, 0);
			timer+=Time.deltaTime;
		}
		
	}
	
	void Start () {
		
		timer = 6.0f;
	}
	
	public void showCrate()
	{
		timer = 0.0f;
	}
}
