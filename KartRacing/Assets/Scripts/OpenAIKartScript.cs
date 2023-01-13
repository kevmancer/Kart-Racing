using System.Net;
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
    public Transform GameObject;
    public float distToGround = 1f;
    public float driftForce = 10f;
    public float driftFriction = 0.9f;
    public AnimationCurve driftInputCurve;
    public float jumpForce = 30f;

    public float turnDirection;
    public int lapNumber;
    public int checkpointIndex;
    int driftDirection;

    public float pitchModifier;
    
    public ParticleSystem rightWheelDriftFX;
    public ParticleSystem leftWheelDriftFX;
    public ParticleSystem exhaustFX;

    public Rigidbody rb;
    public LayerMask ground;

    AudioSource kartSound;
    public AudioClip mainEngineSound;

    public bool isTurning;
    private bool isGrounded;
    public bool isDrifting;
    public bool isDriftingLeft;
    public bool isReversing;

    float initialSpeed;
    float initialTurnSpeed;

    public Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        initialSpeed = speed;
        initialTurnSpeed = turnSpeed;

        anim.speed = 2;

        lapNumber = 1;
        checkpointIndex = 0;

        rightWheelDriftFX.Stop();
        leftWheelDriftFX.Stop();

        kartSound = GetComponent<AudioSource>();
    }

    void Update() 
    {
        getVerticalInput = Input.GetAxis("Vertical");
        getHorizontalInput = Input.GetAxis("Horizontal");

        IsGrounded();
        TurnAnimation();
        ChangeEnginePitch();
        TurnCache();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
        GroundNormalHandler();
        
        ResetInertiaTenors();

        UpdateDirection();

        DriftController();

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
            // rb.AddForce(Vector3.down * acceleration);

            // Limit the speed of the rigid body
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, speed);

            //Play Forward Wheel animation
            anim.SetBool("isDriving", true);

        }
        else if (forwardInput < 0 && rb.velocity.magnitude < reverseSpeed)
        {
            // Move Backward
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
        //Initializes turnInput with Input from horizontal input
        float turnInput = getHorizontalInput;
        float currentRotation = GameObject.eulerAngles.z;

        //Rotates Rigid body when the speed is greater than 0
        if(getVerticalInput != 0 || rb.velocity.magnitude > 1f)
        {
          
            transform.Rotate(0, turnInput * turnSpeed * Time.smoothDeltaTime, 0); 
    
        } 

        //Sets isTurning bool to true when horizontal input is applied
        if(turnInput > 0 || turnInput < 0)
        {
            isTurning = true;
        } else{
            isTurning = false;
        }
        
    }

    //Animates the front wheels of the kart  to turn in direction the kart is rotating 
    //based on the Horizontal Input
    void TurnAnimation()
    {
        if(getVerticalInput < 0)
        {
            isReversing = true;
        } 
        else
        {
            isReversing = false;
        }

        if(isReversing == false && getHorizontalInput > 0)
        {
            anim.SetFloat("SteeringAxis", 1);
        } 
        else if(isReversing == false && getHorizontalInput < 0)
        {
            anim.SetFloat("SteeringAxis", -1);
        }
        else if(isReversing == true && getHorizontalInput > 0)
        {
            anim.SetFloat("SteeringAxis", -1);
        }
        else if(isReversing == true && getHorizontalInput < 0)
        {
            anim.SetFloat("SteeringAxis", 1);
        }
        else 
        {
            anim.SetFloat("SteeringAxis", 0);
        }
    }

    void ChangeEnginePitch()
    {
        float rbSpeed = rb.velocity.magnitude;
        float soundPitch = 1f;

        // if(rbSpeed > 10f)
        // {
        //     soundPitch = 1.5f;
        // }

        if(rbSpeed > 15f)
        {
            soundPitch = 1.2f;
        }

        kartSound.pitch = (rbSpeed  / soundPitch) * pitchModifier + 0.6f;

    }

    //Resets Interia Tensor to help with stuttering and Jank movement with physics
    void ResetInertiaTenors()
    {
        rb.inertiaTensor = Vector3.zero;
        rb.inertiaTensorRotation = Quaternion.identity;
    }

    //Sets the gravity of the world
    //Might cause some bugs later on in development FYI
    void GravityMod()
    {
        Physics.gravity = new Vector3(0, -50, 0);
    }

    //Uses a raycast to detect the normal of the ground and rotates the Kart mesh to match the Ground normal
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
    }

    //Drifts the Kart when Spacebar or other input is pressed. 
    //Drift is applied by adding a sideways force to the rigidbody.
    // //Make a variable for drag to make it more usable 
    private void DriftController()
    {
        float drift = getHorizontalInput;

        // Change the input value to be between -1 and 1
        drift = Mathf.Clamp(drift, -1f, 1f);

        // DriftDirection(drift);

        // Remap the input value to be between 0 and 1
        if(turnDirection > 0)
        {
            drift = (drift + 1f) * 0.5f;
        }
        // drift = (drift + 1f) * 0.5f;

        // Apply input curve to adjust drift sensitivity
        drift = driftInputCurve.Evaluate(drift);

        // Remap the output value to be between -1 and 1
        if(turnDirection < 0)
        {
            drift = drift * 2f - 1f;
        }
        // drift = drift * 2f - 1f;
        
        if(isGrounded == true && getVerticalInput != 0 || rb.velocity.magnitude > 1f)
        {
            if(Input.GetKey(KeyCode.Space))
            {
        
            // rb.AddForce(rb.transform.TransformDirection(-turnInput * driftForce * Time.deltaTime, 0 ,0));
           
            // rb.AddForce(transform.right * turnInput * driftForce * Time.deltaTime);
            
            Vector3 driftForceVector = transform.right * -drift * driftForce * Time.deltaTime;
            rb.AddForce(driftForceVector);
            
            Vector3 oppositeForce = -rb.velocity * driftFriction;
            rb.AddForce(oppositeForce);
            // rb.drag = .8f;
            isDrifting = true;

            rightWheelDriftFX.Play();
            leftWheelDriftFX.Play();

            } 
            else if(isDrifting)
            {
                // rb.drag = 1.5f;
                isDrifting = false;
                rightWheelDriftFX.Stop();
                leftWheelDriftFX.Stop();
            }
        } 
    }

    void TurnCache()
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                turnDirection = getHorizontalInput;
            }
        }
       
    
}
