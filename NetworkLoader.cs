using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


[RequireComponent (typeof (NetworkView))]
public class NetworkLoader : MonoBehaviour {

	string[] supportedNetworkLevels  = { "Track1", "Track2", "Track3" };
	string[] supportedNetworkLevelsNames = {"Cooking pot (Hard)", "Donkey meadow (Easy)", "Solid rock (Medium)" };
	string disconnectedLevel = "MultiMenu";
	private int lastLevelPrefix = 0;
	SortedList playersList = new SortedList();
	private bool isLoaded = false;
	public string playerName;
	private GameObject[] startPositions;
	private GameController gameController;
	public GameObject player;
	public GameObject enemyBot;
	public GUIStyle customGuiStyle;
	private CameraController cameraController;
	private GameObject mainCamera;
	private bool allReady;
	private bool allChecked;
	private GameObject thisPlayer;
	private Vector2 chatScrollPosition;
	private string chatString = "";
	private string stringToSend = "";
	private string[] botNames = {"Frank (bot)", "Rambo (bot)", "Leon (bot)"};
	private int maxPlayers = 4;
	private bool loading = false;


	void Awake ()
	{
		// Network level loading is done in a separate channel.
		DontDestroyOnLoad(this);
		GetComponent<NetworkView>().group = 1;
		//Application.LoadLevel(disconnectedLevel);
	}
	
	void Update()
	{
		allReady = true;
		allChecked = true;

		for (int i =0; i<playersList.Count; i++) {
			if ((bool)((ArrayList)playersList.GetByIndex (i))[0] == false)
				allReady = false;
			if ((bool)((ArrayList)playersList.GetByIndex (i))[1] == false)
				allChecked = false;
		}
		if ((allReady)&&(gameController!=null))
			gameController.setAllNetworkReady (allReady);
	}
	
	void OnGUI ()
	{
		if ((Network.peerType != NetworkPeerType.Disconnected) && (!isLoaded)) {
						GUI.Box (new Rect (Screen.width / 2 - 250, Screen.height / 2 + 50, 450, 130), "Chat:");
						GUILayout.BeginArea (new Rect (Screen.width / 2 - 250, Screen.height / 2 + 60, 450, 200));
						chatScrollPosition = GUILayout.BeginScrollView (chatScrollPosition, GUILayout.Width (450), GUILayout.Height (120));
						GUILayout.Label (chatString);
						GUILayout.EndScrollView ();
						if (Event.current.Equals (Event.KeyboardEvent ("return"))) {
								GetComponent<NetworkView>().RPC ("addToChat", RPCMode.All, playerName + ": " + stringToSend);
								stringToSend = "";
						}
						stringToSend = GUILayout.TextField (stringToSend);
						if ((GUILayout.Button ("Send message"))) {
								GetComponent<NetworkView>().RPC ("addToChat", RPCMode.All, playerName + ": " + stringToSend);
								stringToSend = "";
						}
						GUILayout.EndArea ();
						GUI.BeginGroup (new Rect (Screen.width / 2 - 250, Screen.height / 2 - 200, 270, 240));
						GUI.Box (new Rect (0, 0, 270, 240), "Players:");

						for (int i =0; i<playersList.Count; i++) {
								ArrayList list = ((ArrayList)playersList.GetByIndex (i));
								bool check = (bool)list [1];
								GUI.Label (new Rect (10, 40 + (i * 40), 140, 30), (i + 1) + ". " + playersList.GetKey (i).ToString ());
								if (playersList.GetKey (i).ToString () != playerName)
										GUI.enabled = false;
								list [1] = GUI.Toggle (new Rect (200, 40 + (i * 40), 100, 30), (bool)list [1], "Ready");
								if (check != (bool)list [1])
										GetComponent<NetworkView>().RPC ("checkPlayer", RPCMode.OthersBuffered, i);
								GUI.enabled = true;
								;
								if ((Network.isServer) && (playersList.GetKey (i).ToString () != playerName)) {
										if (GUI.Button (new Rect (150, 40 + (i * 40), 40, 20), "kick")) {
												Debug.Log ("Kick: " + playersList.GetKey (i).ToString ());	
												GetComponent<NetworkView>().RPC ("kickPlayer", RPCMode.All, playersList.GetKey (i).ToString ());
										}
								}
						}
						GUI.EndGroup ();

						GUI.BeginGroup (new Rect (Screen.width / 2 + 30, Screen.height / 2 - 200, 170, 240));
						GUI.Box (new Rect (0, 0, 170, 200), "Tracks:");
						int j = 1;
						foreach (string level in supportedNetworkLevels) {
								if ((!Network.isServer) || (!allChecked)) {
										GUI.enabled = false;
								}
								if (GUI.Button (new Rect (10, 40 * j, 150, 30), supportedNetworkLevelsNames [j - 1])) {
										Network.RemoveRPCsInGroup (0);
										Network.RemoveRPCsInGroup (1);
										GetComponent<NetworkView>().RPC ("LoadLevel", RPCMode.AllBuffered, level, lastLevelPrefix + 1);
								}
								j++;
						}
						if ((Network.isServer) && (playersList.Count < maxPlayers)) {
								GUI.enabled = true;
						} else
								GUI.enabled = false;
						if (GUI.Button (new Rect (0, 210, 170, 30), "Add bot")) {
								int botNameIndex = playersList.Count - 1;
								GetComponent<NetworkView>().RPC ("addBot", RPCMode.AllBuffered, botNames [botNameIndex]);
						}
						GUI.enabled = true;
						GUI.EndGroup ();

						if (GUI.Button (new Rect (Screen.width - 110, 10, 100, 50), "Disconnect")) {
								GetComponent<NetworkView>().RPC ("deletePlayer", RPCMode.All, playerName);
								Network.Disconnect ();
						}
				} else if (loading)
				{
					
					GUI.Label(new Rect(Screen.width / 2-100, Screen.height / 2, 200, 20), "Loading...", customGuiStyle);
				}

	}
	
