using UnityEngine;
using System.Collections;
using OpenCVForUnitySample;
using System;

public class ColorItem : MonoBehaviour {

    public MultiObjectTrackingBasedOnColorSample sample;
    public string name;

	// Use this for initialization
	void Start () 
    {
	
	}
	
	// Update is called once per frame
	void Update () 
    {
        try
        {
            
            ColorObject colorObject = sample.MaxItems[name];
            Debug.Log(colorObject.ColorName);
            Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(colorObject.XPos, colorObject.YPos, 0));
            transform.position = new Vector3(pos.x, -pos.y-300, pos.z+20);
            transform.localScale = new Vector3((float)Math.Sqrt(colorObject.Area), (float)Math.Sqrt(colorObject.Area), 1);
            if(colorObject.Area > 0)
            {
                transform.Rotate(0, 0, 5);
            }
            
        }
        catch (Exception) { }
        
	}
}
