using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenAIKartScript : MonoBehaviour
{
    float getVerticalInput;
    float getHorizontalInput;

    public float acceleration = 10.0f;
    public float speed = 10.0f;
    public float reverseSpeed = 10f;
    public float turnSpeed = 10.0f;
    public float distToGround = 1f;
    public float driftForce = 10f;
    public Rigidbody rb;
    public LayerMask ground;

    public bool isTurning;
    private bool isGrounded;
    private bool isDrifting;

    public Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update() 
    {
        getVerticalInput = Input.GetAxis("Vertical");
        getHorizontalInput = Input.GetAxis("Horizontal");

        IsGrounded();
        DriftController();
        TurnAnimation();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
        GroundNormalHandler();
        
        ResetInertiaTenors();

        UpdateDirection();

        if(isGrounded == true)
        {
            TurnHandler();
            Physics.gravity = new Vector3(0, -10, 0);
        } else if(isGrounded == false)
        {
            Invoke("GravityMod", .3f);
        }


    }

    void UpdateDirection()
    {
        float forwardInput = getVerticalInput;

        if (forwardInput > 0)
        {
            // Move forward
            rb.AddForce(transform.forward * acceleration);

            // Limit the speed of the rigid body
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, speed);

            //Play Forward Wheel animation
            anim.SetBool("isDriving", true);
        }
        else if (forwardInput < 0 && rb.velocity.magnitude < reverseSpeed)
        {
            // Move forward
            rb.AddForce(transform.forward * acceleration * -1);

            // Limit the speed of the rigid body
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, speed);
        } else
        {
            anim.SetBool("isDriving", false);
        }

    }

    void TurnHandler()
    {
        float turnInput = getHorizontalInput;

        if(rb.velocity.magnitude > 0)
        {
            transform.Rotate(0, turnInput * turnSpeed * Time.smoothDeltaTime, 0); 
        } 

        if(turnInput > 0 || turnInput < 0)
        {
            isTurning = true;
        } else{
            isTurning = false;
        }
        
    }

    void TurnAnimation()
    {
        if(getHorizontalInput > 0)
        {
            anim.SetFloat("SteeringAxis", 1);
        } 
        else if(getHorizontalInput < 0)
        {
            anim.SetFloat("SteeringAxis", -1);
        }
        else
        {
            anim.SetFloat("SteeringAxis", 0);
        }
    }

    void ResetInertiaTenors()
    {
        rb.inertiaTensor = Vector3.zero;
        rb.inertiaTensorRotation = Quaternion.identity;
    }

    void GravityMod()
    {
        Physics.gravity = new Vector3(0, -50, 0);
    }

    void GroundNormalHandler()
    {
    RaycastHit hit;
    Physics.Raycast(transform.position, -transform.up, out hit, 1, ground);
    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation, 0.1f);
    }

    //Checks ground and returns bool if grounded or not
    void IsGrounded()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, distToGround + 0.1f);
        Debug.Log(isGrounded);
    }

    void DriftController()
    {
        float turnInput = getHorizontalInput;

        if(isTurning == true && rb.velocity.magnitude > 20f && isGrounded == true)
        {
             if(Input.GetKey(KeyCode.Space))
            {
            rb.AddForce(rb.transform.TransformDirection(-turnInput * driftForce * Time.deltaTime, 0 ,0));
            rb.drag = .8f;
            isDrifting = true;
            } else{
                rb.drag = 1.5f;
                isDrifting = false;
            }
        }
       
    }


}
