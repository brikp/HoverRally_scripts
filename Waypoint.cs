using UnityEngine;
using System.Collections;

public class Waypoint : MonoBehaviour 
{
    public static Waypoint startingWaypoint;
    public bool isStart = false;
    public Waypoint[] nextWaypoints;

    void Awake() 
	{
        if (isStart)
            startingWaypoint = this;
    }

    public Vector3 calculateTargetPosition(Vector3 position) 
	{
        Vector3 result;
		
        if (Vector3.Distance(transform.position, position) < 5 && nextWaypoints.Length > 0)
			result = getNextWaypoint().transform.position;
        else
            result = transform.position;

        result += new Vector3(Random.Range(0, 5), 0, Random.Range(0, 5));
        //Debug.Log("" + result);
        return result;
    }

	public Waypoint getNextWaypoint() 
	{
		int next = Random.Range(0, nextWaypoints.Length);
		//Debug.Log(""+next, this);
		return nextWaypoints[next];
	}
}
