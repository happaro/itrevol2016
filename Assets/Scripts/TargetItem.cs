using UnityEngine;
using System.Collections;

public class TargetItem : MonoBehaviour {
    string target;
    string[] names = { "red", "green", "yellow", "blue" };
    SpriteRenderer renderer;
    public Sprite red;
    public Sprite green;
    public Sprite yellow;
    public Sprite blue;
    public TextMesh label;
	// Use this for initialization
	void Start () {
        renderer = gameObject.GetComponent<SpriteRenderer>();
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
                renderer.sprite = red;
                break;
            case "green":
                renderer.sprite = green;
                break;
            case "yellow":
                renderer.sprite = yellow;
                break;
            case "blue":
                renderer.sprite = blue;
                break;
            default:
                break;
        }

    }
}
