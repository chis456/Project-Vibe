using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


public class PlayerMovement : MonoBehaviour
{
    //public AudioClip activate;
    //public AudioClip runout;
    public AudioSource source;
    public AudioClip jumpSound;
    public AudioClip activate;
    public AudioClip off;
    public AudioClip death;
    public GameObject spawnPlatformArea;
    public GameObject PlayerEntity;
    public Rigidbody2D rb;
    public float maxSpeed = 12f;
    public float deacceleration = 5;
    public InputAction playerControls;
    public LayerMask dynamicEnvironment;
    public Animator animator;

    //Ground collider stuff 
    public Transform groundCheckCollider;
    const float groundCheckRadius = 0.5f;
    public LayerMask groundLayer;
    public bool isGrounded = false; //checks to see if player is grounded

    //jump
    private float jumpPower = 10f;
    //private float timer = 0f;

    private float moveInput;
    private bool isFacingRight = true;
    private float acceleration = 3f;
    private float decceleration = -3f; //ignore that theres two imma fix it later
    private float velPower = 1f;

    //private bool lowGravityMode = false; UNUSED
    public bool noGravityMode = false;
    public bool reverseGravityMode = false;

    //jetpacking stuff
    public bool isJetPacking = false;

    public float currentFuel;
    public float maxFuel = 100;

    //gravity mechanics
    public float reverseFuel;
    public float maxReverseFuel = 50;

    public float noFuel;
    public float maxNoFuel = 50;
    public InputAction jetpack;

    //fuel ui
    public FuelBar reverseFuelBar;
    public FuelBar noGravFuelBar;

    //respawn point
    private Vector2 respawnPoint;
    public GameObject fallDetector;
    public GameObject heightLimit;

    //stage
    private bool stage1 = true;
    private bool stage2 = false;
    private bool stage3 = false;

