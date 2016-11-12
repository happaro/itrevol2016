using UnityEngine;
using System;

public class ColorItem : MonoBehaviour 
{
    public MainController sample;
    public string name;
	void Update () 
    {
        try
        {
            ColorObject colorObject = sample.MaxItems[name];
			if (sample.MaxItems[name] == null)
			{
				transform.position = Vector3.one * 1000;
				return;
			}
			float prop = (float) sample.webCamTexture.height / (float) Screen.height;
			int xMyScreen = Mathf.RoundToInt(((float)colorObject.XPos / (float)sample.webCamTexture.width) * (float)Screen.width);
			int yMyScreen = Mathf.RoundToInt(((float)colorObject.YPos / (float)sample.webCamTexture.height) * (float)Screen.height);
			Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(xMyScreen, yMyScreen, 0));
			transform.position = new Vector3(pos.x, -pos.y * prop, -5);
            transform.localScale = new Vector3((float)Math.Sqrt(colorObject.Area), (float)Math.Sqrt(colorObject.Area), 1);
            if(colorObject.Area > 0)
                transform.Rotate(0, 0, 5);
        }
        catch (Exception) { }
	}
}
