using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour 
{

	public GameObject player;
	public GameObject enemy;
	public Material[] hoverMaterials;
	public Material[] markerMaterials;

	//GUI
	public GUIText velocityText;
	public GUIText healthText;
	public GUIText gameOverText;
	public GUIText winText;
	public GUIText checkpointText;
	public GUIText lapText;
	public GUIText bulletsText;
	public GameObject bulletsAnchor;
	public GameObject turboAnchor;
	public GameObject healthAnchor;
	public GameObject[] enemiesAnchorns;
	public GUIText minesText;
	public GameObject[] mineIcons;
	public GameObject[] repairIcons;
	public Texture gaugePointerTexture;
	public GameObject gaugePointerPivot;
	public GUIText repairText;
	public GUIText positionText;
	public GUIText wrongWayText;
	public GUIText showText;
	public GUIText currentLapTimeText;
	public GUIText bestLapTimeText;
	public GUIText countdownTimer;
	public GUIText exitText;

	//total number of laps in race and checkpoints
	public int lapNumber;

	//player parameters on start
	public int startMines;
	public int startBullets;
	public int startTurbo;
	public bool startRepair;

	private GameObject thisPlayer;
	private int maxPlayers = 4;
	private CameraController cameraController;
	private NetworkLoader networkLoader;
	private ArrayList allPlayers;
	private GameObject[] enemies;
	private GameObject[] players;
	private GameObject[] checkpoints;
	private GameObject[] startPositions;
	private string[] enemyNames = {"Frank", "Rambo", "Leon"};
	public ArrayList finalResults;
	private bool gameOver;
	private bool allEnemiesKilled;
	private bool endOfRace;
	private bool countdown;
	private bool showResultsDelay;
	private float countdownTime;
	private int playerPosition;
	protected float raceStartTime;
	private LevelParameters levelParameters;
	private bool allNetworkReady = false;
	private int allPlayersCount;
	private bool showGameMenu;
	public Texture gameMenuTexture;
	private int cameraCounter;
	private string[] menuOptions = { "Help", "Exit Game" ,"Return to game (Esc)"};
	private int selectedIndex;
	private bool guiEnter;
	private HelpWindow window;
	private Rect helpRect = new Rect(Screen.width / 2 - 100, Screen.height / 2 - 150, 200, 300);
	private GameObject gui;
	private GameObject miniMap;
	void Awake()
	{
		getLevelParameters ();
		startPositions = getStartPositions ();
		checkpoints = getCheckpoints();
		cameraController = getMainCamera();
		if (levelParameters.getMultiPlayer ())
		{
			getNetworkLoader();
		}
		gui = GameObject.FindWithTag ("GUI");
		miniMap = GameObject.FindWithTag ("MiniMap");
		gui.SetActive(false);
		miniMap.SetActive(false);
	}

	void Start ()
	{	

		foreach(GameObject enemyAnchor in enemiesAnchorns)
		{
			enemyAnchor.transform.parent.gameObject.SetActive(false);
		}

		guiEnter = false;
		selectedIndex = 0;
		showGameMenu = false;
		Time.timeScale = 1;
		allPlayersCount = 0;
		cameraCounter = 0;
		if (levelParameters.getSinglePlayer())
		{
			allNetworkReady = true;
			thisPlayer = (GameObject)Instantiate (player, startPositions[0].transform.position, startPositions[0].transform.rotation);
			thisPlayer.GetComponent <PlayerController> ().racerName = "Player 001";
			cameraController.player = thisPlayer;
			thisPlayer.transform.Find("_Body").GetComponent<Renderer>().sharedMaterial = hoverMaterials[0];
			thisPlayer.transform.Find("Mini Map Marker").GetComponent<Renderer>().sharedMaterial = markerMaterials[0];
			thisPlayer.GetComponent <PlayerController> ().bodyMaterial = hoverMaterials[0];
			for(int i=1;i<maxPlayers;i++){
				GameObject newEnemy = (GameObject)Instantiate (enemy, startPositions[i].transform.position, startPositions[i].transform.rotation);
				newEnemy.GetComponent <EnemyController> ().checkPoints = checkpoints;
				newEnemy.GetComponent <EnemyController> ().racerName = enemyNames[i-1];
				newEnemy.GetComponent <EnemyController> ().speed+=(i*2); //diffrent speed for each enemy
				newEnemy.transform.Find("_Body").GetComponent<Renderer>().sharedMaterial = hoverMaterials[i];
				newEnemy.transform.Find("Mini Map Marker").GetComponent<Renderer>().sharedMaterial = markerMaterials[i];
				newEnemy.GetComponent <EnemyController> ().bodyMaterial = hoverMaterials[i];
			}
		}

		gameOver = false;
		allEnemiesKilled = false;
		endOfRace = false;
		showResultsDelay = false;
		healthText.text = "Hoover condition: 100";
		velocityText.text = "0 km/h";
		gameOverText.text = "";
		winText.text = "";
		wrongWayText.text = "";
		showText.text = "";
		exitText.text = "";
		lapText.text = "Lap: 1/" + lapNumber.ToString();
		minesText.text = "Mines: " +  startMines.ToString();
		countdownTimer.text = "";
		UpdateRepair (startRepair);
		countdown = true;
		countdownTime = 0.0f;
		UpdateCurrentLapTime (0);
		bestLapTimeText.text = "Best lap time: -------------";
		finalResults = new ArrayList();
		allPlayers = new ArrayList();
		UpdateBullets (startBullets);
		UpdateTurbo (startTurbo);
		UpdateMines (startMines);
		UpdateRepair (startRepair);
		window = gameObject.AddComponent<HelpWindow>() as HelpWindow;
		window.setButtonName("Return to game (Esc)");

	}

	void OnGUI ()
	{
		if(showGameMenu)
		{
			if(!window.getHelpActive())
			{
				if(Event.current.Equals (Event.KeyboardEvent("return")))
				{
					guiEnter = true;
				}
				if(levelParameters.getSinglePlayer())
					Time.timeScale = 0;
				GUI.BeginGroup (new Rect (Screen.width / 2 - 100, Screen.height / 2-120, 200, 200));
				GUI.Box (new Rect (0,0,200,200), gameMenuTexture);
				GUI.Label(new Rect(65,20,100,20), "Game Menu");
				GUI.SetNextControlName (menuOptions[0]);
				if(selectedIndex == 0)
					GUI.color = Color.yellow;
				if((GUI.Button (new Rect (30,50,140,30), menuOptions[0]))||((guiEnter)&&(selectedIndex == 0)))
				{
					guiEnter = false;
					window.setHelpActive(true);
				}
				GUI.color = Color.white;
				GUI.SetNextControlName (menuOptions[1]);
				if(selectedIndex == 1)
					GUI.color = Color.yellow;
				if((GUI.Button (new Rect (30,90,140,30), menuOptions[1]))||((guiEnter)&&(selectedIndex == 1)))
				{

					if(levelParameters.getSinglePlayer()==true)
					{
						Destroy(levelParameters);
						Application.LoadLevel("MainMenu");
						//Debug.Log (levelParameters);
					}
					else
					{
						networkLoader.disconnectFromGame();

					}
					guiEnter = false;
				}
				GUI.color = Color.white;
				GUI.SetNextControlName (menuOptions[2]);
				if(selectedIndex == 2)
					GUI.color = Color.yellow;
				if((GUI.Button (new Rect (30,150,140,30), menuOptions[2]))||((guiEnter)&&(selectedIndex == 2)))
				{
					showGameMenu = false;
					Time.timeScale = 1.0f;
					guiEnter = false;
				}
				GUI.color = Color.white;
				GUI.EndGroup ();
				GUI.FocusControl (menuOptions[selectedIndex]);
			}
			else
			{
				helpRect = GUI.Window(0, helpRect, window.helpWindow, "Help");
			}
		}
	}

	void Update()
	{
		if(allNetworkReady)
		{
			gui.SetActive(true);
			miniMap.SetActive(true);

			if(players==null)
			{
				enemies = GameObject.FindGameObjectsWithTag("Enemy");
				players = GameObject.FindGameObjectsWithTag("Player");
				allPlayersCount+=enemies.Length;
				allPlayersCount+=players.Length;
				foreach (GameObject player in players)
				{
					allPlayers.Add (player);
					//Debug.Log (player.GetComponent <ParentController> ().racerName);
				}
				foreach (GameObject enemy in enemies)
				{
					allPlayers.Add (enemy);
					//Debug.Log (enemy.GetComponent <EnemyController> ().racerName);
				}
				int i =0;
				foreach(GameObject onePlayer in allPlayers)
				{
					if(onePlayer.GetComponent <ParentController> ().racerName!=thisPlayer.GetComponent <ParentController> ().racerName)
					{
						enemiesAnchorns[i].GetComponent <GUIText> ().text = onePlayer.GetComponent <ParentController> ().racerName;
						if(onePlayer.GetComponent <ParentController> ().bodyMaterial==hoverMaterials[0])
						{
							enemiesAnchorns[i].GetComponent <GUIText> ().color = Color.blue;
						}
						else if(onePlayer.GetComponent <ParentController> ().bodyMaterial==hoverMaterials[1])
						{
							enemiesAnchorns[i].GetComponent <GUIText> ().color = Color.red;
						}
						else if(onePlayer.GetComponent <ParentController> ().bodyMaterial==hoverMaterials[2])
						{
							enemiesAnchorns[i].GetComponent <GUIText> ().color = Color.green;
						}
						else if(onePlayer.GetComponent <ParentController> ().bodyMaterial==hoverMaterials[3])
						{
							Color orange = new Color(1.0f,0.6f,0.0f,1.0f);
							enemiesAnchorns[i].GetComponent <GUIText> ().color = orange;
						}
						UpdateEnemyHealth(onePlayer.GetComponent <ParentController> ().health, onePlayer.GetComponent <ParentController> ().racerName);
						enemiesAnchorns[i].transform.parent.gameObject.SetActive(true);
						i++;
					}
				}

			}
		
			foreach(GameObject enemyAnchorn in enemiesAnchorns)
			{
				if(Input.GetKey(KeyCode.Tab))
					enemyAnchorn.transform.parent.gameObject.SetActive(true);
				else
					enemyAnchorn.transform.parent.gameObject.SetActive(false);
			}

			if(Input.GetKey(KeyCode.Tab))
				bestLapTimeText.enabled = true;
			else
				bestLapTimeText.enabled = false;

			if(Input.GetKey(KeyCode.Tab))
				currentLapTimeText.enabled = true;
			else
				currentLapTimeText.enabled = false;

			countdownTime += Time.deltaTime;
			
			PlayerCurrentPosition ();
			
			if (countdownTime > 3.0f) 
			{
				countdown = false;
			}
			
			if (countdown)
				raceStartTime = Time.time;

			//finishing race
			if (endOfRace) 
			{
				setExitText();
				UpdateWrongWay(false);
				winText.text = "YOU FINISHED RACE ON POSITION: " + playerPosition + "!";
				showText.text ="Wait for other players to finish race.";
				if(showResultsDelay)
				{
					setShowResultText();
					if (Input.GetKeyDown (KeyCode.S))
					{
						levelParameters.setResults(getResults());
						if(levelParameters.getMultiPlayer())
							if(!Network.isServer)
								networkLoader.disconnectFromGame();
						Application.LoadLevel("Highscores");
					}
				}
			}

			if((((endOfRace)||(gameOver))&&(!allEnemiesKilled)||(thisPlayer==null))&&(allPlayersCount>0))
			{
				if ((Input.GetKeyDown (KeyCode.Space))&&(!showGameMenu))
				{
					bool flag = true;
					do
					{
						cameraCounter++;
						if(cameraCounter==allPlayers.Count)
							cameraCounter = 0;
						if(!allPlayers[cameraCounter].Equals(null))
						{
							cameraController.player = (GameObject)allPlayers[cameraCounter];
							flag=false;
						}
					}while(flag);
				}
			}

			//win by killing all enemies
			if ((allEnemiesKilled)&&(!gameOver)) 
			{
				winText.text = "YOU KILLED ALL ENEMIES! YOU WIN!";
				UpdateWrongWay(false);
				setExitText();
			}

			//game over whey player have been destroyed
			if ((gameOver)&&(!allEnemiesKilled)&&(!endOfRace)&&(thisPlayer==null))
			{
				gameOverText.text = "GAME OVER!";
				UpdateWrongWay(false);
				setExitText();
			}

			if(Input.GetKeyDown (KeyCode.Escape))
			{
				if(showGameMenu)
				{
					showGameMenu = false;
					Time.timeScale = 1.0f;
				}
				else
				{
					showGameMenu = true;
					selectedIndex = 0;
				}
					
			}

			if(finalResults.Count==allPlayersCount)
			{
				showResultsDelay = true;
			}
			if(showGameMenu)
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

	public GameObject[] getStartPositions()
	{
		return GameObject.FindGameObjectsWithTag("StartPosition");
	}

	public void getNetworkLoader()
	{
		GameObject networkLoaderObject = GameObject.FindWithTag ("NetworkLoader");
		if (networkLoaderObject != null)
		{
			networkLoader = networkLoaderObject.GetComponent <NetworkLoader>();
		}
		if (networkLoader == null)
		{
			Debug.Log ("Cannot find 'NetworkLoader' script");
		}
	}


	public CameraController getMainCamera()
	{
		GameObject cameraControllerObject = GameObject.FindWithTag ("MainCamera");
		if (cameraControllerObject != null)
		{
			return cameraControllerObject.GetComponent <CameraController>();
		}
		else
		{
			Debug.Log ("Cannot find 'GameController' script");
			return null;
		}

	}

	public void getLevelParameters()
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
	}

	public GameObject[] getCheckpoints()
	{
		GameObject[] checkpointsToSort = GameObject.FindGameObjectsWithTag("Checkpoint");
		GameObject[] checkpointsAfterSort = new GameObject[checkpointsToSort.Length];
		for(int i=0; i<checkpointsToSort.Length; i++)
		{
			checkpointsAfterSort[checkpointsToSort[i].GetComponent <CheckpoitPass>().checkpointNumber] = checkpointsToSort[i];
		}
		return checkpointsAfterSort;
	}


	public int PlayerCurrentPosition()
	{
		
		if ((thisPlayer != null)&&(!endOfRace))
		{
			PlayerController playerController = thisPlayer.GetComponent<PlayerController>();
			playerPosition = 1;

			foreach (GameObject enemy in enemies)
			{
				if(enemy!=null)
				{
					EnemyController enemyController = enemy.GetComponent<EnemyController>();

					if(playerController.getLapCount()<enemyController.getLapCount())
					{
						playerPosition++;
					}
					else if (playerController.getLapCount()==enemyController.getLapCount())
					{
						if(playerController.getCurrentCheckpoint()<enemyController.getCurrentCheckpoint())
						{
							playerPosition++;
						}
						else if(playerController.getCurrentCheckpoint()==enemyController.getCurrentCheckpoint())
						{
							if(playerController.getDistanceToCheckpoint()>enemyController.getDistanceToCheckpoint())
							{
								playerPosition++;

							}
						}
					}
				}
			}
			foreach (GameObject player in players)
			{
				if((player!=null)&&(player!=thisPlayer))
				{
					PlayerController enemyController = player.GetComponent<PlayerController>();
					
					if(playerController.getLapCount()<enemyController.getLapCount())
					{
						playerPosition++;
					}
					else if (playerController.getLapCount()==enemyController.getLapCount())
					{
						if(playerController.getCurrentCheckpoint()<enemyController.getCurrentCheckpoint())
						{
							playerPosition++;
						}
						else if(playerController.getCurrentCheckpoint()==enemyController.getCurrentCheckpoint())
						{
							if(playerController.getDistanceToCheckpoint()>enemyController.getDistanceToCheckpoint())
							{
								playerPosition++;
								
							}
						}
					}
				}
			}
		}
		UpdatePosition (playerPosition);
		return playerPosition;
	}
	
	private void setExitText()
	{
		exitText.text = "Press 'Esc' to show Game Menu or 'Space' to observe other players";
	}

	private void setShowResultText()
	{
		showText.text = "Press 'S' to show Results";
	}

	public void setShowResultsDelay(bool delay)
	{
		showResultsDelay = true;
	}

	public bool getShowResultsDelay()
	{
		return showResultsDelay;
	}

	public void UpdateHealth(float health)
	{
		healthText.text = "Hoover condition:: " + health.ToString ("0");
		healthAnchor.transform.localScale = new Vector3((float)health/100, 1, 1);
	}

	public void UpdateEnemyHealth(float health, string name)
	{
		for(int i=0; i<enemiesAnchorns.Length; i++)
		{
			if(enemiesAnchorns[i].GetComponent<GUIText>().text == name)
			{
				enemiesAnchorns[i].transform.localScale = new Vector3((float)health/100, 1, 1);
			}
				
		}
	}

	public void UpdateVelocity(float velocity)
	{
		velocityText.text = (velocity*3.5f).ToString ("0") + " km/h";
		
		//GUIUtility.RotateAroundPivot(clockNeedlRotation, Vector2(clockNeedlOffset.x, clockNeedlOffset.y+clockNeedl.height));
		//DrawImage(clockNeedlOffset, clockNeedl);
		//GUIUtility.RotateAroundPivot(-clockNeedlRotation, Vector2(clockNeedlOffset.x, clockNeedlOffset.y+clockNeedl.height));

		//pivotPoint = new Vector2(transform.localPosition.x, transform.localPosition.y);
		//GUIUtility.RotateAroundPivot (rotAngle, pivotPoint);
		//draw and return to original
	}

	public void UpdateCheckpoint(int checkpoint, int total)
	{
		checkpointText.text = "Checkpoint: " + checkpoint.ToString () +"/" + total.ToString();
	}

	public void UpdateLap(int lap)
	{
		lapText.text = "Lap: " + lap.ToString () +"/" + lapNumber.ToString();
	}

	public void UpdateMines(int mines)
	{
		minesText.text = "Mines: " + mines.ToString ();
		foreach(GameObject o in mineIcons)
		{
			o.SetActive(false);
		}
		for (int i = 0; i < mines; i++)
			mineIcons[i].SetActive(true);
	}

	public void UpdateBullets(int bullets)
	{
		bulletsText.text = bullets.ToString() + "/100";
		bulletsAnchor.transform.localScale = new Vector3((float)bullets/100, 1, 1);
	}

	public void UpdateTurbo(float turbo)
	{
		turboAnchor.transform.localScale = new Vector3(turbo/100, 1, 1);
	}

	public void UpdateRepair(bool repair)
	{
		if(repair) 
		{
			repairIcons[0].SetActive(true);
			repairIcons[1].SetActive(false);
			repairText.text = "READY!";
		}
		else
		{
			repairIcons[0].SetActive(false);
			repairIcons[1].SetActive(true);
			repairText.text = "-----------";
		}
	}

	public void GameOver()
	{
		gameOver = true;
	}

	public void EndOfRace()
	{
		endOfRace = true;
	}

	public bool getEndOfRace() 
	{
		return endOfRace;
	}

	public void setAllNetworkReady(bool ready)
	{
		allNetworkReady = ready;
	}

	public bool getAllNetworkReady()
	{
		return allNetworkReady;
	}

	//checking if there is any enemy left if its not set flag to end round
	public void UpdateEnemies(string name)
	{
		decreaseAllPlayersCount ();
		if(thisPlayer!=null)
			if ((allPlayersCount == 1)&&(thisPlayer.GetComponent <ParentController> ().racerName!=name))
				allEnemiesKilled = true;
	}

	public bool getCountdown() 
	{
		return countdown;
	}

	public void UpdatePosition(int position)
	{
		string txt = "";
		switch(position) 
		{
		case 1:
			txt = "1st";
			break;
		case 2:
			txt = "2nd";
			break;
		case 3:
			txt = "3rd";
			break;
		case 4:
			txt = "4th";
			break;
		}
		positionText.text = txt;
	}

	public void UpdateWrongWay(bool wrongWay)
	{
		if(wrongWay)
			wrongWayText.text = "WRONG WAY!";
		else
			wrongWayText.text = "";
	}

	public string getResults()
	{
		string allResult = "";
		int counter = 0;
			foreach (string finalResult in finalResults)
			{
				counter++;
				allResult += counter.ToString() + ". " + finalResult  + "\n\n";
			}
		return allResult;
	}

	public void UpdateCurrentLapTime(float timer)
	{	
		string timerFormatted;
		System.TimeSpan t = System.TimeSpan.FromSeconds(timer);
		timerFormatted = string.Format("Current lap time: {0:00}:{1:00}:{2:000}", t.Minutes, t.Seconds, t.Milliseconds);
		currentLapTimeText.text = timerFormatted;
	}

	public void UpdateBestLapTime(float timer)
	{
		string timerFormatted;
		System.TimeSpan t = System.TimeSpan.FromSeconds(timer);
		timerFormatted = string.Format("Best lap time: {0:00}:{1:00}:{2:000}", t.Minutes, t.Seconds, t.Milliseconds);
		bestLapTimeText.text = timerFormatted;
	}

	public float getRaceStartTime()
	{
		return raceStartTime;
	}

	public void setThisPlayer(GameObject p)
	{
		thisPlayer = p;
	}

	public GameObject getThisPlayer()
	{
		return thisPlayer;
	}

	public void decreaseAllPlayersCount()
	{
		allPlayersCount--;
	}

	public void addToResultList(string racerName)
	{
		finalResults.Add(racerName);
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

	public ArrayList getAllPlayers()
	{
		return allPlayers;
	}
	
}