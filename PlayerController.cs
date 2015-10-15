using UnityEngine;
using System.Collections;

public class PlayerController : ParentController 
{

	private bool runRepair = false;
	private float currentRepairTime = 0.0f;
	private float previousDistenceToCheckPoint;
	private float wrongWayDelayCount;
	private float timeToShowResult;
	public AudioClip finishSound;
	private WheelCollider wheel;
	private bool handbrake;
	private float lastSynchronizationTime = 0f;
	private float syncDelay = 0f;
	private float syncTime = 0f;
	private Vector3 syncStartPosition;
	private Vector3 syncEndPosition;
	private Quaternion syncStartRotation;
	private Quaternion syncEndRotation;
	public GameObject turboFlare;
	public GameObject turboCore;
	private ParticleSystem flare;
	private ParticleSystem core;
	
	public override void Start()
	{
		base.Start ();
		previousDistenceToCheckPoint = getDistanceToCheckpoint ();
		wrongWayDelayCount = 0;
		timeToShowResult = 0.0f;
		wheel = gameObject.GetComponent<WheelCollider> ();
		handbrake = false;
		syncStartPosition = GetComponent<Rigidbody>().position;
		syncEndPosition = GetComponent<Rigidbody>().position;
		syncStartRotation = GetComponent<Rigidbody>().rotation;
		syncEndRotation = GetComponent<Rigidbody>().rotation;
		flare = turboFlare.GetComponent<ParticleSystem> ();
		core = turboCore.GetComponent<ParticleSystem> ();
		flare.startSize = 2.0f;
		core.startSize = 2.0f;
		turboFlare.SetActive (false);
		turboCore.SetActive (false);
	}

	public override void Update()
	{
			base.Update ();
			flare.startRotation = transform.eulerAngles.y *Mathf.Deg2Rad;	
			core.startRotation = transform.eulerAngles.y *Mathf.Deg2Rad;
			if ((levelParameters.getSinglePlayer ()) || (GetComponent<NetworkView>().isMine))
			{
				//update GUI velocity
				gameController.UpdateVelocity (getVelocityMagnitude ());
				
				//delay after finish race by player to increase time scale and show results
				if (isFinishedRace)
				{
					crashSoundPlayer.clip = finishSound;
					if (!crashSoundPlayer.isPlaying)
						crashSoundPlayer.Play();
					
					if(levelParameters.getSinglePlayer()==true)
					{
						timeToShowResult+=Time.deltaTime;
						if(timeToShowResult>3)
						{
							Time.timeScale = 100;
							//gameController.setShowResultsDelay(true);
						}
					}
				}
				
				if ((!gameController.getCountdown ()&&(!isFinishedRace))) 
				{
					wrongWayDelayCount+=Time.deltaTime;
					
					//FIRE
					if ((Input.GetButton ("Fire1") && Time.time > nextFire) && (bulletNum > 0))
					{
						nextFire = Time.time + fireRate;
						
						if(levelParameters.getSinglePlayer()==true)
							Instantiate (shot, shotSpawn.position, shotSpawn.rotation);
						else
							GetComponent<NetworkView>().RPC ("spawnBullet", RPCMode.All);
						
						bulletNum--;
						gameController.UpdateBullets (bulletNum);
						//audio.Play ();
					}
					
					//MINE
					if ((Input.GetKeyDown (KeyCode.X)) && (minesNum > 0))
					{
						
						if(levelParameters.getSinglePlayer()==true)
							Instantiate (mine, mineSpawn.position, shotSpawn.rotation);
						else
							GetComponent<NetworkView>().RPC ("spawnMine", RPCMode.All);
						minesNum--;
						gameController.UpdateMines (minesNum);
					}
					
					//REPAIR
				if ((repairOn)) //&& (Input.GetKeyDown (KeyCode.C)))
					{
						runRepair = true;
						repairOn = false;
						gameController.UpdateRepair (false);
						currentRepairTime = 0.0f;
					}
					
					if ((runRepair))
					{
						currentRepairTime += Time.deltaTime;
						if ((health < 100)&&(currentRepairTime<repairTime))
						{
							health = health + 0.1f;
							gameController.UpdateHealth (health);

							if(levelParameters.getSinglePlayer()!=true)
							{
								GetComponent<NetworkView>().RPC("updateEnemyHealth", RPCMode.All, health, racerName);
							}
						} 
						else
						{
							runRepair = false;
						}
					}
					
					
					//chceck if player goes in wrong direction
					if(wrongWayDelayCount>1.0f)
					{
						if (previousDistenceToCheckPoint < (getDistanceToCheckpoint()-0.1f)) // -0.1f dont show when collision with terrain occurs
						{
							gameController.UpdateWrongWay(true);
						}
						else
						{
							gameController.UpdateWrongWay(false);
						}
						
						wrongWayDelayCount = 0;
					}
					previousDistenceToCheckPoint = getDistanceToCheckpoint();
				}

			}

	}

	void FixedUpdate ()
	{
		if (!gameController.getCountdown()&&(!isFinishedRace))
			if((levelParameters.getSinglePlayer())||(GetComponent<NetworkView>().isMine))
			{
				movePlayer();
			}
			else
			{
				SyncedMovement();
			}
	}

