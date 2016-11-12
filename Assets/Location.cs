using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Location : MonoBehaviour {
    public Text text;
    void Start()
    {
        StartCoroutine("Init");
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
            text.text = "Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp;
            // Access granted and location value could be retrieved
            Debug.Log("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);
        }

        // Stop service if there is no need to query location updates continuously
        Input.location.Stop();
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
