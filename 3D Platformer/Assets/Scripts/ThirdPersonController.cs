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

    private RaycastCommand groundingRay;
    public float steepAngle = 120f;

    public LayerMask ground;
    private RaycastHit hitInfo;
    public bool debug;

    /*
    // public variables for slopes
    public float height = 1f;
    public float heightPadding = .1f;
    public LayerMask ground;
    public float maxGroundAngle = 120;
    public bool debug;

    // private variables for slopes
    private float groundAngle;
    private Vector3 forward;
    private bool grounded;
    */

    // Start is called before the first frame update
    void Start()
    {
        // assign our Character Controller
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
        CalculateDirection();
        CalculateVerticalMovement();
        MovePlayer();
        DrawDebugLines();
        CalculateSlope();
        Animation();
    }

    // Retrieves the input from the player
    void GetInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 tempDirection = new Vector3(horizontal, 0f, vertical).normalized;
        direction = tempDirection;
    }

    // Direction relative to the camera's rotation
    void CalculateDirection()
    {
        targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
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
        if (Input.GetButtonDown("Jump") && Physics.Raycast(transform.position, -Vector3.up, out hitInfo, 1.15f, ground))
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
        if (direction.magnitude >= 0.1f /*&& groundAngle >= maxGroundAngle*/)
        {
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward * speed;
            // add the vertical and horizontal movement vectors to get our total movement vector
            moveDir = moveDir + velocity;
            controller.Move(moveDir * Time.deltaTime);
        }

        // if the player is not holding a direction, only gravity and jumping will be accounted for
        else
        {
            controller.Move(velocity * Time.deltaTime);
        }
    }
    // uh oh
    void CalculateSlope()
    {
        if (Physics.Raycast(transform.position, -Vector3.up, out hitInfo, 1.15f, ground))
        {
            Vector3 normal = hitInfo.normal;
            Vector3 up = player.transform.up;
            Vector3 cross = Vector3.Cross(up, normal);
            float vectorAngle = Vector3.Angle(up, normal);
            if (cross.magnitude > .0001)
            {
                player.RotateAround(player.position, cross.normalized, vectorAngle);
            }
            Vector3 crossUp = Vector3.Cross(Vector3.up, cross);
            direction = Quaternion.AngleAxis(vectorAngle,crossUp.normalized) * direction;
        }
    }

    void DrawDebugLines()
    {
        if (!debug) return;

        Debug.DrawLine(player.position, player.position + crossUp * 2 * 2, Color.red);
        Debug.DrawLine(transform.position, transform.position - Vector3.up * 2, Color.yellow);
    }
    void Animation()
    {
        anim.SetBool("isGrounded", controller.isGrounded);
        anim.SetFloat("Speed", (Mathf.Abs(Input.GetAxis("Vertical")) + Mathf.Abs(Input.GetAxis("Horizontal"))));
    }
}
