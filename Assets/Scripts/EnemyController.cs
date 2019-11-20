using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : PhysicsObject
{
    public float maxSpeed = 7;
    public float jumpTakeOffSpeed = 7;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private float direction = 1;
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Mathf.Abs(velocity.x) < 0.01) {            
            direction *= -1;
            spriteRenderer.flipX = (direction < 0);
        }
        Vector2 move = new Vector2(direction, 0);
        targetVelocity = move * maxSpeed;
        animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);
    }
}