    GameObject[] dynamicObjects;
    // Start is called before the first frame update
    void Start()
    {
        currentFuel = maxFuel; //start off with maximum fuel
        reverseFuel = maxReverseFuel; 
        noFuel = maxNoFuel; //start with 0
        dynamicObjects = GameObject.FindGameObjectsWithTag("Dynamic"); //get all gameObjecst that have tag "dynamic" used later in gravity contorl functions
        for(int i = 0; i < dynamicObjects.Length; i++)
        {
            Debug.Log(dynamicObjects[i].name);
        }

        //jetpack fuel UI
        reverseFuelBar.setMaxFuel(maxReverseFuel);
        noGravFuelBar.setMaxFuel(maxNoFuel);

        //set respawn point to current point
        respawnPoint = rb.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        dynamicObjects = GameObject.FindGameObjectsWithTag("Dynamic");
        if(stage1)
        {
            noFuel = 0;
        }

        else if(stage2)
        {
            reverseFuel = 0;
        }
        /*
        if(stage1 == false && stage2 == true)
        {
            maxReverseFuel = 0;
            reverseFuel = 0;
            maxNoFuel = 1000;
            stage2 = false;
        }
        */
        moveInput = playerControls.ReadValue<Vector2>().x; 
        //Debug.Log("input:" + moveInput);
        GroundCheck();

        //turning off reverse Gravity in case of running out of "fuel" (callback context is not updated per frame so have to check "fuel" status manually here in update function)
        if(reverseGravityMode && reverseFuel > 0)
        {
            reverseFuel -= 150 * Time.deltaTime;
            
        }
        else if(reverseGravityMode && reverseFuel <= 0)
        {
            Vector3 upsideDown = new Vector3(0, 0, -180);
            Vector3 rightsideUp = new Vector3(0, 0, 0);
            reverseGravityMode = false;
            Debug.Log("reverse off");
            source.PlayOneShot(off);

            //flip dude back
            PlayerEntity.transform.eulerAngles = rightsideUp;
            for(int i = 0; i < dynamicObjects.Length; i++) //go thru all dynamic objects and set their gravity to 1
            {
                if(dynamicObjects[i].GetComponent<Rigidbody2D>() != null) //preventing errors
                {
                    Debug.Log(dynamicObjects[i].name);
                    dynamicObjects[i].GetComponent<Rigidbody2D>().gravityScale = 1f;
                }
            }
            rb.gravityScale = 1;
        }
        //turning off no Gravity in case of running out of "fuel"
        if(noGravityMode && noFuel > 0)
        {
            noFuel -= 150 * Time.deltaTime;
         
        }
        else if(noGravityMode && noFuel <= 0)
        {
            noGravityMode = false;
            Debug.Log("gravity on");
            source.PlayOneShot(off);

            for(int i = 0; i < dynamicObjects.Length; i++) //go thru all dynamic objects and set their gravity to 1
            {
                if(dynamicObjects[i].GetComponent<Rigidbody2D>() != null) //preventing errors
                {
                    Debug.Log(dynamicObjects[i].name);
                    dynamicObjects[i].GetComponent<Rigidbody2D>().gravityScale = 1;
                }
                
            }
            rb.gravityScale = 1;
        }
        reverseFuelBar.setFuel(reverseFuel);
        noGravFuelBar.setFuel(noFuel);

        fallDetector.transform.position = new Vector2(transform.position.x, fallDetector.transform.position.y); //fall detector will follow the player's x, not the y (constant)
        heightLimit.transform.position = new Vector2(transform.position.x, heightLimit.transform.position.y);

        //if(playerControls.ReadValue)
        //if(playerControls.ReadValue)

        //decrease jetpack fuel
        /* jetpack.performed += jetPack =>
         {
             currentFuel -= jetPack.duration; //not decreasing but why
         };*/
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "FallDetector")
        {
            source.PlayOneShot(death);
            //set things back to normal before respawning
            if (noGravityMode == true)
            {
                noGravityMode = false;
                Debug.Log("gravity on");

                for (int i = 0; i < dynamicObjects.Length; i++) //go thru all dynamic objects and set their gravity to 1
                {
                    if (dynamicObjects[i].GetComponent<Rigidbody2D>() != null) //preventing errors
                    {
                        Debug.Log(dynamicObjects[i].name);
                        dynamicObjects[i].GetComponent<Rigidbody2D>().gravityScale = 1;
                    }

                }

                rb.gravityScale = 1;
            }

            Vector3 rightsideUp = new Vector3(0, 0, 0);
            if (reverseGravityMode == true)
            {
                reverseGravityMode = false;
                Debug.Log("reverse off");
                //flip dude back
                PlayerEntity.transform.eulerAngles = rightsideUp;

                for (int i = 0; i < dynamicObjects.Length; i++) //go thru all dynamic objects and set their gravity to 1
                {
                    if (dynamicObjects[i].GetComponent<Rigidbody2D>() != null) //preventing errors
                    {
                        Debug.Log(dynamicObjects[i].name);
                        dynamicObjects[i].GetComponent<Rigidbody2D>().gravityScale = 1f;
                    }

                }
                rb.gravityScale = 1;
            }
            transform.position = respawnPoint;
            reverseFuel = maxReverseFuel; //refuel
            spawnPlatformArea.GetComponent<spawnPlatformSTG2>().ResetObject();

        }
        else if (collision.tag == "Checkpoint")
        {
            respawnPoint = transform.position; //set respawnpoint to new checkpoint
            Debug.Log("checkpoint set");
            RestoreFuel();
            dynamicObjects = GameObject.FindGameObjectsWithTag("Dynamic");
            for(int i = 0; i < dynamicObjects.Length; i++)
            {
                Debug.Log(dynamicObjects[i].name);
            }
        }

        else if (collision.tag == "Portal_Level1")
        {
            SceneManager.LoadScene(3); //loads level 2
        }

