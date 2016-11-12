using OpenCVForUnity;

[System.Serializable]
public class ColorObject
{
	public int XPos { get; set; }
    public int YPos { get; set; }
    public string ColorName { get; set; }
    public Scalar HSVmin { get; set; }
    public Scalar HSVmax { get; set; }
    public Scalar Color { get; set; }

	public double Area{ get; set;}

    public ColorObject()
    {
        ColorName = "Object";
        Color = new Scalar(0, 0, 0);
    }

	public ColorObject(string name, float min1, float min2, float min3, float max1, float max2, float max3, bool isDebugColor = false)
    {
		ColorName = name;
		switch (ColorName)
		{
		case "blue":

			if (isDebugColor) 
			{
				HSVmin = new Scalar(min1, min2, min3);
				HSVmax = new Scalar(max1, max2, max3);
			}
				
			HSVmin = new Scalar(90, 75, 100);
			HSVmax = new Scalar(130, 256, 245);
			Color = new Scalar(0, 0, 255);
			break;
		case "green":
			if (isDebugColor) 
			{
				HSVmin = new Scalar(min1, min2, min3);
				HSVmax = new Scalar(max1, max2, max3);
			}
			HSVmin = new Scalar(32, 33, 64);
			HSVmax = new Scalar(75, 256, 256);
			Color = new Scalar(0, 255, 0);
			break;
		case "yellow":
			if (isDebugColor) 
			{
				HSVmin = new Scalar(min1, min2, min3);
				HSVmax = new Scalar(max1, max2, max3);
			}
			HSVmin = new Scalar(16, 100, 145);
			HSVmax = new Scalar(35, 256, 256);
			Color = new Scalar(255, 255, 0);
            break;

        case "red":
			if (isDebugColor) 
			{
				HSVmin = new Scalar(min1, min2, min3);
				HSVmax = new Scalar(max1, max2, max3);
			}
            HSVmin = new Scalar(0, 113, 153);
            HSVmax = new Scalar(148, 191, 255);
            Color = new Scalar(255, 0, 0);
            break;
		default:
			break;
        }
    }
}
