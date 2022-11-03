using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PunchCollision : MonoBehaviour
{
    public Transform fist;
    private bool hitSomething = false;
    private Collision aCollision;
    public bool HitSomething
    {
        get { return this.hitSomething; }
    }
    public Collision ACollision
    {
        get { return aCollision; }
    }
    private void OnCollisionEnter(Collision other)
    {
        if (other.collider.tag == "Enemy")
        {
            hitSomething = true;
            aCollision = other;
        }
    }
    private void OnCollisionExit(Collision other)
    {
        if (other.collider.tag == "Enemy")
        {
            hitSomething = false;
        }
    }


}
