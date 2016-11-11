using UnityEngine;
using System.Collections;

using OpenCVForUnity;

public class ColorObject
{

    public int XPos { get; set; }
    public int YPos { get; set; }
    public string ColorName { get; set; }
    public Scalar HSVmin { get; set; }
    public Scalar HSVmax { get; set; }
    public Scalar Color { get; set; }

    public ColorObject()
    {
        //set values for default constructor
        ColorName = "Object";
        Color = new Scalar(0, 0, 0);

    }

    public ColorObject(string name)
    {

        ColorName = name;

        if (name == "blue")
        {

            //TODO: use "calibration mode" to find HSV min
            //and HSV max values

            HSVmin = new Scalar(180f / 2, 0.15 * 256, 0.1 * 256);
            HSVmax = new Scalar(255f / 2, 256, 256);

            //BGR value for Green:
            Color = new Scalar(0, 0, 255);

        }
        if (name == "green")
        {

            //TODO: use "calibration mode" to find HSV min
            //and HSV max values

            HSVmin = new Scalar(64f / 2, 0.15 * 256, 0.15 * 256);
            HSVmax = new Scalar(150f / 2, 256, 256);

            //BGR value for Yellow:
            Color = new Scalar(0, 255, 0);

        }
        if (name == "yellow")
        {

            //TODO: use "calibration mode" to find HSV min
            //and HSV max values
            Debug.Log("yellow");
            HSVmin = new Scalar(15f / 2, 0.15 * 256, 0.75 * 256);
            HSVmax = new Scalar(64f / 2, 256, 256);

            //BGR value for Red:
            Color = new Scalar(255, 255, 0);

        }
        if (name == "red")
        {

            //TODO: use "calibration mode" to find HSV min
            //and HSV max values
            HSVmin = new Scalar(-10, 0.5 * 256, 0.1 * 256);
            HSVmax = new Scalar(15f / 2, 255, 255);
            //BGR value for Red:
            Color = new Scalar(255, 0, 0);

        }
    }
}
