using UnityEngine;
using System.Collections;


public class RotateScript : MonoBehaviour 
{
	void FixedUpdate () 
	{
		transform.Rotate (0, 0, 1);
	}
}
