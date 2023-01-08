using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPadScript : MonoBehaviour
{
    [SerializeField] float jumpForce;

    OpenAIKartScript playerKart;
   
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.GetComponent<OpenAIKartScript>())
        {
            OpenAIKartScript player = other.gameObject.GetComponent<OpenAIKartScript>();
            playerKart = player;
            StartCoroutine(JumpPad());
        }
    }
     
    IEnumerator JumpPad()
    {
        playerKart.rb.AddForce(transform.up * jumpForce * Time.deltaTime);
        
        yield return new WaitForSeconds(0f);

    }

}
