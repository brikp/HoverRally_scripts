using UnityEngine;
using System.Collections;

//move bullet after Instantiate on the level
public class BulletMover : MonoBehaviour 
{

	public float speed;
	public AudioClip damageSound;
	public GameObject explosion;
	
	private float lifetime;

	void OnCollisionEnter(Collision other)
	{
		if ((other.collider.tag == "Terrain")||(other.collider.tag == "Wall")||(other.collider.tag == "Wreck"))
			Destroy (gameObject);
		
		if (other.collider.tag == "Player" || other.collider.tag == "Enemy")
		{
			ParentController parentController = other.gameObject.GetComponent<ParentController>();
			parentController.getCrashSoundPlayer().clip = damageSound;
			parentController.getCrashSoundPlayer().volume = 0.5f;
			parentController.getCrashSoundPlayer().Play();
		}
	}
	
	void Start ()
	{
		GetComponent<Rigidbody>().AddForce(transform.forward * speed);
		lifetime = Time.time + 2;
	}

	void Update()
	{
		if (lifetime < Time.time)
			Destroy(gameObject);
	}

	void OnDestroy()
	{
		Instantiate (explosion, transform.position, transform.rotation);
	}
}
