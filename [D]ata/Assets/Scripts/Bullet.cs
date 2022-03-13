using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public Rigidbody2D rb;
    public float liveTime;

     void Start()
    {
        rb.velocity = transform.right * speed;
    }

     void Update()
    {
      Destroy(gameObject, liveTime);  
    }
}
