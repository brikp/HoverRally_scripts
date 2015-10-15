using UnityEngine;
using System.Collections;

//parent class for player and enemy

public class ParentController : MonoBehaviour, IDamageable<float> 
{

	public float health;
	public float speed;
	public float backGearSpeed;
	public float breakSpeed;
	public float rotationSpeed;
	public float driftForce;
	//public float jumpHeight;
	public GameObject shot;
	public Transform shotSpawn;
	public GameObject mine;
	public Transform mineSpawn;
	public int maxAmmo;
	public int maxMines;
	public int maxTurbo;
	public string racerName;
	public GameObject wreckPrefab;
	public Material bodyMaterial;

	public GameObject[] checkPoints;
	protected float scalarVelocity;
	protected Vector3 vectorVelocity;
	protected float moveHorizontal;
	protected float moveVertical;
	protected float fireRate = 0.1f; 
	protected float nextFire = 0.0f;
	protected float rotation; 
	//protected float jumpRate = 2.0f;
	//protected float nextJump = 0.0f;
	protected GameController gameController;
	protected LevelParameters levelParameters;
	protected AudioSource crashSoundPlayer;
	protected bool killed;
	protected int lapCount;
	protected int lapNumber;
	protected int nextCheckpoint;
	protected int currentCheckpoint;
	protected int checkpointNumber;
	protected int minesNum;
	protected int bulletNum;
	protected float turboNum;
	protected bool repairOn;
	protected bool isFinishedRace;
	protected float totalTimer;
	protected float currentLapTimer;
	protected float bestLapTime;
	protected AudioClip carCrashClip;
	protected float repairTime = 5.0f;
	protected ParticleSystem particles;
	protected bool inAir;
	protected bool outOfTrack;
	protected NetworkLoader networkLoader;

