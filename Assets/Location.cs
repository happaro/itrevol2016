using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Location : MonoBehaviour {
    public TextMesh text;
    
	void Start()
    {
        StartCoroutine("Init");
    }

	public void UpdateLocation()
	{
		//text.text = string.Format("status:{5}\n x: {0}\\{1}\ny: {2}\\{3}\ntimestamp:{4}", Input.location.lastData.latitude, Input.location.lastData.longitude, Input.location.lastData.altitude, Input.location.lastData.horizontalAccuracy, Input.location.lastData.timestamp, Input.location.status);
		//Debug.LogWarning (Input.location.lastData.ToString());
		StartCoroutine("Init");
	//	Input.location.
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
        Input.location.Start();

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
			text.text = string.Format("lati: {0}\\long{1}\nalti: {2}\nacurracy:{3}\ntimestamp:{4}", Input.location.lastData.latitude, Input.location.lastData.longitude, Input.location.lastData.altitude, Input.location.lastData.horizontalAccuracy, Input.location.lastData.timestamp);
            //text.text = "Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp;
            // Access granted and location value could be retrieved
            Debug.Log("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);
        }

        // Stop service if there is no need to query location updates continuously
        Input.location.Stop();
    }
}