	[RPC]
	public void kickPlayer(string playerName)
	{
		if(this.playerName==playerName)
		{
			GetComponent<NetworkView>().RPC ("deletePlayer", RPCMode.All, playerName);
			Network.Disconnect();
		}
		for(int i=0;i<botNames.Length;i++)
		{
			if(playerName==botNames[i])
			{
				deletePlayer (playerName);
			}
				
		}

	}
	
	[RPC]
	public void addToChat(string s)
	{
		chatString += "\n" + s;
	}

	[RPC]
	void LoadLevel (string level, int levelPrefix)
	{
		lastLevelPrefix = levelPrefix;
		
		// There is no reason to send any more data over the network on the default channel,
		// because we are about to load the level, thus all those objects will get deleted anyway
		Network.SetSendingEnabled(0, false);    
		
		// We need to stop receiving because first the level must be loaded first.
		// Once the level is loaded, rpc's and other state update attached to objects in the level are allowed to fire
		Network.isMessageQueueRunning = false;
		
		// All network views loaded from a level will get a prefix into their NetworkViewID.
		// This will prevent old updates from clients leaking into a newly created scene.
		Network.SetLevelPrefix(levelPrefix);
		Application.LoadLevel(level);
		isLoaded = true;

		StartCoroutine(WaitForSpawn());
	}

	[RPC]
	void addBot(string botName)
	{
		ArrayList readyList = new ArrayList();
		readyList.Add (true);
		readyList.Add (true);
		readyList.Add (null);
		readyList.Add (false);
		playersList.Add (botName,readyList);
	}

	// readyList: [0]isReady [1]isChecked [2]Network.player (null for bot) [3]playerIsDead
	[RPC]
	void updatePlayersList(string player, NetworkPlayer networkPlayer)
	{
		ArrayList readyList = new ArrayList();
		readyList.Add (false);
		readyList.Add (false);
		readyList.Add (networkPlayer);
		readyList.Add (false);
		playersList.Add (player,readyList);
	}
	
