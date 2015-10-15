using UnityEngine;
using System.Collections;

public class MiniMapCamera : MonoBehaviour {

	private float aspect;
	private float wideAspect169 = Mathf.Round((16.0f/9.0f)*100f)/100f;
	private float wideAspect1610 = Mathf.Round((16.0f/10.0f)*100f)/100f;

	void Start () {
		aspect = Mathf.Round (((float)Screen.width / ((float)Screen.height)) * 100f) / 100f;
		if(aspect == wideAspect169)
		{
			//Debug.Log ("wideaspect true");
			GetComponent<Camera>().rect = new Rect (0.84f, 0.04f, 0.11f, 0.2f);
		}
		else if( aspect == wideAspect1610)
		{
			GetComponent<Camera>().rect = new Rect (0.84f, 0.04f, 0.125f, 0.2f);
		}
	}
}
