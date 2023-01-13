using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LapManager : MonoBehaviour
{
    public List<CheckpointScript> checkpoints;
    public int totalLaps;

    public TextMeshProUGUI counterText;

    public TextMeshProUGUI totalNumberLaps;

    public GameObject player;

    void Start()
    {
        counterText.text = "1";
        totalNumberLaps.text = "/" + " " + totalLaps;
    }


    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.GetComponent<OpenAIKartScript>())
        {
            OpenAIKartScript player = other.gameObject.GetComponent<OpenAIKartScript>();

            if(player.checkpointIndex == checkpoints.Count)
            {
                player.checkpointIndex =0;
                player.lapNumber++;

                counterText.text = player.lapNumber.ToString();

                if(player.lapNumber > totalLaps)
                {
                    Debug.Log("You won");
                }
            }
        }
    }

}
