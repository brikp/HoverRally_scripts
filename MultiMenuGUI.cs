using UnityEngine;
using System.Collections;
using System;

public class MultiMenuGUI : MonoBehaviour {

	private string gameName;
	private bool refreshing;
	private HostData[] hostData;
	private int serverPort;
	private LevelParameters levelParameters;
	private NetworkLoader networkLoader;
	public Vector2 scrollPosition = Vector2.zero;

	void Awake()
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
			Debug.Log ("Cannot find 'networkLoader' script");
		}
	}


	void Start()
	{
		Time.timeScale = 1.0f;
		refreshing = false;
		gameName = "Delta_Team_PJ_EPG_2014";
	}
	
	void startServer()
	{
		MasterServer.ipAddress = levelParameters.serverIpAdress;
		MasterServer.port = levelParameters.serverPort;
		Network.natFacilitatorIP = levelParameters.serverIpAdress;
		Network.natFacilitatorPort = 50005;
		// Use NAT punchthrough if no public IP present
		Network.InitializeServer(3, 25002, !Network.HavePublicAddress());
		MasterServer.RegisterHost(gameName, "Death Race Game", "This is multiplayer test for our game");
		Application.LoadLevel("NewServer");

	}
	
	void refreshHostList()
	{
		MasterServer.ipAddress = levelParameters.serverIpAdress;
		MasterServer.port = levelParameters.serverPort;
		Network.natFacilitatorIP = levelParameters.serverIpAdress;
		Network.natFacilitatorPort = 50005;
		MasterServer.RequestHostList (gameName);
		refreshing = true;
	}
	
	void Update()
	{
		if (refreshing)
		{
			if(MasterServer.PollHostList().Length>0)
			{
				refreshing = false;
				Debug.Log (MasterServer.PollHostList ().Length);
				hostData = MasterServer.PollHostList ();
			}
		}
	}
		
	//mesages
	void OnMasterServerEvent(MasterServerEvent mse)
	{
		if (mse == MasterServerEvent.RegistrationSucceeded)
		{
			Debug.Log ("Regritered server");
		}
	}
	
	void OnGUI()
	{
		if(!Network.isClient && !Network.isServer)
		{

			GUI.BeginGroup (new Rect (Screen.width / 2-200, Screen.height / 2-100, 140, 240));
			GUI.Box (new Rect (0,0,140,240),"Multiplayer settings:");
			GUI.Label (new Rect (10,30,140,20), "Player name:");
			levelParameters.playerName = GUI.TextField(new Rect (10,50,120,20), levelParameters.playerName, 15);
			networkLoader.playerName = levelParameters.playerName;

			GUI.Label (new Rect (10,70,140,20), "Server IP:");
			levelParameters.serverIpAdress = GUI.TextField(new Rect (10,90,120,20), levelParameters.serverIpAdress, 15);

			GUI.Label (new Rect (10,110,140,20), "Server port:");
			Int32.TryParse(GUI.TextField(new Rect (10,130,120,20), levelParameters.serverPort.ToString(), 5), out levelParameters.serverPort);

			if(GUI.Button (new Rect (10,160,120,30), "Refresh Hosts"))
			{
				Debug.Log ("Refreshing");
				refreshHostList();

			}

			if(GUI.Button (new Rect (10,200,120,30), "Start server"))
			{
				Debug.Log ("Starting server");
				startServer();
			}

			GUI.EndGroup ();


			GUI.BeginGroup (new Rect (Screen.width / 2-50, Screen.height / 2-100, 280, 240));
			GUI.Box (new Rect (0,0,280,240),"Avaliable games:");
			scrollPosition = GUI.BeginScrollView(new Rect(10, 20, 260, 220),  scrollPosition, new Rect(0, 0, 240, 1000));

			if (hostData!=null)
			{
				for(int i=0; i<hostData.Length; i++)
				{
					if(GUI.Button (new Rect(10,10+(40*i),220,30), hostData[i].gameName)) 
					{
						Network.Connect(hostData[i]);
						Application.LoadLevel("NewServer");
					}
					
				}
				
			}

			GUI.EndScrollView();
			GUI.EndGroup ();

			if(GUI.Button (new Rect (Screen.width / 2-50, Screen.height / 2+150, 180,30), "Return to Main Menu"))
			{
				Destroy (levelParameters);
				Destroy (networkLoader);
				Application.LoadLevel("MainMenu");
			}

		}
		
	}
}
