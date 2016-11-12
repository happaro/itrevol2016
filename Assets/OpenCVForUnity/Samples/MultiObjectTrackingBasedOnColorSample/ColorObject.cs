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
        ColorName = "Object";
        Color = new Scalar(0, 0, 0);
    }

    public ColorObject(string name)
    {
        ColorName = name;
        switch (ColorName)
        {
            case "blue":
                HSVmin = new Scalar(180f / 2, 0.15 * 256, 0.1 * 256);
                HSVmax = new Scalar(255f / 2, 256, 256);
                Color = new Scalar(0, 0, 255);
                break;
            case "green":
                HSVmin = new Scalar(64f / 2, 0.15 * 256, 0.15 * 256);
                HSVmax = new Scalar(150f / 2, 256, 256);
                Color = new Scalar(0, 255, 0);
                break;
            case "yellow":
                HSVmin = new Scalar(15f / 2, 0.15 * 256, 0.75 * 256);
                HSVmax = new Scalar(64f / 2, 256, 256);
                Color = new Scalar(255, 255, 0);
                break;
            case "red":
                HSVmin = new Scalar(-10, 0.5 * 256, 0.1 * 256);
                HSVmax = new Scalar(15f / 2, 255, 255);
                Color = new Scalar(255, 0, 0);
                break;
        }
    }
}
