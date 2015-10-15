using UnityEngine;
using System.Collections;

public class MusicPlayer : MonoBehaviour {

	public AudioClip mainMenu;
	public AudioClip track1;
	private bool track;
	private bool menu;
	private bool destroyMenu;

	void Awake()
	{
		DontDestroyOnLoad(this);
	}

	void Start () {
	
		track = false;
		menu = true;
		destroyMenu = false;
	}

	void Update () {
		if (((Application.loadedLevelName == "Track1")||((Application.loadedLevelName == "Track2"))||(Application.loadedLevelName == "Track3"))&&(!track))
		{
			GetComponent<AudioSource>().clip = track1;
			GetComponent<AudioSource>().Play ();
			GetComponent<AudioSource>().loop = true;
			track = true;
			menu = false;
			destroyMenu = true;

		}
		else if ((Application.loadedLevelName == "MainMenu")&&(destroyMenu))
		{
			Destroy(gameObject);
		}
		else if(((Application.loadedLevelName == "MultiMenu"))&&(!menu))
		{
			GetComponent<AudioSource>().clip = mainMenu;
			GetComponent<AudioSource>().Play ();
			GetComponent<AudioSource>().loop = true;
			track = false;
			menu = true;
		}
		if(Application.loadedLevelName == "MultiMenu")
		{
			destroyMenu = true;
		}
	}
}
