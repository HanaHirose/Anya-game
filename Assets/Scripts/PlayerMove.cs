using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    private PlayerInput playerInput;
    private Rigidbody rb;
    private PlayerInputAction playerInputAction;
    private Animator anim;
    [SerializeField] private Transform cam;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask ground;

    public float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;
    private Vector3 direction;
    private int jumpCounter = 0;
    private int jumpCounterN = 8;
    private bool jumpAnim = false;
    private int landingCounter = 0;
    private int landingCounterN = 8;
    private bool preIsGrounded;

    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 5f;

    private enum MovementState  {ilde, running, jumpingUp, falling, landing }
    //private MovementState state = MovementState.ilde;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerInputAction = new PlayerInputAction();
        playerInputAction.Player.Enable();
        //playerInputAction.Player.Movement.performed += Movement_performed;
        playerInputAction.Player.Jump.performed += Jump_performed;
        

        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

    }

    private void Update()
    {
        Debug.Log(IsGrounded());

       //jumpAnim is true when jump animation is supposed to be true.
        if(!IsGrounded() && rb.velocity.y > 0.1f)
        {
            jumpAnim = true;
        }
        //From when jump button is pressed until actual physical jumping action starts
        else if (jumpCounter > .1f)
        {
            jumpAnim = true;
        }
        else
        {
            jumpAnim = false;
        }

        

        // When previously player was in the air and lands in this frame
        if( !preIsGrounded && IsGrounded())
        {
            landingCounter = landingCounterN;
        }

        

        //Remember the bool of IsGrounded for the next update frame.
        preIsGrounded = IsGrounded();

        UpdateAnimationState();
    }

    private void FixedUpdate()
    {
        Vector2 inputVector = playerInputAction.Player.Movement.ReadValue<Vector2>();

        direction = new Vector3(inputVector.x, 0f, inputVector.y).normalized;

        if (direction.magnitude >= 0.1f)
        {
            
            //This is for movement without taking the camera rotation into account.
            //rb.velocity = new Vector3(inputVector.x * speed, rb.velocity.y, inputVector.y * speed);

            //Taking the camera rotaiton into account
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;

            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            //Use this moveDir to determine which direction to move instead of using targetAngle.
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            //Movement with taking the camera rotation into account.
            rb.velocity = new Vector3(moveDir.x * speed, rb.velocity.y, moveDir.z * speed);
        }
        else
        {
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
        }

        //Physical jumping action will start when jumpCounter equal 1. 
        if (jumpCounter > .1f)
        {
            jumpCounter -= 1;
        }
        if (jumpCounter == 1)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
        }

        //While landing
        if(landingCounter > .1f)
        {
            landingCounter -= 1;
        }
       

    }

    private void UpdateAnimationState()
    {
        MovementState state;
        if (!jumpAnim && IsGrounded() && landingCounter <= .1f && direction.magnitude >= 0.1f)
        {
            state = MovementState.running;
        }
        else if (jumpAnim)
        {
            state = MovementState.jumpingUp;
        }
        else if (!IsGrounded() && rb.velocity.y < .1f)
        {
            state = MovementState.falling;
        }
        else if(landingCounter > .1f)
        {
            state = MovementState.landing;
        }
        else
        {
            state = MovementState.ilde;
        }
        
        anim.SetInteger("state", (int)state);
        Debug.Log("state: " + state);
    }

  

    private void Jump_performed(InputAction.CallbackContext context)
    {
        if (IsGrounded())
        {
            //rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
            jumpCounter = jumpCounterN;
            jumpAnim = true;
        }

    }

    private bool IsGrounded()
    {
        return Physics.CheckSphere(groundCheck.position, .3f, ground);
    }


    //private void Movement_performed(InputAction.CallbackContext context)
    //{
    //    Debug.Log(context);
    //    //Vector2 inputVector = context.ReadValue<Vector2>();
    //    //float speed = 500f;
    //    //rb.AddForce(new Vector3(inputVector.x, 0, inputVector.y) * speed, ForceMode.Force);
    //    Vector3 direction = new Vector3(inputVector.x, 0f, inputVector.y).normalized;

    //    if(direction.magnitude >= 0.1f)
    //    {
    //        rb.velocity = new Vector3(inputVector.x * speed, rb.velocity.y, inputVector.y * speed);
    //        float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
    //        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
    //        transform.rotation = Quaternion.Euler(0f, angle, 0f);
    //    }

    //}
}
