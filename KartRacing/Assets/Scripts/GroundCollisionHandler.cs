using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCollisionHandler : MonoBehaviour
{

    public OpenAIKartScript kartSpeed;
    public float offRoadSpeed;
    public float currentSpeed;

    void Start()
    {
        currentSpeed = kartSpeed.speed;
    }   
    
    private void OnCollisionEnter(Collision other)
    {
        switch(other.gameObject.tag)
        {
            case "OffRoad":
                Debug.Log("Off road");
                kartSpeed.speed = kartSpeed.speed - offRoadSpeed;
                break;
            case "Track":
                Debug.Log("Track");
                kartSpeed.speed = currentSpeed;
                break;
        }
    }
}
