using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Windows;

public class ThirdPersonController : MonoBehaviour
{
    // public variables for basic movement
    public CharacterController controller;
    public float speed = 20f;
    public float turnSmoothTime = .1f;
    public Transform player;
    public Transform cam;
    public float jumpForce = 20f;
    public float gravityScale = 4.9f;
    public Animator anim;

    // private variables for basic movement
    private Vector3 velocity;
    private Vector3 direction;
    private Vector3 moveDir;
    private float turnSmoothVelocity;
    private float targetAngle;
    private float horizontal;
    private float vertical;
    private float vectorAngle;
    private Vector3 crossUp;
    private Vector3 normal;
    private float angleVector;
    private double movementStore;
    private bool canMove = true;
    private bool timeStored = false;
    public float timeStore;
    public bool bonking = false;
    private Vector3 normalBonkVector;
    public bool wallJumping = false;

    public LayerMask wall;
    public LayerMask ground;
    private RaycastHit hitInfo;
    public bool debug;


    // Start is called before the first frame update
    void Start()
    {
        // assign our Character Controller
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        // Enironment Checks
        WallBonkAndJump();

        // Turn player input into direction
        GetDirection();

        // Player movement methods
        CalculateVerticalMovement();
        MovePlayer();

        // Animations
        Animation();

        // Methods for debugging
        DrawDebugLines();
    }

    // Retrieves the input from the player
    void GetDirection()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        if (!canMove)
        {
            // pass
        }
        else
        {
            // calculate direction relative to camera's orientation
            direction = new Vector3(horizontal, 0f, vertical).normalized;
            targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
        }
    }

    // Find the vertical component of our movement
    void CalculateVerticalMovement()
    {
        // store our vertical speed
        float yStore = velocity.y;
        velocity.y = yStore;

        // find the player's vertical speed previous speed and if they are jumping or not
        if (controller.isGrounded)
        {
            velocity.y = -2f;
        }
        if (Input.GetButtonDown("Jump") && (Physics.Raycast(transform.position, -Vector3.up, out hitInfo, 1.2f, ground) || controller.isGrounded))
        {
            velocity.y = jumpForce;
        }

        // apply gravity and take previous vertical speed into account
        velocity.y = velocity.y + (Physics.gravity.y * gravityScale * Time.deltaTime);

    }

    // Moves and rotates the player
    void MovePlayer()
    {
        // if the player is holding a direction, it will move them depending on where the camera is facing
        if (direction.magnitude >= 0.1f)
        {
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            if (!bonking || wallJumping)
            {
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
            }
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward * speed;
            //convert character to slope movement if on slope
            if (Physics.Raycast(transform.position, -Vector3.up, out hitInfo, 1.2f, ground) && 35 >= Vector3.Angle(player.transform.up, hitInfo.normal))
            {
                Vector3 normal = hitInfo.normal;
                Vector3 up = player.transform.up;
                Vector3 cross = Vector3.Cross(up, normal);
                float vectorAngle = Vector3.Angle(up, normal);
                if (cross.magnitude > .0001)
                {
                    player.RotateAround(player.position, cross.normalized, vectorAngle);
                }
                Vector3 crossSide = Vector3.Cross(Vector3.up, normal);
                Vector3 slopeVector = Vector3.Cross(normal, crossSide);
                float radians = -vectorAngle * Mathf.Deg2Rad;
                moveDir = Vector3.RotateTowards(moveDir, slopeVector, radians, 0);
            }
            else
            {
                Vector3 normal = Vector3.up;
                Vector3 up = player.transform.up;
                Vector3 cross = Vector3.Cross(up, normal);
                float vectorAngle = Vector3.Angle(up, normal);
                if (cross.magnitude > .0001)
                {
                    player.RotateAround(player.position, cross.normalized, vectorAngle);
                }
            }
            movementStore = Math.Sqrt(Math.Pow(moveDir.x, 2) + Math.Pow(moveDir.z, 2));
            // add the vertical and horizontal movement vectors to get our total movement vector
            moveDir = moveDir + velocity;
            controller.Move(moveDir * Time.deltaTime);
        }

        // if the player is not holding a direction, only gravity and jumping will be accounted for
        else
        {
            //convert characters rotation if standing still on slope
            if (Physics.Raycast(transform.position, -Vector3.up, out hitInfo, 1.2f, ground) && 35 >= Vector3.Angle(player.transform.up, hitInfo.normal))
            {
                Vector3 normal = hitInfo.normal;
                Vector3 up = player.transform.up;
                Vector3 cross = Vector3.Cross(up, normal);
                float vectorAngle = Vector3.Angle(up, normal);
                if (cross.magnitude > .0001)
                {
                    player.RotateAround(player.position, cross.normalized, vectorAngle);
                }
            }
            else
            {
                Vector3 normal = Vector3.up;
                Vector3 up = player.transform.up;
                Vector3 cross = Vector3.Cross(up, normal);
                float vectorAngle = Vector3.Angle(up, normal);
                if (cross.magnitude > .0001)
                {
                    player.RotateAround(player.position, cross.normalized, vectorAngle);
                }
            }
            movementStore = 0f;
            controller.Move(velocity * Time.deltaTime);
        }
    }


    void DrawDebugLines()
    {
        if (!debug) return;

        Debug.DrawLine(player.position, player.position + moveDir * 10, Color.red);
        Debug.DrawLine(transform.position, transform.position - Vector3.up * 2, Color.yellow);
    }
    void Animation()
    {
        anim.SetBool("isGrounded", controller.isGrounded);
        anim.SetFloat("Speed", (Mathf.Abs(Input.GetAxis("Vertical")) + Mathf.Abs(Input.GetAxis("Horizontal"))));
    }
    void WallBonkAndJump()
    {
        if (Physics.Raycast(transform.position, transform.forward, out hitInfo, .6f, wall) && movementStore > 0f && controller.isGrounded == false)
        {
            if (bonking == false || wallJumping == true)
            {
                timeStore = Time.time;
                canMove = false;
                normalBonkVector = hitInfo.normal.normalized;
                direction = normalBonkVector * 20;
                targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                bonking = true;
            }
        }
        if (bonking && Time.time < timeStore + .25f && Input.GetButtonDown("Jump"))
        {
            velocity = new Vector3(0f, 20f, 0f);
            direction = normalBonkVector * 75;
            targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(targetAngle, 0f, 0f);
            wallJumping = true;
        }
        if (bonking && controller.isGrounded)
        {
            canMove = true;
            bonking = false;
            wallJumping = false;
        }
    }
}
