using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public GameObject PlayerEntity;
    public Rigidbody2D rb;
    public float maxSpeed = 12f;
    public float deacceleration = 5;
    public InputAction playerControls;
    public LayerMask dynamicEnvironment;

    //Ground collider stuff 
    public Transform groundCheckCollider;
    const float groundCheckRadius = 0.5f;
    public LayerMask groundLayer;
    public bool isGrounded = false; //checks to see if player is grounded

    //jump
    private float jumpPower = 10f;

    private float moveInput;
    private bool isFacingRight = true;
    private float acceleration = 3f;
    private float decceleration = -3f; //ignore that theres two imma fix it later
    private float velPower = 1f;

    private bool lowGravityMode = false;
    public bool noGravityMode = false;
    public bool reverseGravityMode = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        moveInput = playerControls.ReadValue<Vector2>().x; //basicall
        GroundCheck();
        //if(playerControls.ReadValue)
        //if(playerControls.ReadValue)
        
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

    void GroundCheck()
    {
        //check if groundCheck object is colliding with "Walkable"; if yes then true, if no then false
        isGrounded = false;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheckCollider.position, groundCheckRadius, groundLayer); //add whatever is contacted to array
        if(colliders.Length > 0)
        {
            isGrounded = true;
        }
    }

    public void Jump(InputAction.CallbackContext context) //context is command "space"
    {
        if(context.performed && isGrounded == true && reverseGravityMode == false)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpPower); //jump
        }
        else if(context.performed && isGrounded == true && reverseGravityMode == true)
        {
            rb.velocity = new Vector2(rb.velocity.x, -jumpPower); //jump downwards
        }
        if(context.canceled && rb.velocity.y > 0f && reverseGravityMode == false) //higher jump depending on time held
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }
        else if(context.canceled && rb.velocity.y > 0f && reverseGravityMode == true) //jump downwards more depending on time held
        {
            rb.velocity = new Vector2(rb.velocity.x, -rb.velocity.y * 0.5f);
        }
    }

    public void activateLow(InputAction.CallbackContext context)
    {
        if (lowGravityMode == true && context.performed)
        {
            lowGravityMode = false;
            rb.gravityScale = 1f;
            Debug.Log("low gravity off");
        }

        //else if isGravity false, context performed, turn on gravity
        else if (lowGravityMode == false && noGravityMode == false && reverseGravityMode == false && context.performed)
        {
            lowGravityMode = true;
            rb.gravityScale = 0.5f;
            Debug.Log("low gravity on");
        }
    }

    public void activateNo(InputAction.CallbackContext context)
    {
        //if noGravityMode is true (gravity off), context performed, turn on gravity
        if (noGravityMode == true && context.performed)
        {
            noGravityMode = false;
            Debug.Log("gravity on");
            //var allArray = FindObjectsOfType(gameObject);
            //GameObject[] objects = ; 
            //FindObjectsOfType(GameObject);
            //for(int i = 0; i < objects.Length; i++)
            //{
            //    Debug.Log(objects[i].name);
           // }
            //float on
            rb.gravityScale = 1;
        }

        //else if noGravityMode is false (gravity on), context performed, turn off gravity
        else if (noGravityMode == false && context.performed && lowGravityMode == false && reverseGravityMode == false)
        {
            noGravityMode = true;
            Debug.Log("gravity off");

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
            //flip dude back
            PlayerEntity.transform.eulerAngles = rightsideUp;
            rb.gravityScale = 1;
           

        }
        else if(reverseGravityMode == false && context.performed && noGravityMode == false && lowGravityMode == false)
        {
            reverseGravityMode = true;
            Debug.Log("reverse on");

            //flip the dude
            PlayerEntity.transform.eulerAngles = upsideDown;

            rb.gravityScale = -1;

            //check if touching a platform above
            if(isGrounded == true)
            {

            }
            //if true, then allow for jump downwards
        }


    }


    void FixedUpdate()
    {
        if(moveInput != 0) //copied this from a video
        {
            float targetSpeed = moveInput * maxSpeed;

            float velocityDifference = targetSpeed - rb.velocity.x;

            float accelerationRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : decceleration;

            float movement = Mathf.Pow(Mathf.Abs(velocityDifference) * accelerationRate, velPower) * Mathf.Sign(velocityDifference);

            if(isGrounded == false) 
            {
                rb.AddForce(0.1f * movement * Vector2.right); //deaccelerate side to side movement when in the air
                if(noGravityMode == false)
                {
                    rb.AddForce(5f * Vector2.down); //make them fall quicker in air
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
            float newVelocity = 0f;
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
