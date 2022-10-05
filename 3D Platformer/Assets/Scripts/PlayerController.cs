using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{


    public float moveSpeed;
    //public Rigidbody theRB;
    public float jumpForce;
    public CharacterController controller;
    private Vector3 moveDirection;
    public float gravityScale;

    // Start is called before the first frame update
    void Start()
    {
        //theRB = GetComponent<Rigidbody>();
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        /*theRB.velocity = new Vector3(Input.GetAxis("Horizontal") * moveSpeed,theRB.velocity.y,Input.GetAxis("Vertical") * moveSpeed);
        if (Input.GetButtonDown("Jump"))
        {
            theRB.velocity = new Vector3(theRB.velocity.x,jumpForce,theRB.velocity.y);
        }*/
        moveDirection = new Vector3(Input.GetAxis("Horizontal") * moveSpeed, 0f, Input.GetAxis("Vertical") * moveSpeed);
        if (Input.GetButtonDown("Jump"))
        {
            if (Physics.Raycast(transform.position, Vector3.down, 1.1f))
            {
                moveDirection.y = jumpForce;
            }
        }
        //if (Physics.Raycast(transform.position, Vector3.down, 1.1f))
        moveDirection.y = moveDirection.y + (Physics.gravity.y * gravityScale);
        controller.Move(moveDirection * Time.deltaTime);
    }
}
