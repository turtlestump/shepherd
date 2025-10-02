using Unity.VisualScripting;
using UnityEngine;

public class Movement2D : MonoBehaviour
{

    public Rigidbody2D body;
    public float speed;
    public float movement;

    // Update is called once per frame
    void Update()
    {

        movement = Input.GetAxisRaw("Horizontal");

    }
    void FixedUpdate()
    {

        body.linearVelocity = new Vector2(movement * speed, body.linearVelocityY);

    }

}
