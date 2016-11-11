using UnityEngine;

public class SaveManager
{
	public enum Key{BestScore, Coins, CurrentScore, IsTutorialPassed}

	//------INT
	static void SaveInt(string key, int value)
	{
		PlayerPrefs.SetInt(key, value);
	}

	static int LoadInt(string key)
	{
		if (!PlayerPrefs.HasKey(key))
			PlayerPrefs.SetInt(key, 0);
		return PlayerPrefs.GetInt(key);
	}

	//-------BOOL
	static bool LoadBool(string key)
	{
		return PlayerPrefs.GetInt(key) == 1;
	}

	static void SetBool(string key, bool value)
	{
		PlayerPrefs.SetInt(key, value ? 1 : 0);
	}
	//------

	public static int bestScore
	{
		get{return LoadInt (Key.BestScore.ToString());}
		set{SaveInt (Key.BestScore.ToString(), value);}
	}

	public static int currentScore
	{
		get{return LoadInt (Key.CurrentScore.ToString());}
		set{SaveInt (Key.CurrentScore.ToString(), value);}
	}

	public static int coinsCount
	{
		get{return PlayerPrefs.GetInt(Key.Coins.ToString());}
		set{PlayerPrefs.SetInt(Key.Coins.ToString(), value);}
	}

	public static bool isTutorialPassed
	{
		get{return LoadBool (Key.IsTutorialPassed.ToString ());}
		set{SetBool (Key.IsTutorialPassed.ToString (), value);}
	}
}