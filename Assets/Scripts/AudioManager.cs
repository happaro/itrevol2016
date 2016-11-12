using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour {
    public AudioSource click;
    public AudioSource select;
    public static AudioManager Instance;

    public void Awake()
    {
        Instance = this;
    }

    public void Click()
    {
        if(click)
        {
            click.Play();
        }
    }
    public void Select()
    {
        if (select)
        {
            select.Play();
        }
    }
}
