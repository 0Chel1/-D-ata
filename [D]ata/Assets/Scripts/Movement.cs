using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float speed;
    public float offset;
    private Vector2 moveVelocity;
    private Rigidbody2D rb;


     void Start()
    {
       rb = GetComponent<Rigidbody2D>(); 
    }

     void Update()
    {
        /*Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        moveVelocity = moveInput.normalized * speed;*/
        moveVelocity.x = Input.GetAxisRaw("Horizontal");
        moveVelocity.y = Input.GetAxisRaw("Vertical");
        Vector3 difference = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        float rotateZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, rotateZ + offset);
    }

     void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveVelocity * speed * Time.fixedDeltaTime);
    }
}
