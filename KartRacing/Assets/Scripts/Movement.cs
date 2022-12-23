using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] float movementSpeed = 100f;
    [SerializeField] float turnSpeed = 20f;
    [SerializeField] float accleration = 10f;

    private float currentSpeed;
    private float forwardAmount;
    private float turnAmount;
    
    public CharacterController controller;
    public Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb.transform.parent = null;
    }

    void Update()
    {
        transform.position = rb.transform.position;

        forwardAmount = Input.GetAxis("Vertical");
        turnAmount = Input.GetAxis("Horizontal");

          if(forwardAmount > 0)
        {
            DriveForward();
        } else if(forwardAmount < 0)
        {
            DriveReverse();
        }

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
        TurnHandler();
    }

    void TurnHandler()
    {
        
        float newRotation = turnAmount * turnSpeed * Time.deltaTime;
        transform.Rotate(0, newRotation , 0, Space.World);
        
    }
    void DriveForward()
    {
       rb.AddForce(Vector3.forward * accleration);
       rb.velocity = Vector3.ClampMagnitude(rb.velocity, movementSpeed);
    }

    void DriveReverse()
    {
        rb.AddForce(Vector3.back * movementSpeed * Time.deltaTime);
    }


    void OnCollisionEnter(Collision other)
    {
        float xRotation;

        if(other.gameObject.tag == "Ground")
        {
            xRotation = GetComponent<Transform>().position.x;
            Vector3 getRotation = transform.forward * xRotation;
            rb.transform.Rotate(-getRotation);
            Debug.Log("Ground" + xRotation);
        }
    }
}
