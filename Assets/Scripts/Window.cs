using UnityEngine;
using System.Collections;

public class Window : MonoBehaviour 
{
	public virtual void Close()
	{   
		gameObject.SetActive (false);
	}

	public virtual void Open()
	{
		gameObject.SetActive (true);
	}
}
