using UnityEngine;
using System.Collections;

public class EnemyController : ParentController 
{

    public int shotDistance;
    public int shotAngle;
    public int collisionForce;
	private Waypoint activeWaypoint;
	private GameObject[] enemies;
	private GameObject player;
	private GameObject[] obstacles;
	private float nextMine;
	//private bool test;
	private Vector3 vel;
	private float mag;
	private float timeToDestroy;
	private float timeFromStart;
	private float startRotation;
	private float lastSynchronizationTime = 0f;
	private float syncDelay = 0f;
	private float syncTime = 0f;
	private Vector3 syncStartPosition;
	private Vector3 syncEndPosition;
	private Quaternion syncStartRotation;
	private Quaternion syncEndRotation;
	private bool nextBurst;
	private float burstTime;
	public GameObject turboFlare;
	public GameObject turboCore;
	private ParticleSystem flare;
	private ParticleSystem core;
	private bool runRepair = false;
	private float currentRepairTime = 0.0f;



	public override void Start()
	{
		base.Start ();
		startRotation = -180f;
		activeWaypoint = Waypoint.startingWaypoint;
		player = GameObject.FindGameObjectWithTag("Player");
		enemies = GameObject.FindGameObjectsWithTag("Enemy");	
		for (int i = 0; i < enemies.Length; i++)
			if (enemies[i] == this.gameObject)
				enemies[i] = player;
		obstacles = GameObject.FindGameObjectsWithTag("Terrain");
		timeToDestroy = 0.0f;
		rotation += startRotation;
		syncStartPosition = GetComponent<Rigidbody>().position;
		syncEndPosition = GetComponent<Rigidbody>().position;
		syncStartRotation = GetComponent<Rigidbody>().rotation;
		syncEndRotation = GetComponent<Rigidbody>().rotation;
		nextBurst = true;
		burstTime = 0.0f;
		flare = turboFlare.GetComponent<ParticleSystem> ();
		core = turboCore.GetComponent<ParticleSystem> ();
		flare.startSize = 2.0f;
		core.startSize = 2.0f;
		flare.startRotation = transform.eulerAngles.y *Mathf.Deg2Rad;	
		core.startRotation = transform.eulerAngles.y *Mathf.Deg2Rad;	
		turboFlare.SetActive (false);
		turboCore.SetActive (false);

	}

	// Update is called once per frame
	public override void Update () 
	{
		base.Update ();
		enemies = GameObject.FindGameObjectsWithTag("Enemy");
		flare.startRotation = transform.eulerAngles.y *Mathf.Deg2Rad;	
		core.startRotation = transform.eulerAngles.y *Mathf.Deg2Rad;

		if (!gameController.getCountdown()&&(!isFinishedRace))
		{
			if ((ifShoot() && Time.time > nextFire)&&(bulletNum>0)&&(Time.time > gameController.getRaceStartTime() + 3.0f)) {
	            nextFire = Time.time + fireRate;
				if(levelParameters.getSinglePlayer()==true)
					Instantiate (shot, shotSpawn.position, shotSpawn.rotation);
				else
					GetComponent<NetworkView>().RPC ("spawnBullet", RPCMode.All);
				bulletNum--;
	        }
		}

		if (Time.time > gameController.getRaceStartTime() + 5.0f)
			MineDeployment();

		RunRepairs();
	}

    void FixedUpdate() 
	{
		if((levelParameters.getSinglePlayer())||(Network.isServer))
		{
			Vector3 targetPosition = activeWaypoint.calculateTargetPosition(transform.position);
				
			if (!gameController.getCountdown()&&(!isFinishedRace))
			{
				timeFromStart += Time.deltaTime;
				if ((ifShoot())&&(!inAir)&&(nextBurst)&&(bulletNum>0))
				{
					if(burstTime<1.0f)
					{
						targetPosition = player.transform.position;
					}
					else
					{
						nextBurst = false;
					}
				}
				else
				{
					if(burstTime>10.0f)
					{
						nextBurst=true;
						burstTime = 0.0f;
					}
				}
				burstTime += Time.deltaTime;
				updateWithTargetPosition(targetPosition);
					
				RunTurbo();
			}
		}
		else
		{
			SyncedMovement();
		}
    }

    void updateWithTargetPosition(Vector3 target) 
	{
		if(!outOfTrack)
		{
			Vector3 relativeTarget = transform.InverseTransformPoint(target);
			float targetAngle = Mathf.Atan2(relativeTarget.x, relativeTarget.z);
			targetAngle *= Mathf.Rad2Deg;

			if ((!inAir)&&(timeFromStart>0.3f))
				rotation += targetAngle * speed * Time.deltaTime;
			rotationSpeed = 0.3f;
			
			GetComponent<Rigidbody>().rotation = (Quaternion.AngleAxis(rotationSpeed * rotation, Vector3.up));

			int rubberband;
			if(timeFromStart<1.0f)
				rubberband = 10;
			else if(timeFromStart<2.0f)
				rubberband = 15;
			else
				rubberband = calculateRubberband();
			if(getVelocityMagnitude()<50)
			{
				GetComponent<Rigidbody>().AddForce(transform.forward * speed * rubberband/1.1f);
				GetComponent<Rigidbody>().AddForce(-transform.right * targetAngle/2);
			}

			
			if (inAir)
				GetComponent<Rigidbody>().AddForce(Physics.gravity * 3.5f);
			
			if(isGrounded())
				inAir = false;
			
			// Zwalniamy
			if (GetComponent<Rigidbody>().velocity.magnitude > 5 && isGrounded()) 
			{
				GetComponent<Rigidbody>().drag = 10;
			}
		}else
		{
			timeToDestroy += Time.deltaTime;
			GetComponent<Rigidbody>().AddForce(Physics.gravity * 20f);
			if(timeToDestroy>5.0f)
			{
				if(levelParameters.getSinglePlayer()==true)
					Kill ();
				else
					GetComponent<NetworkView>().RPC("Kill", RPCMode.All);
			}
		}
    }

