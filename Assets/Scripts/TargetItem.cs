using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TargetItem : MonoBehaviour 
{
    string target;
    string[] names = { "red", "green", "yellow", "blue" };
    public Sprite red;
    public Sprite green;
    public Sprite yellow;
    public Sprite blue;
	public Text label;

	private Image img;

	void Start () 
	{
		img = gameObject.GetComponent<Image>();
        NewTarget();
	}
	   
    public void NewTarget()
    {
        System.Random rand = new System.Random();
        target = names[rand.Next(4)];
        label.text = target;
        switch (target)
        {
            case "red":
				img.sprite = red;
                break;
            case "green":
				img.sprite = green;
                break;
            case "yellow":
				img.sprite = yellow;
                break;
            case "blue":
				img.sprite = blue;
                break;
            default:
                break;
        }

    }
}
