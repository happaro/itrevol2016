using UnityEngine;
using System.Collections;

public class WindowInfo : Window 
{
	public UnityEngine.UI.Button okButton;
	public TextMesh title;
	public TextMesh info;

	public void Open(string title, string newText)
	{
        base.Open();
		info.text = newText;
	}

	public void Open(string titleText, string infoText, Button.MyAction closeActionPlus)
	{
        base.Open();
		title.text = titleText;
		info.text = infoText;
	}
}