    //zwalnia przeciwnika kiedy gracz jest za daleko i przyspiesza jesli gracz jest pierwszy
    int calculateRubberband() 
	{
//		if (!player)
//			return 20;
//
//        float distance = Vector3.Distance(transform.position, player.transform.position);
//        int rubberband;
//
//		if (gameController.PlayerCurrentPosition() != 1) 
//		{
//	        if (distance > 100)
//	            rubberband = 12;
//	        else if (distance > 90)
//	            rubberband = 14;
//	        else if (distance > 80)
//	            rubberband = 16;
//	        else if (distance > 70)
//	            rubberband = 17;
//	        else if (distance > 60)
//	            rubberband = 18;
//	        else if (distance > 50)
//	            rubberband = 19;
//	        else
//	            rubberband = 20;
//		}
//		else 
//		{
//			if (distance > 100)
//				rubberband = 50;
//			else if (distance > 90)
//				rubberband = 45;
//			else if (distance > 80)
//				rubberband = 40;
//			else if (distance > 70)
//				rubberband = 35;
//			else if (distance > 60)
//				rubberband = 30;
//			else if (distance > 50)
//				rubberband = 25;
//			else
//				rubberband = 20;
//		}
        return 25;
    }

    bool ifShoot() 
	{
		foreach(GameObject player in gameController.getAllPlayers())
		{
			if ((player!=null)&&(player.GetComponent <ParentController> ().racerName!=racerName))
			{
				Vector3 relativeTarget = transform.InverseTransformPoint(player.transform.position);
				float targetAngle = Mathf.Atan2(relativeTarget.x, relativeTarget.z);
				targetAngle *= Mathf.Rad2Deg;
				
				float distance = Vector3.Distance(transform.position, player.transform.position);
				
				if (targetAngle <= shotAngle && targetAngle >= -shotAngle && distance <= shotDistance)
					return true;
			}
		}
        return false;
    }

	public override void OnTriggerEnter (Collider trigger) 
	{
		base.OnTriggerEnter (trigger);
        if (activeWaypoint.GetComponent<Collider>() == trigger) {
            activeWaypoint = activeWaypoint.getNextWaypoint();
        }
    }

    void OnCollisionEnter(Collision other) 
	{
        base.collisionDamage(other);

        if ((other.collider.tag == "Player")&&((levelParameters.getSinglePlayer())||(Network.isServer))) 
		{
            GetComponent<Rigidbody>().AddForce(other.relativeVelocity * collisionForce);
        }
    }

	void MineDeployment() 
	{
		float distance;
		float dot;


		foreach(GameObject player in gameController.getAllPlayers())
		{
			if ((player!=null)&&(player.GetComponent <ParentController> ().racerName!=racerName))
			{
				distance = Vector3.Distance(transform.position, player.transform.position);
				if (distance < 40) 
				{
					dot = Vector3.Dot(GetComponent<Rigidbody>().velocity.normalized, player.transform.position.normalized);
					if ((dot >= 0.85 && minesNum > 0 && Time.time > nextMine)&&(!isFinishedRace))
					{
						//Debug.Log("Mine! Dot: " + dot, this);
						if(levelParameters.getSinglePlayer()==true)
							Instantiate (mine, mineSpawn.position, shotSpawn.rotation);
						else
							GetComponent<NetworkView>().RPC ("spawnMine", RPCMode.All);
						minesNum--;
						nextMine = Time.time + 1.0f;
					}
				}
			}
		}
	}

	void RunRepairs() 
	{
		if ((repairOn))//&&(health<50))
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
				if(levelParameters.getSinglePlayer()==true)
				{
					updateEnemyHealth (health, racerName);
				}
				else
				{
					GetComponent<NetworkView>().RPC("updateEnemyHealth", RPCMode.All, health, racerName);
				}

			} 
			else
			{
				runRepair = false;
			}
		}

	}

	void RunTurbo() 
	{
		if (!player)
			return;

		//float distance = Vector3.Distance(transform.position, player.transform.position);
		if (turboNum > 0)// && distance > 50 && gameController.PlayerCurrentPosition() == 1)
		{
			GetComponent<Rigidbody>().AddForce (transform.forward *speed*1.4f);
			turboNum = turboNum - 0.5f;
			turboFlare.SetActive (true);
			turboCore.SetActive (true);
		}
		else
		{
			turboFlare.SetActive (false);
			turboCore.SetActive (false);
		}

	}

	bool checkJump() 
	{
		float distance = 100;
		float dot;
		foreach (GameObject i in obstacles)
		{
			if (i.transform.localScale.y <= 1)
				distance = Vector3.Distance(transform.position, i.transform.position);
			if (distance <= 6) 
			{
				dot = Vector3.Dot(GetComponent<Rigidbody>().velocity.normalized, i.transform.position.normalized);
				if (dot >= 0.8)
				{
					//Debug.Log("Jump!, dot: " + dot, this);
					return true;
				}
			}
			distance = 100;
		}
		return false;
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
