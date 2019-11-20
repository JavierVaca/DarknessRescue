using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class PlayerController : PhysicsObject
{

    public float maxSpeed = 7;
    public float jumpTakeOffSpeed = 7;
    public Text livesText;
    public Text winText;
    public int maxLives = 5;
    private int lives;
    public Tilemap map;
    public TileBase tileType;

    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private Light Flashlight;
    private float height;
    private Vector3 fp;   //First touch position
    private Vector3 lp;   //Last touch position
    private float dragDistance;  //minimum distance for a swipe to be registered
    private bool dead;
    private Vector2 startPosition;
    private bool dig;
    private List<Vector3Int> tiles;   

    // Use this for initialization
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        Flashlight = GetComponentInChildren<Light>();
        dig = false;
        height = (float)Screen.height / 2.0f;
        dragDistance = Screen.height * 15 / 100; //dragDistance is 15% height of the screen
        lives = maxLives;
        livesText.text = "Lives: x" + lives.ToString();
        tiles = new List<Vector3Int>();
    }

    private void Start()
    {
        startPosition = rb2d.position;
    }

    protected override void ComputeVelocity()
    {
        Vector2 move = Vector2.zero;

        if (SystemInfo.deviceType == DeviceType.Handheld && !dead)
        {
            var touch = Input.GetTouch(0);
            move.x = touch.position.x < height ? -1 : 1;
            if (touch.phase == TouchPhase.Began) //check for the first touch
            {
                fp = touch.position;
                lp = touch.position;
            }
            else if (touch.phase == TouchPhase.Moved) // update the last position based on where they moved
            {
                lp = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended) //check if the finger is removed from the screen
            {
                lp = touch.position;  //last touch position. Ommitted if you use list

                //Check if drag distance is greater than 20% of the screen height
                if (Mathf.Abs(lp.x - fp.x) > dragDistance || Mathf.Abs(lp.y - fp.y) > dragDistance)
                {//It's a drag
                 //check if the drag is vertical or horizontal
                    if (Mathf.Abs(lp.x - fp.x) > Mathf.Abs(lp.y - fp.y))
                    {   //If the horizontal movement is greater than the vertical movement...
                        if ((lp.x > fp.x))  //If the movement was to the right)
                        {   //Right swipe
                            Debug.Log("Right Swipe");
                        }
                        else
                        {   //Left swipe
                            Debug.Log("Left Swipe");
                        }
                    }
                    else
                    {   //the vertical movement is greater than the horizontal movement
                        if (lp.y > fp.y)  //If the movement was up
                        {   //Up swipe
                            velocity.y = jumpTakeOffSpeed;
                            Debug.Log("Up Swipe");
                        }
                        else
                        {   //Down swipe
                            Dig();
                            Debug.Log("Down Swipe");
                        }
                    }
                }
                else
                {   //It's a tap as the drag distance is less than 20% of the screen height
                    Debug.Log("Tap");
                }
            }
        }
        if (!dead)
        {
            move.x = Input.GetAxis("Horizontal");

            if (Input.GetButtonDown("Jump") && grounded)
            {
                velocity.y = jumpTakeOffSpeed;
            }
            else if (Input.GetAxis("Vertical") < 0)
            {
                Dig();
            }
            else if (Input.GetButtonUp("Jump"))
            {
                if (velocity.y > 0)
                {
                    velocity.y = velocity.y * 0.5f;
                }
            }
        }


        bool flipSprite = (spriteRenderer.flipX ? (move.x > 0f) : (move.x < 0f));
        if (flipSprite)
        {
            spriteRenderer.flipX = !spriteRenderer.flipX;
        }

        animator.SetBool("grounded", grounded);
        animator.SetBool("Dig", dig);
        if (grounded)
        {
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);
        }
        else
        {
            animator.SetFloat("velocityX", -1f);
        }
        targetVelocity = move * maxSpeed;
        dig = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Water" && !dead)
        {
            Dead();
        }
        else if(collision.tag == "Exit" && !dead)
        {
            Win();
        }
    }

    private void Win()
    {
        winText.text = "You Win!";
        dead = true;
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        EnemyController enemy = other.gameObject.GetComponent<EnemyController>();
        if (enemy != null && !dead)
        {
            Dead();
        }
    }

    private void Dig()
    {
        Vector3 ground = Vector3.zero;
        ground.x = rb2d.position.x;
        ground.y = rb2d.position.y - 1;
        Vector3Int pPos = map.WorldToCell(ground);
        map.SetTile(pPos, null);
        tiles.Add(pPos);
        dig = true;
    }

    public void Dead()
    {
        dead = true;
        lives--;
        Invoke("ResetPosition", 2);
        animator.SetBool("Dead", dead);
        Flashlight.intensity = 0;
    }

    void ResetPosition()
    {
        livesText.text = "Lives: x" + lives.ToString();
        rb2d.position = startPosition;
        dead = false;
        Flashlight.intensity = 3;
        animator.SetBool("Dead", dead);
        ResetWorld();
    }

    void ResetWorld()
    {
        foreach (var tile in tiles)
        {
            map.SetTile(tile, tileType);
        }
        tiles.Clear();
    }
}