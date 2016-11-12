using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class Location : MonoBehaviour {
    public TextMesh text, allowText;
    
	public bool allowed = true;
	public float allowDist = 0.00025f;
	List<Vector2> usedPositions;
	Vector2 lastPos;
	void Start()
    {
		usedPositions = new List<Vector2> ();
        StartCoroutine("Init");
    }

	public void CheckPoint()
	{
		
		allowed = false;
		lastPos = new Vector2 (Input.location.lastData.latitude, Input.location.lastData.longitude);
		//usedPositions.Add (new Vector2(Input.location.lastData.latitude, Input.location.lastData.longitude));
		//allowText.text = "NOPE!";
	}

	public void UpdateLocation()
	{
		//text.text = string.Format("status:{5}\n x: {0}\\{1}\ny: {2}\\{3}\ntimestamp:{4}", Input.location.lastData.latitude, Input.location.lastData.longitude, Input.location.lastData.altitude, Input.location.lastData.horizontalAccuracy, Input.location.lastData.timestamp, Input.location.status);
		//Debug.LogWarning (Input.location.lastData.ToString());
		StartCoroutine("Init");

	//	Input.location.
	}

	void UpdateAllow()
	{
		var distance = Vector2.Distance (new Vector2 (lastPos.x, lastPos.y), new Vector2 (Input.location.lastData.latitude, Input.location.lastData.longitude));
		allowText.text = "Distance : " + string.Format ("{0:0.000000}", distance);
		allowed = distance > allowDist;
		/*foreach (var pos in usedPositions) 
		{
			var distance = Vector2.Distance (new Vector2 (pos.x, pos.y), new Vector2 (Input.location.lastData.latitude, Input.location.lastData.longitude));
			string a = string.Format ("{0:0.000000}", distance);
			allowText.text += "\n and " + a;
			if (distance < allowDist)
				{
					allowed = false;
					allowText.text += "NOT ALLOWED";
					break;
					
				}
		}	
		if (allowed)
			allowText.text += "ALLOWED";*/
	}

	void Update()
	{
		//if (Input.location.status == LocationServiceStatus.
		//UpdateLocation ();
	}

    IEnumerator Init()
    {
        // First, check if user has location service enabled
        if (!Input.location.isEnabledByUser)
        {
            text.text = "Is No Enabled By User";
            Debug.Log("Is No Enabled By User");
            yield break;
        }
            

        // Start service before querying location
		Input.location.Start(1,1);

        // Wait until service initializes
        int maxWait = 60;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            text.text = Input.location.status.ToString();
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Service didn't initialize in 20 seconds
        if (maxWait < 1)
        {
            text.text = "Timed out";
            Debug.Log("Timed out");
            yield break;
        }

        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            text.text = "Unable to determine device location";
            Debug.Log("Unable to determine device location");
            yield break;
        }
        else
        {
			text.text = string.Format("status:{2}\nlati: {0}\nlong{1}", Input.location.lastData.latitude, Input.location.lastData.longitude, Input.location.status);
            //text.text = "Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp;
            // Access granted and location value could be retrieved
            Debug.Log("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);
			//Input.location.
        }

		UpdateAllow ();
        // Stop service if there is no need to query location updates continuously
        Input.location.Stop();
    }
}
