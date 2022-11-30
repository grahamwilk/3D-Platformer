using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Windows;
using UnityEngine.UIElements;

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
    public Health playerHealth;
    public Transform fist;
    public PunchCollision punchCollision;

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
    private float timeStore;
    private bool bonking = false;
    private Vector3 normalBonkVector;
    private bool wallJumping = false;
    private bool punching = false;
    private bool punchEnd = false;
    private bool wallPunch = false;
    private bool groundBonking = false;
    private int totalHealth = 10;
    private int health = 10;
    private float deathTimeStore;
    private GameObject enemyObjectStore;
    private bool wallJumpAnim;
    private float wallJumpAnimTimeStore;
    private bool ableToWallJump;

    public LayerMask wall;
    public LayerMask ground;
    private RaycastHit hitInfo;
    public bool debug;


    // Start is called before the first frame update
    void Start()
    {
        // assign our Character Controller
        controller = GetComponent<CharacterController>();
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        // Enironment Checks

        Debug.Log("Hello");
        WallBonkAndJump();

        //punch
        Punch();

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
    // if the player gets in contact with an enemy, this script runs
    void OnCollisionEnter(Collision collisionInfo)
    {
        if (collisionInfo.collider.tag == "Enemy" && !groundBonking)
        {
            canMove = false;
            ContactPoint contactPoint = collisionInfo.GetContact(0);
            normalBonkVector = contactPoint.normal;
            direction = normalBonkVector;
            targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            speed = speed / 2;
            groundBonking = true;
            playerHealth.Damage(2);
            timeStore = Time.time;
            FindObjectOfType<AudioManager>().Play("PlayerDamage");
        }
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
        if (Input.GetButtonDown("Jump") && (Physics.Raycast(transform.position, -Vector3.up, out hitInfo, 1.2f, ground) || controller.isGrounded) && !bonking && !groundBonking)
        {
            FindObjectOfType<AudioManager>().Play("Jump");
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
                    transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, angle, transform.rotation.eulerAngles.z);
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
                    transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, angle, transform.rotation.eulerAngles.z);
                }
            }
            movementStore = Math.Sqrt(Math.Pow(moveDir.x, 2) + Math.Pow(moveDir.z, 2));
            if (punching) { moveDir = moveDir * .5f; }
            if (wallPunch) { moveDir = moveDir * 3f; }
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

    // Draws some minor debug lines
    void DrawDebugLines()
    {
        if (!debug) return;

        Debug.DrawLine(player.position, player.position + moveDir * 10, Color.red);
        Debug.DrawLine(transform.position, transform.position - Vector3.up * 2, Color.yellow);
    }
    // basic player animation
    void Animation()
    {
        anim.SetBool("isGrounded", controller.isGrounded);
        anim.SetFloat("Speed", (Mathf.Abs(Input.GetAxis("Vertical")) + Mathf.Abs(Input.GetAxis("Horizontal"))));
        anim.SetBool("isGroundBonking", groundBonking);
        anim.SetBool("isBonking", bonking);
        anim.SetBool("isWallJumping", wallJumping);
        anim.SetBool("resetWallJump", wallJumpAnim);
        anim.SetBool("isAbleToWallJump", ableToWallJump);
        anim.SetBool("isPunching", punching);
        anim.SetTrigger("attack", 
    }
   
    // if the player moves into a wall with a high enough speed, they will bonk off of it.
    void WallBonkAndJump()
    {
        if (Physics.Raycast(transform.position, transform.forward, out hitInfo, .6f, wall) && movementStore > 0f && controller.isGrounded == false)
        {
            if (bonking == false || wallJumping == true)
            {
                timeStore = Time.time;
                canMove = false;
                normalBonkVector = hitInfo.normal.normalized;
                direction = normalBonkVector;
                targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                bonking = true;
                wallJumping = false;
                ableToWallJump = true;
            }
        }
        if (bonking && Time.time < timeStore + .15f && Input.GetButtonDown("Jump"))
        {
            velocity = new Vector3(0f, 20f, 0f);
            direction = normalBonkVector;
            targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(targetAngle, 0f, 0f);
            wallJumping = true;
            bonking = false;
            FindObjectOfType<AudioManager>().Play("WallJump");
            wallJumpAnim = true;
            wallJumpAnimTimeStore = Time.time;
        }
        if (bonking && Time.time > timeStore + .15f)
        {
            ableToWallJump = false;
        }
        if (Time.time > wallJumpAnimTimeStore + .1f)
        {
            wallJumpAnim = false;
        }
        if (wallJumping && controller.isGrounded)
        {
            wallJumping = false;
            canMove = true;
        }
        if (bonking && controller.isGrounded)
        {
            speed = speed / 2;
            bonking = false;
            groundBonking = true;
            wallJumping = false;
            timeStore = Time.time;
            FindObjectOfType<AudioManager>().Play("WallBonk");
        }
        if (groundBonking && Time.time > timeStore + 1f)
        {
            speed = speed * 2;
            canMove = true;
            groundBonking = false;
            direction = Vector3.zero;
        }
    }
   
   
    // if the player presses a button, they will punch
    void Punch()
    {
        if (Input.GetButtonDown("Fire1") && !groundBonking)
        {
            if (controller.isGrounded == true && punching == false)
            {
                targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
                timeStore = Time.time;
                canMove = false;
                punching = true;
                fist.position += transform.forward;
                FindObjectOfType<AudioManager>().Play("Punch");
            }
        }
        if (punching)
        {
            if (Physics.Raycast(transform.position, transform.forward, out hitInfo, 1f, wall))
            {
                Vector3 normalPunchVector = hitInfo.normal.normalized;
                direction = normalPunchVector;
                targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                wallPunch = true;
                FindObjectOfType<AudioManager>().Play("PunchWall");
            }
        }
        if (punching && punchCollision.HitSomething)
        {
            Vector3 forceVector = (Vector3.up/2) + transform.forward;
            punchCollision.ACollision.rigidbody.AddForce(forceVector /2, ForceMode.VelocityChange);
            deathTimeStore = Time.time;
            enemyObjectStore = punchCollision.ACollision.gameObject;
        }
        if (Time.time > deathTimeStore +.5f)
        {
            Destroy(enemyObjectStore);
        }
        // punch stops, extra .25 to stop player
        if (punching && Time.time > timeStore + .5f)
        {
            direction = Vector3.zero;
            punching = false;
            wallPunch = false;
            punchEnd = true;
            fist.position -= transform.forward;
        }
        // end of the punch
        if (punchEnd && Time.time > timeStore + .85f)
        {
            canMove = true;
            punchEnd = false;
        }
    }
}
