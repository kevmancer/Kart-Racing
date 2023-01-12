using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LapManager : MonoBehaviour
{
    public List<CheckpointScript> checkpoints;
    public int totalLaps;

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.GetComponent<OpenAIKartScript>())
        {
            OpenAIKartScript player = other.gameObject.GetComponent<OpenAIKartScript>();

            if(player.checkpointIndex == checkpoints.Count)
            {
                player.checkpointIndex =0;
                player.lapNumber++;

                if(player.lapNumber > totalLaps)
                {
                    Debug.Log("You won");
                }
            }
        }
    }
}
