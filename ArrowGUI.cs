using UnityEngine;
using System.Collections;

public class ArrowGUI : MonoBehaviour {

	public Texture arrowTexture;
	private float timer;

	void OnGUI()
	{
		if(timer<3.5f)
		{
			GUI.DrawTexture(new Rect(Screen.width / 2 -150, 10, 150, 150), arrowTexture, ScaleMode.ScaleToFit, true, 0);
			timer+=Time.deltaTime;
		}

	}

	void Start () {
	
		timer = 6.0f;
	}
	
	public void showArrow()
	{
		timer = 0.0f;
	}
	
}