	void movePlayer() 
	{
		moveHorizontal = Input.GetAxis ("Horizontal");
		moveVertical = Input.GetAxis ("Vertical");

		//TURBO
		if ((Input.GetKey(KeyCode.Z))&&(turboNum>0))
		{
			if((getVelocityMagnitude()<75.0f))
			{
				GetComponent<Rigidbody>().AddForce (transform.forward * speed *2.0f);
			}
			turboNum = turboNum - 0.5f;
			gameController.UpdateTurbo(turboNum);
			turboFlare.SetActive (true);
			turboCore.SetActive (true);

		}
		else
		{
			turboFlare.SetActive (false);
			turboCore.SetActive (false);
		}

		float dot = Vector3.Dot(GetComponent<Rigidbody>().velocity.normalized, transform.forward.normalized);
		//move forward and backward
		if((!handbrake))
		{
			float velocityCondition = 1.0f;
			if(inAir||outOfTrack)
				velocityCondition = 0.3f;

			if ((moveVertical > 0)&&(getVelocityMagnitude()<50.0f))
			{
				GetComponent<Rigidbody>().AddForce (transform.forward * 1.5f*speed * moveVertical*velocityCondition);
			}
			else if (moveVertical < 0)
			{
				//Debug.Log("Dot = " + dot + ", mv = " + moveVertical, this);
				if (dot > 0)
					GetComponent<Rigidbody>().AddForce (transform.forward * breakSpeed * moveVertical);
				else 
					GetComponent<Rigidbody>().AddForce (transform.forward * backGearSpeed * moveVertical);
			}
		}
		else
		{
			moveVertical = 0.0f;
		}
		bool engineRunning = false;
		if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
			engineRunning = true;
		if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
			if (dot <= 0)
				engineRunning = true;

		if (engineRunning && !GetComponent<AudioSource>().isPlaying)
			GetComponent<AudioSource>().Play();
		if (!engineRunning) 
			GetComponent<AudioSource>().Stop();

		//add drift force while hand brake
		if((moveVertical !=0)&&(handbrake)&&(getVelocityMagnitude () > 10.0f)&&(!inAir)&&(!outOfTrack))
		{
			GetComponent<Rigidbody>().AddForce (-transform.right*moveHorizontal*2*driftForce);

		}
	
		//rotation along y axis
		if((getVelocityMagnitude () > 0.3f)) //(!inAir)&&(!outOfTrack)&&
		{
			int velocityCondition = 0;
			if((inAir)||(outOfTrack))
				velocityCondition = 15;
			else if((handbrake)&&(getVelocityMagnitude () > 10.0f))
				velocityCondition = 30;
			else if((handbrake)&&(getVelocityMagnitude () < 10.0f))
				velocityCondition = 0;
			else if(getVelocityMagnitude () > 12.0f)
				velocityCondition = 50;
			else if (getVelocityMagnitude () > 6.0f)
				velocityCondition = 25;
			else if (getVelocityMagnitude () > 1.0f)
				velocityCondition = 10;
			else
				velocityCondition = 5;

			rotation += moveHorizontal * speed * Time.fixedDeltaTime*velocityCondition;
		}

		if (wheel.isGrounded)
		{
			inAir = false;
			outOfTrack = false;
		}
			
		GetComponent<Rigidbody>().rotation = (Quaternion.AngleAxis (rotationSpeed * rotation, Vector3.up));	

		//handbrake
		if (Input.GetKey(KeyCode.Space))
		{
				handbrake = true;
				wheel.brakeTorque = 50;
		}
		else
		{
			wheel.brakeTorque =0;
			handbrake = false;
		}
	}

	public void setWrongWayDelayCount(float count)
	{
		wrongWayDelayCount = count;
	}

	public override void OnTriggerEnter(Collider trigger) 
	{
		base.OnTriggerEnter (trigger);
	
		if (trigger.GetComponent<Collider>().tag == "Arrow") 
		{
			trigger.GetComponent <ArrowGUI>().showArrow();
		}
		if (trigger.GetComponent<Collider>().tag == "Crate") 
		{
			trigger.GetComponent <CrateGUI>().showCrate();
		}
	}


	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{
		//Debug.Log ("serialize");
		Vector3 syncPosition = Vector3.zero;
		Vector3 syncVelocity = Vector3.zero;
		Quaternion syncRotation = Quaternion.Euler(0f,0f,0f); 


		if (stream.isWriting)
		{
			syncPosition = GetComponent<Rigidbody>().position;
			stream.Serialize(ref syncPosition);
			
			syncVelocity = GetComponent<Rigidbody>().velocity;
			stream.Serialize(ref syncVelocity);

			syncRotation = GetComponent<Rigidbody>().rotation;
			stream.Serialize(ref syncRotation);
		}
		else
		{
			stream.Serialize(ref syncPosition);
			stream.Serialize(ref syncVelocity);
			stream.Serialize (ref syncRotation);
			
			syncTime = 0f;
			syncDelay = Time.time - lastSynchronizationTime;
			lastSynchronizationTime = Time.time;

			if(syncVelocity.magnitude>25)
				syncEndPosition = syncPosition + syncVelocity * syncDelay/2;
			else
				syncEndPosition = syncPosition + syncVelocity * syncDelay;
			syncStartPosition = GetComponent<Rigidbody>().position;

			syncEndRotation = syncRotation;
			syncStartRotation = GetComponent<Rigidbody>().rotation;
		}
		
	}
	
	private void SyncedMovement()
	{
			syncTime += Time.deltaTime;
			GetComponent<Rigidbody>().position = Vector3.Lerp (syncStartPosition, syncEndPosition, syncTime / syncDelay);
			GetComponent<Rigidbody>().rotation = Quaternion.Lerp (syncStartRotation, syncEndRotation, syncTime / syncDelay);
	}
}