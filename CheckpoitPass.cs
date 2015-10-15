using UnityEngine;
using System.Collections;

public class CheckpoitPass : MonoBehaviour 
{

	//each check point has his unique number and to finish race player have to pass all on the track in incrementing order
	public int checkpointNumber;
	
	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Player")
		{
			PlayerController playerController = other.gameObject.GetComponent<PlayerController>();
			playerController.setWrongWayDelayCount(0);
		}
		
		if ((other.gameObject.tag == "Player")||(other.gameObject.tag == "Enemy")) 
		{
			ParentController parentController = other.gameObject.GetComponent<ParentController>();

			if (checkpointNumber == parentController.getNextCheckpoint())
			{
				if(checkpointNumber == 0)
				{
					parentController.increaseLap();
				}

				parentController.increaseCheckpoint();
			}
		
		}		
	}
}
