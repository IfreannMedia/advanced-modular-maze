using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    public AudioSource shot;


    public void Fire()
    {
        shot.Play();
    }

}
