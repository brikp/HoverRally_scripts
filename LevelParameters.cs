using UnityEngine;
using System.Collections;

public class LevelParameters : MonoBehaviour {

	private bool singlePlayer;
	private bool multiPlayer;
	private string results;
	public string playerName;
	public string serverIpAdress;
	public int serverPort;

	void Awake()
	{
		DontDestroyOnLoad(this);
	}
	
	void Start ()
	{
		singlePlayer = false;
		multiPlayer = false;
		playerName = "Player";
		serverIpAdress = "127.0.0.1";
		serverPort = 23466;
	}
	
	public void setSinglePlayer(bool singlePlayer)
	{
		this.singlePlayer = singlePlayer;
	}

	public bool getSinglePlayer()
	{
		return this.singlePlayer;
	}

	public void setMultiPlayer(bool multiPlayer)
	{
		this.multiPlayer = multiPlayer;
	}
	
	public bool getMultiPlayer()
	{
		return this.multiPlayer;
	}

	public void setResults(string results)
	{
		this.results = results;
	}
	
	public string getResults()
	{
		return this.results;
	}

	public void setPlayerName(string playerName)
	{
		this.playerName = playerName;
	}
	
	public string getPlayerName()
	{
		return this.playerName;
	}

	public void destroy()
	{
		Destroy (this);
	}

}