	public void deadPlayer(string name)
	{
		int i  = playersList.IndexOfKey (name);
		((ArrayList)playersList.GetByIndex (i))[3] = true;
	}

	[RPC]
	void deletePlayer(string player)
	{
		playersList.Remove (player);
	}

	[RPC]
	void readyPlayer(int index)
	{
		ArrayList list = ((ArrayList)playersList.GetByIndex (index));
		list [0] = true;
		playersList.SetByIndex (index, list);
	}
	
	[RPC]
	void checkPlayer(int index)
	{
		ArrayList list = ((ArrayList)playersList.GetByIndex (index));
		if ((bool)list [1] == true)
			list [1] = false;
		else
			list [1] = true;
		playersList.SetByIndex (index, list);
	}
	
	[RPC]
	void refreshPlayersList(NetworkPlayer networkPlayer)
	{
		for (int i =0; i<playersList.Count; i++) {
			Debug.Log ((((ArrayList)playersList.GetByIndex (i))[2].ToString()));
			if ((NetworkPlayer)((ArrayList)playersList.GetByIndex (i))[2] == networkPlayer)
			{
				playersList.RemoveAt(i);
				break;
			}
		}
	}

	[RPC]
	void decreaseAllPlayersCount()
	{
		gameController.decreaseAllPlayersCount();
	}


	[RPC]
	void setPlayerPrefabName(string racerName)
	{
		gameController.player.GetComponent<ParentController> ().racerName = racerName;
	}
	
	[RPC]
	void setEnemyPrefabName(string racerName)
	{
		gameController.enemy.GetComponent<ParentController> ().racerName = racerName;
	}

	[RPC]
	void setPlayerBodyMaterial(int materialNum)
	{
		gameController.player.transform.Find ("_Body").GetComponent<Renderer>().sharedMaterial = gameController.hoverMaterials [materialNum];
		gameController.player.transform.Find("Mini Map Marker").GetComponent<Renderer>().sharedMaterial = gameController.markerMaterials[materialNum];
		gameController.player.GetComponent<ParentController> ().bodyMaterial = gameController.hoverMaterials[materialNum];
	}

	[RPC]
	void setEnemyBodyMaterial(int materialNum)
	{
		gameController.enemy.transform.Find ("_Body").GetComponent<Renderer>().sharedMaterial = gameController.hoverMaterials [materialNum];
		gameController.enemy.transform.Find("Mini Map Marker").GetComponent<Renderer>().sharedMaterial = gameController.markerMaterials[materialNum];
		gameController.enemy.GetComponent<ParentController> ().bodyMaterial = gameController.hoverMaterials[materialNum];
	}

	void OnDisconnectedFromServer (NetworkDisconnection info)
	{
		if (Network.isServer)
			Debug.Log("Local server connection disconnected");
		else
			if (info == NetworkDisconnection.LostConnection)
		{
			Debug.Log("Lost connection to the server");
		}
		else
		{
			Debug.Log("Successfully diconnected from the server");
		}
		Application.LoadLevel(disconnectedLevel);
		if(Network.isServer)
			MasterServer.UnregisterHost();
		Destroy (this);
	}

	void OnPlayerDisconnected(NetworkPlayer networkPlayer) {
		Debug.Log("Clean up after player " + networkPlayer);
		bool updateFlag = true;
		for (int i =0; i<playersList.Count; i++) {

			if(((ArrayList)playersList.GetByIndex (i))[2]!=null)
			{
				if ((NetworkPlayer)((ArrayList)playersList.GetByIndex (i))[2] == networkPlayer)
				{
					if((bool)((ArrayList)playersList.GetByIndex (i))[3] == true)
						updateFlag=false;
				}
			}
		}
		Network.RemoveRPCs(networkPlayer);
		Network.DestroyPlayerObjects(networkPlayer);
		GetComponent<NetworkView>().RPC ("refreshPlayersList", RPCMode.All, networkPlayer);
		if ((gameController != null)&&(updateFlag))
		{
			GetComponent<NetworkView>().RPC ("decreaseAllPlayersCount", RPCMode.AllBuffered);
		}

	}

