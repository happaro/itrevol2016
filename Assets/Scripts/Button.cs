﻿using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
//using System
public class Button : MonoBehaviour 
{
	private bool isPressed;

	public delegate void MyAction();
	public MyAction myAction;

	private Vector3 startScale;
	public Sprite enableSprite, disableSprite;

	public bool pushingAction = false;
	private bool isActive = true;

	public enum ActionType
	{
		Menu,
		Play,
		Scan,
		SelectColor
	}
	public ActionType actionType;


	void Awake()
	{
		startScale = transform.localScale;
	}

	public void SetActive(bool active, bool collider = false)
	{
		//GetComponent<image> ().sprite = active ? enableSprite : disableSprite;
		this.GetComponent<Collider2D> ().enabled = active ? true : collider;
		isActive = active;
	}
	

	void DownAction()
	{
		transform.localScale = startScale * 0.9f;
	}

	void UpAction()
	{
		isPressed = false;
		transform.localScale = startScale;
	}

	void OnMouseDown()
	{
		DownAction ();
		isPressed = true;
	}
		
	void OnMouseExit()
	{
		UpAction ();
		isPressed = false;	
	}

	void OnMouseUp()
	{
		if (isPressed) 
		{
			UpAction ();
			Action ();
		}
	}

	protected virtual void Action()
	{
		if (myAction == null) 
		{
			switch (actionType) 
			{
			case ActionType.SelectColor:
				var target = GameObject.FindObjectOfType<TargetItem> ();
				target.NewTarget ();
				GameObject.FindObjectOfType<MainController> ().ResetBitches();
				GameObject.FindObjectOfType<AudioManager> ().Click ();
				 break;
			case ActionType.Scan:
				GameObject.FindObjectOfType<MainController> ().Scan();
				break;
			case ActionType.Menu:
				SceneManager.LoadScene (0);
				break;
			case ActionType.Play:
				SceneManager.LoadScene (1);GameObject.FindObjectOfType<AudioManager> ().Click ();
				GameObject.FindObjectOfType<AudioManager> ().Click ();
				break;
			}
		}

	}
}
