using UnityEngine;
using System.Collections;

public class LocationManager : MonoBehaviour {

	public int x, y;

	// Use this for initialization
	void Start () 
	{
		Input.location.Start();
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		
		LocationInfo info = Input.location.lastData;
		Debug.Log ("Location:  " + info.altitude + "  :  " + info.latitude + " :  " + info.longitude);
	}
}
