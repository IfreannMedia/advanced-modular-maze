using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : MonoBehaviour
{
    public Vector3 destination;
    public AudioSource teleSound;

    void OnTriggerEnter(Collider col)
    {
        //ensure the player character is tagged in the Inspector as "Player"
        if (col.gameObject.tag == "Player")
        {
            col.gameObject.transform.position = destination + new Vector3(0,1,0);
            teleSound.Play();
        }
    }
}
