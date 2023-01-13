using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostPadHandler : MonoBehaviour
{

    [SerializeField] float boosterForce;
    [SerializeField] float boostTime = 2f;

    OpenAIKartScript playerKart;
   
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.GetComponent<OpenAIKartScript>())
        {
            OpenAIKartScript player = other.gameObject.GetComponent<OpenAIKartScript>();
            playerKart = player;
            StartCoroutine(BoosterForce());
        }
    }

        
    IEnumerator BoosterForce()
    {
        playerKart.rb.AddForce(transform.forward * boosterForce * Time.deltaTime);
        playerKart.rb.drag = .8f;

        playerKart.rightWheelDriftFX.Play();
        playerKart.leftWheelDriftFX.Play();

        yield return new WaitForSeconds(boostTime);
        
        playerKart.rb.drag = 1.5f;
        playerKart.rightWheelDriftFX.Stop();
        playerKart.leftWheelDriftFX.Stop();
        
    }
}