        else if(collision.tag == "stage2")
        {
            stage1 = false;
            stage2 = true;
        }

        else if(collision.tag == "spawnPlatform")
        {
            dynamicObjects = GameObject.FindGameObjectsWithTag("Dynamic");
        }

        else if(collision.tag == "stage3")
        {
            stage3 = true;
            stage2 = false;
        }

        else if(collision.tag == "playagain")
        {
            SceneManager.LoadScene("Base");
        }

        else if(collision.tag == "quit")
        {
            SceneManager.LoadScene("Menu");
        }
    }

    //needed for unitys *new* input system (Window -> Package Manager -> Select search for Packages in Unity Registry -> Search for Input System)
    private void OnEnable()
    {
        playerControls.Enable(); //idk what these do 

    }
    
    private void OnDisable()
    {
        playerControls.Disable();
        
    }

    public void RestoreFuel() //restores fuel upon being called
    {
        reverseFuel = maxReverseFuel;
        noFuel = maxNoFuel;
        Debug.Log("fuel restored");
    }

    void GroundCheck()
    {
        //check if groundCheck object is colliding with "Walkable"; if yes then true, if no then false
        isGrounded = false;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheckCollider.position, groundCheckRadius, groundLayer); //add whatever is contacted to array
        if(colliders.Length > 0)
        {
            isGrounded = true;
        }
        Collider2D[] collidersDynamic = Physics2D.OverlapCircleAll(groundCheckCollider.position, groundCheckRadius, dynamicEnvironment); //able to jump off dynamic
        if(collidersDynamic.Length > 0) {
            isGrounded = true;
        } 
    }

    public void Jump(InputAction.CallbackContext context) //context is command "space"
    {
        if(context.performed && isGrounded == true && reverseGravityMode == false && isJetPacking == false)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpPower); //jump
            source.PlayOneShot(jumpSound);
        }
        else if(context.performed && isGrounded == true && reverseGravityMode == true && isJetPacking == false)
        {
            rb.velocity = new Vector2(rb.velocity.x, -jumpPower); //jump downwards
            source.PlayOneShot(jumpSound);
        }
        if(context.canceled && rb.velocity.y > 0f && reverseGravityMode == false && isJetPacking == false) //higher jump depending on time held
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
            //AudioSource.PlayClipAtPoint(jumpSound, transform.position, 1);
        }
        else if(context.canceled && rb.velocity.y < 0f && reverseGravityMode == true && isJetPacking == false) //jump downwards more depending on time held
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
            //AudioSource.PlayClipAtPoint(jumpSound, transform.position, 1);
        }
    }

    public void activateLow(InputAction.CallbackContext context) //UNUSED
    {
        /*
        if (lowGravityMode == true && context.performed)
        {
            lowGravityMode = false;
            Debug.Log("low gravity off");
            for(int i = 0; i < dynamicObjects.Length; i++) //go thru all dynamic objects and set their gravity to 1
            {
                if(dynamicObjects[i].GetComponent<Rigidbody2D>() != null) //preventing errors
                {
                    Debug.Log(dynamicObjects[i].name);
                    dynamicObjects[i].GetComponent<Rigidbody2D>().gravityScale = 1f;
                }
                
            }
            rb.gravityScale = 1f;
        
        }

        //else if isGravity false, context performed, turn on gravity
        else if (lowGravityMode == false && noGravityMode == false && reverseGravityMode == false && context.performed && reverseFuel > 0)
        {
            lowGravityMode = true;
            Debug.Log("low gravity on");
            for(int i = 0; i < dynamicObjects.Length; i++) //go thru all dynamic objects and set their gravity to 1
            {
                if(dynamicObjects[i].GetComponent<Rigidbody2D>() != null) //preventing errors
                {
                    Debug.Log(dynamicObjects[i].name);
                    dynamicObjects[i].GetComponent<Rigidbody2D>().gravityScale = 0.5f;
                }
                
            }
            rb.gravityScale = 0.5f;
        }
        */
    }

    public void activateNo(InputAction.CallbackContext context)
    {
        //if noGravityMode is true (gravity off), context performed, turn on gravity
        if (noGravityMode == true && context.performed)
        {
            noGravityMode = false;
            source.PlayOneShot(off);
            Debug.Log("gravity on");

            for(int i = 0; i < dynamicObjects.Length; i++) //go thru all dynamic objects and set their gravity to 1
            {
                if(dynamicObjects[i].GetComponent<Rigidbody2D>() != null) //preventing errors
                {
                    Debug.Log(dynamicObjects[i].name);
                    dynamicObjects[i].GetComponent<Rigidbody2D>().gravityScale = 1;
                }
                
            }
            
            rb.gravityScale = 1;
        }

        //else if noGravityMode is false (gravity on), context performed, turn off gravity
        else if (noGravityMode == false && context.performed && reverseGravityMode == false)
        {
            noGravityMode = true;
            source.PlayOneShot(activate);
            Debug.Log("gravity off");
            
            for(int i = 0; i < dynamicObjects.Length; i++) //go thru all dynamic objects and set their gravity to 0
            {
                if(dynamicObjects[i].GetComponent<Rigidbody2D>() != null)
                {
                    Debug.Log(dynamicObjects[i].name);
                    dynamicObjects[i].GetComponent<Rigidbody2D>().gravityScale = 0;
                }
                
            }

            //float off
            rb.gravityScale = 0;
        }

        else if (noGravityMode == false && context.performed && reverseGravityMode == true)
        {
            reverseGravityMode = false;
            noGravityMode = true;
            source.PlayOneShot(activate);
            Debug.Log("gravity off");
            
            for(int i = 0; i < dynamicObjects.Length; i++) //go thru all dynamic objects and set their gravity to 0
            {
                if(dynamicObjects[i].GetComponent<Rigidbody2D>() != null)
                {
                    Debug.Log(dynamicObjects[i].name);
                    dynamicObjects[i].GetComponent<Rigidbody2D>().gravityScale = 0;
                }
                
            }

            //float off
            rb.gravityScale = 0;
        }
    }

    public void activateReverse(InputAction.CallbackContext context)
    {
        Vector3 upsideDown = new Vector3(0, 0, -180);
        Vector3 rightsideUp = new Vector3(0, 0, 0);
        if(reverseGravityMode == true && context.performed){
            reverseGravityMode = false;
            Debug.Log("reverse off");
            source.PlayOneShot(off);
            //flip dude back
            PlayerEntity.transform.eulerAngles = rightsideUp;

            for(int i = 0; i < dynamicObjects.Length; i++) //go thru all dynamic objects and set their gravity to 1
            {
                if(dynamicObjects[i].GetComponent<Rigidbody2D>() != null) //preventing errors
                {
                    Debug.Log(dynamicObjects[i].name);
                    dynamicObjects[i].GetComponent<Rigidbody2D>().gravityScale = 1f;
                }
                
            }
            rb.gravityScale = 1;
        }
        else if(reverseGravityMode == false && context.performed && noGravityMode == false)
        {
            reverseGravityMode = true;
            source.PlayOneShot(activate);
            Debug.Log("reverse on");

            //flip the dude
            PlayerEntity.transform.eulerAngles = upsideDown;

            for(int i = 0; i < dynamicObjects.Length; i++) //go thru all dynamic objects and set their gravity to -1
            {
                if(dynamicObjects[i].GetComponent<Rigidbody2D>() != null) //preventing errors
                {
                    Debug.Log(dynamicObjects[i].name);
                    dynamicObjects[i].GetComponent<Rigidbody2D>().gravityScale = -1;
                }
                
            }
            rb.gravityScale = -1;
        }

        else if(reverseGravityMode == false && context.performed && noGravityMode == true)
        {
            noGravityMode = false;
            reverseGravityMode = true;
            source.PlayOneShot(activate);
            Debug.Log("reverse on");

            //flip the dude
            PlayerEntity.transform.eulerAngles = upsideDown;

            for(int i = 0; i < dynamicObjects.Length; i++) //go thru all dynamic objects and set their gravity to -1
            {
                if(dynamicObjects[i].GetComponent<Rigidbody2D>() != null) //preventing errors
                {
                    Debug.Log(dynamicObjects[i].name);
                    dynamicObjects[i].GetComponent<Rigidbody2D>().gravityScale = -1;
                }
                
            }
            rb.gravityScale = -1;
        }


    }
    
    public void jetPack(InputAction.CallbackContext context)
    {
        /*

        float thrustForce = 30f; //how much thrust
        //hold down to use
        if (context.performed)
        {
            if(currentFuel > 0) //jetpacking not allowed if out of fuel
            {
                PlayerEntity.GetComponent<ConstantForce2D>().force = new Vector3(0, thrustForce, 0);
                isJetPacking = true;
            }
            else if (currentFuel == 0)
            {
                Debug.Log("out of fuel!");
            }
        }
        else if (context.canceled) //canceled once let go
        {
            PlayerEntity.GetComponent<ConstantForce2D>().force = new Vector3(0, 0, 0);
            isJetPacking = false; //wait until landed to declare no jetpacking
           
        }
        */
    }

    public void killPlayer()
    {
        rb.transform.position = respawnPoint; //reset them back to the last checkpoint
        Debug.Log("player died");
        
    }

    void FixedUpdate()
    {

        //ANIMATION SETTINGS


        if(!reverseGravityMode)
        {
            if(moveInput == 1)
            {   
                animator.SetBool("isMovingRight", true);
                animator.SetBool("isMovingLeft", false);
            }

            else if(moveInput == -1)
            {
                animator.SetBool("isMovingRight", false);
                animator.SetBool("isMovingLeft", true);
            }

            else
            {
                animator.SetBool("isMovingRight", false);
                animator.SetBool("isMovingLeft", false);
            }
        }
        else
        {
            if(moveInput == 1)
            {   
                animator.SetBool("isMovingRight", false);
                animator.SetBool("isMovingLeft", true);
            }

            else if(moveInput == -1)
            {
                animator.SetBool("isMovingRight", true);
                animator.SetBool("isMovingLeft", false);
            }

            else
            {
                animator.SetBool("isMovingRight", false);
                animator.SetBool("isMovingLeft", false);
            }
        }
        



        if(moveInput != 0) //copied this from a video
        {
            float targetSpeed = moveInput * maxSpeed;

            float velocityDifference = targetSpeed - rb.velocity.x;

            float accelerationRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : decceleration;

            float movement = Mathf.Pow(Mathf.Abs(velocityDifference) * accelerationRate, velPower) * Mathf.Sign(velocityDifference);

            if(isGrounded == false) 
            {
                rb.AddForce(0.1f * movement * Vector2.right); //deaccelerate side to side movement when in the air
                if(noGravityMode == false && reverseGravityMode == false)
                {
                    rb.AddForce(5f * Vector2.down); //make them fall quicker in air
                }
                if(reverseGravityMode == true)
                {
                    rb.AddForce(5f * Vector2.up);
                }
               
            }
            else
            {
                rb.AddForce(movement * Vector2.right);
            }
        }
        
        else //deccelerate player when not pressing keys
        {
            float currentVelocity = rb.velocity.x;
            //float newVelocity = 0f;
            if(currentVelocity < 0) //going left
            {
                rb.AddForce(deacceleration * -currentVelocity * Vector2.right);
            }
            else if(currentVelocity > 0) //going right
            {
                rb.AddForce(deacceleration * currentVelocity * Vector2.left);
            }
        }
        

        //Debug.Log(rb.velocity.x);
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }
}

