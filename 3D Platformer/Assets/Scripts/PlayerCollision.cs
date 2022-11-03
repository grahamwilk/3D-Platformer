using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    void OnCollisionEnter(Collision collisionInfo)
    {
        Debug.Log(collisionInfo.collider.tag);
    }
}
