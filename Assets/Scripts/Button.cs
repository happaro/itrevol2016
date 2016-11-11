using UnityEngine;
using System.Collections;

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
		Menu
	}
	public ActionType action;
	public void SetActive(bool active, bool collider = false)
	{
		GetComponent<SpriteRenderer> ().sprite = active ? enableSprite : disableSprite;
		this.GetComponent<Collider2D> ().enabled = active ? true : collider;
		isActive = active;
	}


	float timer = 0, delay = 0.1f;
	void OnMouseOver()
	{
		if (isPressed && pushingAction && isActive) 
		{
			timer += Time.deltaTime;
			if (timer > delay)
			{
				myAction ();
				timer = 0;
			}
		}
		else
			timer = 0;
	}

	void Awake()
	{
		startScale = transform.localScale;
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
		if (action == ActionType.Menu)
			Application.LoadLevel (1);
		//myAction ();
	}
}
