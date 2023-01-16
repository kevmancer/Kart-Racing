using System.Net;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenAIKartScript : MonoBehaviour
{
    float getVerticalInput;
    float getHorizontalInput;

    float driftTimer = 0f;
    public float driftBuildUpTimer;
    public float miniBoostForce;
    public float miniBoostTimer;

    public float acceleration = 10.0f;
    public float speed = 10.0f;
    public float reverseSpeed = 10f;
    public float turnSpeed = 10.0f;
    public Transform GameObject;
    public float distToGround = 1f;
    public float driftForce = 10f;
    public float driftFriction = 0.9f;
    public AnimationCurve driftInputCurve;
    public float turnDirection;
    public int lapNumber;
    public int checkpointIndex;

    public float pitchModifier;
    
    public ParticleSystem rightWheelDriftFX; //Regular Drift FX
    public ParticleSystem leftWheelDriftFX; //Regular Drift FX
    public ParticleSystem rightWheelDriftBoostFX; //Boost FX
    public ParticleSystem leftWheelDriftBoostFX;  //Boost FX
    public ParticleSystem rw_DriftBoostChargedFX; //Boost is charged FX
    public ParticleSystem lw_DriftBoostChargedFX; //Boost is charged FX
    public ParticleSystem exhaustFX;

    public Rigidbody rb;
    public LayerMask ground;

    AudioSource kartSound;
    public AudioClip mainEngineSound;

    public bool isTurning;
    private bool isGrounded;
    public bool isDrifting;
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
        rightWheelDriftBoostFX.Stop();
        leftWheelDriftBoostFX.Stop();
        rw_DriftBoostChargedFX.Stop();
        lw_DriftBoostChargedFX.Stop();

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
        DriftBuildUpTimer();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
        GroundNormalHandler();
        
        ResetInertiaTenors();

        UpdateDirection();

        DriftController();

        ApplyDriftMiniBoost();

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
    
    private void DriftController()
    {
        float turnInput = getHorizontalInput;

        // Change the input value to be between -1 and 1
        turnInput = Mathf.Clamp(turnInput, -1f, 1f);

        // Remap the input value to be between 0 and 1
        if(turnDirection > 0)
        {
            turnInput = (turnInput + 1f) * 0.5f;
        }
        
        // Apply input curve to adjust drift sensitivity
        turnInput = driftInputCurve.Evaluate(turnInput);

        // Remap the output value to be between -1 and 1
        if(turnDirection < 0)
        {
            turnInput = turnInput * 2f - 1f;
        }
        
        
        if(isGrounded == true && getVerticalInput != 0 || rb.velocity.magnitude > 1f)
        {
            if(Input.GetKey(KeyCode.Space))
            {
            
            //Apply drift force to the Kart
            Vector3 driftForceVector = transform.right * -turnInput * driftForce * Time.deltaTime;
            rb.AddForce(driftForceVector);
            
            //Apply Opposing Force with drift friction stop stop the Kart from going faster when drifting
            Vector3 oppositeForce = -rb.velocity * driftFriction;
            rb.AddForce(oppositeForce);
            isDrifting = true;

            rightWheelDriftFX.Play();
            leftWheelDriftFX.Play();

            } 
            else
            {
                isDrifting = false;
                rightWheelDriftFX.Stop();
                leftWheelDriftFX.Stop();
            }
        } 
    }

    // If either turn key is held down for a certain amount of time
    //A mini boost is activated.
    //When player exits drifting the mini boost is applied for x amount of seconds. 
    void DriftBuildUpTimer()
    {
        if(isDrifting)
        {
            if(isTurning)
            {
                driftTimer += Time.deltaTime;
            }
            
        } else if(!isDrifting)
        {
            driftTimer = 0f;
        }

        Debug.Log(driftTimer);
    }

    //if driftTimer is greater than driftBuildUpTimer than the wheel effects should change
    //
    void ApplyDriftMiniBoost()
    {
        if(driftTimer >= driftBuildUpTimer)
        {
            rightWheelDriftFX.Stop();
            leftWheelDriftFX.Stop();

            lw_DriftBoostChargedFX.Play();
            rw_DriftBoostChargedFX.Play();

            if(!isDrifting)
            {
                lw_DriftBoostChargedFX.Stop();
                rw_DriftBoostChargedFX.Stop();
                StartCoroutine(DriftMiniBoost());
            }
        }
        // if(driftTimer >= driftBuildUpTimer && !isDrifting)
        // {
        //     StartCoroutine(DriftMiniBoost());
        // }
    }

      IEnumerator DriftMiniBoost()
    {
        rb.AddForce(transform.forward * miniBoostForce * Time.deltaTime);
        rb.drag = .8f;

        rightWheelDriftBoostFX.Play();
        leftWheelDriftBoostFX.Play();

        yield return new WaitForSeconds(miniBoostTimer);
        
        rb.drag = 1.5f;
        rightWheelDriftBoostFX.Stop();
        leftWheelDriftBoostFX.Stop();
        
    }

//Determines which direction the player is turning and holds the value when not drifting
    void TurnCache()
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                turnDirection = getHorizontalInput;
            }
        }
       
    
}
