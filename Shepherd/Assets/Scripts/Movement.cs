using UnityEngine;

public class Movement : MonoBehaviour
{

    public float speed = 5f;
    public Rigidbody2D body;

    Vector2 movement;

    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (movement.x != 0 || movement.y != 0)
        {
            animator.SetBool("isWalking", true);
            animator.SetFloat("InputX", movement.x);
            animator.SetFloat("InputY", movement.y);
            animator.SetFloat("LastInputX", movement.x);
            animator.SetFloat("LastInputY", movement.y);
        }
        else
        {

            animator.SetBool("isWalking", false);

        }

    }

    void FixedUpdate()
    {

        body.MovePosition(body.position + movement * speed * Time.fixedDeltaTime);

    }

}
