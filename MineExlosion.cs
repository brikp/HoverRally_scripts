using UnityEngine;
using System.Collections;

//explosion tha occurs when obcjet enters a minie
public class MineExlosion : MonoBehaviour 
{

	public float mineDamege;
    public float activationTimer;
	public AudioClip explosionSound;
	public GameObject explosion;

	private float destroyTime;
	private bool triggered;
	private float armTime;
	
	void Start() 
	{
		armTime = Time.time + activationTimer;
		triggered = false;
	}
	
	void Update() 
	{
		if (Time.time > destroyTime && triggered)
			Destroy(gameObject);
	}
	void OnCollisionEnter(Collision other)
	{
		if ((other.gameObject.tag == "Player") || (other.gameObject.tag == "Enemy"))
			if (!triggered)
			{
				if (Time.time >= armTime) {
					ParentController enterObject = other.gameObject.GetComponent<ParentController>();
					other.rigidbody.AddForce(transform.up * 300);
					enterObject.Damage(mineDamege);
					GetComponent<AudioSource>().volume = 1;
					GetComponent<AudioSource>().clip = explosionSound;
					GetComponent<AudioSource>().Play();
					destroyTime = Time.time + GetComponent<AudioSource>().clip.length;
					triggered = true;
					foreach (Transform childTransform in transform) Destroy(childTransform.gameObject);
					Instantiate (explosion, transform.position, transform.rotation);
				}
			}
	}
}