	void Awake()
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
			Debug.Log ("Cannot find 'NetworkLoader' script");
		}
		checkPoints = gameController.getCheckpoints();

	}
	
	public virtual void Start ()
	{
		killed = false;
		repairOn = false;
		isFinishedRace = false;
		inAir = false;
		outOfTrack = false;

		AudioSource[] audioSources = GetComponentsInChildren<AudioSource>();
		foreach (AudioSource source in audioSources)
		{
			if (source.name == "CrashSound")
				crashSoundPlayer = source;
		}
		carCrashClip = crashSoundPlayer.clip;

		particles = GetComponent<ParticleSystem>();
		
		currentCheckpoint = 0;
		nextCheckpoint = 0;
		lapCount = 0;
		bestLapTime = 0;

		//initialization from game controller
		checkpointNumber = checkPoints.Length;
		lapNumber = gameController.lapNumber;
		minesNum = gameController.startMines;
		bulletNum = gameController.startBullets;
		turboNum = gameController.startTurbo;
		repairOn = gameController.startRepair;

		if (gameObject.tag == "Player") 
		{
			gameController.UpdateCheckpoint (currentCheckpoint, checkPoints.Length);
		}
		rotation = transform.eulerAngles.y / rotationSpeed;

	}

	public virtual void Update(){
	
		if ((!gameController.getCountdown ())&&(!isFinishedRace))
		{
			totalTimer += Time.deltaTime;
			currentLapTimer += Time.deltaTime;
			if (gameObject.tag == "Player") 
			{
				gameController.UpdateCurrentLapTime(currentLapTimer);
			}
		}
			
		//end of race when player finish last lap
		if (lapCount == lapNumber + 1)
		{
			if(!isFinishedRace) 
			{
				string s = racerName + " Time: " + getTotalTime()+ "   Condition: " + (int)health;
				if(levelParameters.getSinglePlayer()==true)
					addToFinalResults(s);
				else
					if((gameObject.tag == "Player")||(Network.isServer))
						GetComponent<NetworkView>().RPC ("addToFinalResults", RPCMode.All, s);

				if (gameObject.tag == "Player")
				{
					gameController.EndOfRace ();
				}
				isFinishedRace = true;
			}
				
		}
		//Debug.Log(""+rigidbody.velocity.magnitude, this);
		/*
		if (rigidbody.velocity.magnitude > 0 && isGrounded() && !particles.isPlaying) 
			particles.Play();
		if (rigidbody.velocity.magnitude < 0 || !isGrounded()) 
			particles.Stop();
		*/
	}

	[RPC]
	public void addToFinalResults(string s)
	{
		gameController.addToResultList (s);
	}

	//damege taken when collision occurs
	public void Damage(float damageTaken)
	{
		if (!isFinishedRace)
		{
			if(health>0)
				health -= damageTaken;

			if((GetComponent<NetworkView>().isMine)||(levelParameters.getSinglePlayer()==true))
			{
				if (gameObject.tag == "Player") 
				{
					if(health <= 0)
						gameController.UpdateHealth (0.0f);
					else
						gameController.UpdateHealth (health);
				}

				if ((health <= 0) &&(!killed))
				{
					if(levelParameters.getSinglePlayer()==true)
					{
						Kill ();
						GameObject wreck = Instantiate(wreckPrefab, transform.position, transform.rotation) as GameObject;
						wreck.GetComponent<Rigidbody>().AddForce(Vector3.forward*500.0f);
					}
					else
					{
						GetComponent<NetworkView>().RPC("Kill", RPCMode.All);
							
						if(((gameObject.tag == "Player")&&(GetComponent<NetworkView>().isMine))||((gameObject.tag == "Enemy")&&(Network.isServer)))
						{
							GameObject wreck = Network.Instantiate(wreckPrefab, transform.position, transform.rotation,0) as GameObject;
							wreck.GetComponent<Rigidbody>().AddForce(Vector3.forward*500.0f);

						}
							
					}
						
					if (gameObject.tag == "Player") //game over jest ok dla isMine
					{
						gameController.GameOver();
					}
				}

				if(levelParameters.getSinglePlayer()==true)
				{
					updateEnemyHealth (health, racerName);
				}
				else
				{
					GetComponent<NetworkView>().RPC("updateEnemyHealth", RPCMode.All, health, racerName);
				}
			}
		}	
	}

	[RPC]
	public void updateEnemyHealth (float health, string racerName)
	{
		if(health <= 0)
			gameController.UpdateEnemyHealth(0.0f,racerName);
		else
			gameController.UpdateEnemyHealth(health,racerName);
	}

	//destroy player or enemy object
	[RPC]
	public void Kill()
	{
	
		gameController.UpdateEnemyHealth(0.0f,racerName);
		killed = true;
		Destroy (gameObject);

		if ((gameObject.tag == "Enemy")||(gameObject.tag == "Player")) //also check player for multiplayer game
		{
			gameController.UpdateEnemies(racerName);
		}

		if(levelParameters.getMultiPlayer())
		{
			networkLoader.deadPlayer(racerName);
		}

	}
	
	public bool isKilled()
	{
		return killed;
	}

	public float getVelocityMagnitude()
	{
		return GetComponent<Rigidbody>().velocity.magnitude;
	}

	public int getNextCheckpoint()
	{
		return nextCheckpoint;
	}

	public int getCurrentCheckpoint()
	{
		return currentCheckpoint;
	}

	public int getLapCount()
	{
		return lapCount;
	}

	public string getTotalTime()
	{
		System.TimeSpan t = System.TimeSpan.FromSeconds(totalTimer);
		return string.Format("   {0:00}:{1:00}:{2:000}", t.Minutes, t.Seconds, t.Milliseconds);
	}

	public bool getIsFinishedRace()
	{
		return isFinishedRace;
	}

	public void increaseCheckpoint()
	{
		if (nextCheckpoint == (checkpointNumber-1)) 
		{
			nextCheckpoint = 0; 
		} 
		else
			nextCheckpoint++;

		if (currentCheckpoint == (checkpointNumber)) 
		{
			currentCheckpoint = 1; 
		} 
		else
			currentCheckpoint++;


		if ((gameObject.tag == "Player") &&((GetComponent<NetworkView>().isMine)||(levelParameters.getSinglePlayer()==true)))
			gameController.UpdateCheckpoint (currentCheckpoint, checkPoints.Length);
		
	}

	public void increaseLap()
	{
		if (((currentLapTimer < bestLapTime)||(bestLapTime ==0))&&(lapCount!=0))
		{
			bestLapTime = currentLapTimer;
			if ((gameObject.tag == "Player") &&((GetComponent<NetworkView>().isMine)||(levelParameters.getSinglePlayer()==true))) 
				gameController.UpdateBestLapTime(bestLapTime);
		}
		currentLapTimer = 0;

		lapCount++;


		if (((gameObject.tag == "Player") &&((GetComponent<NetworkView>().isMine)||(levelParameters.getSinglePlayer()==true))) && (lapCount <= lapNumber))
			gameController.UpdateLap (lapCount);
	}

	public float getDistanceToCheckpoint()
	{
		return Vector3.Distance (gameObject.transform.position, checkPoints[nextCheckpoint].transform.position);
	}

	//collisions that make damege to hoover
	void OnCollisionEnter(Collision other)
	{
        collisionDamage(other);

	}

    protected void collisionDamage(Collision other) 
	{
			if ((other.collider.tag == "Wall")||(other.collider.tag == "Wreck"))
			{
				crashSoundPlayer.clip = carCrashClip;
				crashSoundPlayer.volume = 1;
				crashSoundPlayer.GetComponent<AudioSource>().Play();
				Damage(1.0f);
			}
			
			if (other.collider.tag == "Bullet") 
			{
				Damage(3.0f);
				Destroy(other.gameObject);
			}
			
			if ((other.collider.tag == "Player") || (other.collider.tag == "Enemy")) 
			{
				if (!gameController.getCountdown())	
				{
					crashSoundPlayer.clip = carCrashClip;
					crashSoundPlayer.volume = 1;
					crashSoundPlayer.GetComponent<AudioSource>().Play();
				}
				Damage(3.0f);
			}
    }
	
	public virtual void OnTriggerEnter(Collider trigger) 
	{
		if (trigger.GetComponent<Collider>().tag == "Ammo")
		{
			bulletNum += 50;
			bulletNum = (bulletNum > maxAmmo) ? maxAmmo : bulletNum;
			if ((gameObject.tag == "Player")&&((GetComponent<NetworkView>().isMine)||(levelParameters.getSinglePlayer()==true)))
				gameController.UpdateBullets(bulletNum);
			if(levelParameters.getSinglePlayer())
				Destroy(trigger.gameObject);
			else 
				if (Network.isServer)
			{
				Network.Destroy(trigger.gameObject);
			}
				
		} 
		else if (trigger.GetComponent<Collider>().tag == "Mines") 
		{
			minesNum += 3;
			minesNum = (minesNum > maxMines) ? maxMines : minesNum;
			if ((gameObject.tag == "Player")&&((GetComponent<NetworkView>().isMine)||(levelParameters.getSinglePlayer()==true)))
				gameController.UpdateMines(minesNum);
			if(levelParameters.getSinglePlayer())
				Destroy(trigger.gameObject);
			else if (Network.isServer)
				Network.Destroy(trigger.gameObject);
		} 
		else if (trigger.GetComponent<Collider>().tag == "Repair") 
		{
			repairOn = true;
			if ((gameObject.tag == "Player")&&((GetComponent<NetworkView>().isMine)||(levelParameters.getSinglePlayer()==true)))
				gameController.UpdateRepair(true);
			if(levelParameters.getSinglePlayer())
				Destroy(trigger.gameObject);
			else if (Network.isServer)
				Network.Destroy(trigger.gameObject);
		} 
		else if (trigger.GetComponent<Collider>().tag == "Boost") 
		{
			turboNum += 50;
			turboNum = (turboNum > maxTurbo) ? maxTurbo : turboNum;
			if ((gameObject.tag == "Player")&&((GetComponent<NetworkView>().isMine)||(levelParameters.getSinglePlayer()==true)))
				gameController.UpdateTurbo(turboNum);
			if(levelParameters.getSinglePlayer())
				Destroy(trigger.gameObject);
			else if (Network.isServer)
				Network.Destroy(trigger.gameObject);
		}

		if (trigger.GetComponent<Collider>().tag == "Jump") 
		{
			inAir = true;
		}

		if (trigger.GetComponent<Collider>().tag == "Edge") 
		{
			outOfTrack = true;
		} 

	}

	public bool isGrounded()
	{
		return Physics.Raycast(transform.position, -Vector3.up, 2.0f);
	}

	public AudioSource getCrashSoundPlayer()
	{
		return crashSoundPlayer;
	}

	[RPC]
	void spawnBullet() 
	{
		GameObject go = (GameObject)Instantiate (shot, shotSpawn.position, shotSpawn.rotation);
		go.transform.parent = transform;
	}
	
	[RPC]
	void spawnMine() 
	{
		Instantiate (mine, mineSpawn.position, shotSpawn.rotation);
	}
}