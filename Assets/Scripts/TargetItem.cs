using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TargetItem : MonoBehaviour 
{
    public string target = "yellow";
    string[] names = { "red", "green", "yellow", "blue" };
    public Sprite red;
    public Sprite green;
    public Sprite yellow;
    public Sprite blue;
	public Text label;

	private Image img;
	private System.Random rand;
	void Awake () 
	{
		img = gameObject.GetComponent<Image>();
        NewTarget();
		rand = new System.Random ();
	}
	   
    public void NewTarget()
    {
		string newString = target;

		while (newString == target) 
		{
			target = names[Random.Range(0, names.Length)];
		}
		GameObject.FindObjectOfType<MainController> ().AddPoint ();
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
