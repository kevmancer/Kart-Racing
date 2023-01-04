using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointScript : MonoBehaviour
{
    public int index;

    void OnTriggerEnter(Collider collision)
    {
        if(collision.gameObject.GetComponent<OpenAIKartScript>())
        {
            OpenAIKartScript player = collision.gameObject.GetComponent<OpenAIKartScript>();

            if(player.checkpointIndex == index -1)
            {
                player.checkpointIndex = index;
            }
        }
    }
}
