using UnityEngine;

public class Movement : MonoBehaviour
{

    public float speed = 5f;
    public Rigidbody2D body;

    Vector2 movement;

    void Update()
    {

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

    }

    void FixedUpdate()
    {

        body.MovePosition(body.position + movement * speed * Time.fixedDeltaTime);

    }

}