	void OnConnectedToServer()
	{
		Debug.Log("Connected to server");
		StartCoroutine(WaitForAddPlayerToList());
	}
	
	void OnServerInitialized() {
		Debug.Log("Server initialized and ready");
		GetComponent<NetworkView>().RPC ("updatePlayersList", RPCMode.AllBuffered, playerName, Network.player);
	}

	void OnFailedToConnect(NetworkConnectionError error) {
		Debug.Log("Could not connect to server: " + error);
		Application.LoadLevel(disconnectedLevel);
		Destroy (this);
	}

	IEnumerator WaitForSpawn() {
		Debug.Log("Before Waiting 2 seconds");
		loading = true;
		yield return new WaitForSeconds(2);
		Debug.Log("After Waiting 2 Seconds");

		// Allow receiving data again
		Network.isMessageQueueRunning = true;
		// Now the level has been loaded and we can start sending out data to clients
		Network.SetSendingEnabled(0, true);

		foreach (GameObject go in FindObjectsOfType(typeof(GameObject)))
			go.SendMessage("OnNetworkLoadedLevel", SendMessageOptions.DontRequireReceiver); 

		getGameController ();
		cameraController = gameController.getMainCamera ();
		startPositions = gameController.getStartPositions ();

		for(int i =0;i<playersList.Count; i++)
		{
			if(playersList.GetKey(i).ToString()==playerName)
			{
				spawnPlayer (i);
				GetComponent<NetworkView>().RPC ("readyPlayer", RPCMode.AllBuffered, i);
			}

			//tu dodać spawn botów tylko dla serwera
			if(Network.isServer)
			{
				foreach(string botName in botNames)
				{
					if((botName==playersList.GetKey(i).ToString())&&(((ArrayList)playersList.GetByIndex (i))[2] == null))
					{
						spawnBot (i, botName);
					}
				}
			}
		}
		loading = false;


	}

	IEnumerator WaitForAddPlayerToList()
	{
		yield return new WaitForSeconds(1);
		Debug.Log("After Waiting 1 Seconds");
		if(!playersList.ContainsKey(playerName))
		{
			GetComponent<NetworkView>().RPC ("updatePlayersList", RPCMode.AllBuffered, playerName, Network.player);
		}
		else
		{
			disconnectFromGame();
		}
	}
	
	private void spawnPlayer(int i)
	{
		GetComponent<NetworkView>().RPC ("setPlayerPrefabName", RPCMode.All, playerName);
		GetComponent<NetworkView>().RPC ("setPlayerBodyMaterial", RPCMode.All, i);
		thisPlayer = (GameObject)Network.Instantiate(player, startPositions[i].transform.position, startPositions[i].transform.rotation,0);
		cameraController.player = thisPlayer;
		gameController.setThisPlayer (thisPlayer);
	}

	private void spawnBot(int i, string botName)
	{
		GetComponent<NetworkView>().RPC ("setEnemyPrefabName", RPCMode.All, botName);
		GetComponent<NetworkView>().RPC ("setEnemyBodyMaterial", RPCMode.All, i);
		GameObject newEnemy = (GameObject)Network.Instantiate(enemyBot, startPositions[i].transform.position, startPositions[i].transform.rotation,0);
		newEnemy.GetComponent <EnemyController> ().speed+=(i*2); //diffrent speed for each enemy
	}
	
	public void getGameController()
	{
		GameObject gameControllerObject = GameObject.FindWithTag ("GameController");
		if (gameControllerObject != null)
		{
			gameController = gameControllerObject.GetComponent <GameController>();
		}
		if (gameController == null)
		{
			Debug.Log ("Cannot find 'GameController' script");
		}
	}

	public void disconnectFromGame()
	{
		Network.Disconnect();
	}
}
