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

    public ColorObject()
    {
        ColorName = "Object";
        Color = new Scalar(0, 0, 0);
    }

	public ColorObject(string name, float min1, float min2, float min3, float max1, float max2, float max3)
    {
		ColorName = name;
		switch (ColorName)
		{
		case "blue":
			HSVmin = new Scalar(min1 / 2, 0.5 * min2, 0.2 * min3);
			HSVmax = new Scalar(max1 / 2, max2, max3);
			Color = new Scalar(0, 0, 255);
			break;
		case "green":
			HSVmin = new Scalar(64f / 2, 0.5 * 256, 0.2 * 256);
			HSVmax = new Scalar(150f / 2, 256, 256);
			Color = new Scalar(0, 255, 0);
			break;
		
			//great
		case "yellow":
			HSVmin = new Scalar(16, 100, 145);
			HSVmax = new Scalar(35, 256, 256);
			Color = new Scalar(255, 255, 0);
            break;

        case "red":
            HSVmin = new Scalar(-10, 0.4 * 256, 0.2 * 256);
            HSVmax = new Scalar(20f / 2, 255, 255);
            Color = new Scalar(255, 0, 0);
            break;
        }
    }
}
