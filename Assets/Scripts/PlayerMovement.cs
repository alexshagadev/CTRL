using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Hey! I encourage you to play around with this script and the variables back in the editor (but keep a backup just in case).
    // NOTE: The "Header" statement is used to separate our variables into different sections.
    // See the PlayerMovement script component on the Player gameobject and you will see the sections.
    [Header("Component References")]
    public Rigidbody2D rb;

    [Header("Move")]
    float moveInput;
    public float moveForce = 10f;

    [Header("Jump")]
    public float jumpForce = 10f;
    public LayerMask groundCheckLayer;
    public Vector2 groundCheckBoxSize = new Vector2(1f, 0.1f); // Size of the box used for ground checking.
    public float groundCheckYOffset = -0.2f; // Offset used to make the ground check happen directly beneath the player.
    bool jumpInput;
    bool isGrounded;

    [Header("Coyote Time")]
    public float coyoteTimeDuration = 0.2f; // How long after leaving the ground the player can still jump (in seconds).
    float coyoteTimeCounter; // Counter for the Coyote Time.

    [Header("Jump Buffering")]
    public float jumpBufferLength = 0.2f; // How long to buffer a jump input (in seconds).
    float jumpBufferCounter; // Counter for jump buffering.



    // Start is called before the first frame update
    // You can use this method to define references and run code instantly at runtime. 
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        // This script is attached to the Player gameobject. When we use the GetComponent<>() method,
        // it will look through all of the components on the Player gameobject and will find the one
        // that matches the type we specified (Rigidbody2D).
        //
        // "gameObject" just refers to the current gameobject that this script is attached to.
        // It works kind of like the keyword "this", but "this" is a reference to the C# class,
        // whereas "gameObject" is a reference to the whole gameobject. 

        // Side note: because we set the rb reference as public, we can also drag and drop the Rigidbody2D
        // component straight into the script in the Player gameobject. Try it out yourself!
    }

    // Update is called once per frame
    // If the game runs at 60 frames per second, this method will be called 60 times per second.
    void Update()
    {
        HandleInput();
        HandleJump();

        HandleCoyoteTime();
        HandleJumpBuffer();
    }

    // FixedUpdate can be called once, multiple times, or zero times per frame.
    // The frequency with which this method is called is determined by Unity's physics system.
    private void FixedUpdate()
    {
        HandleMovement();
    }

    // This method gets the player's keyboard input.
    void HandleInput()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        // Input.GetAxisRaw returns any decimal value between -1 and 1 relative to the axis specified.
        // Here, we are using the horizontal axis, which means that moving left will return -1.0
        // and moving right will return 1.0. 
        // 
        // The moveInput variable (which is a float) is set to this value every frame since
        // this method is called in the Update() method. So for 60 FPS, there will be 60 input calls.
    }

    // This method handles horizontal movement according to the player input.
    void HandleMovement()
    {
        rb.velocity = new Vector2(moveInput * moveForce * Time.deltaTime, rb.velocity.y);
        // A lot of things are happening over here, so let's break it down. 
        // First, we are accessing the velocity property of the Rigidbody2D reference we made earlier.
        // Because we are working with only the X axis and the Y axis, rb.velocity will take in a Vector2 value. 
        // A Vector2 is a way to store two float values, such as (0,1), (2.3, 8.4), etc...
        // Vector2's can be used to store positional data, coordinates, or really anything else we need.
        // (read more here: https://docs.unity3d.com/ScriptReference/Rigidbody2D-velocity.html AND https://docs.unity3d.com/ScriptReference/Vector2.html)

        // Now that we know that rb.velocity takes in a pair of float values, we need to know what to actually assign it to.
        // Let's imagine our Vector2 as (x,y). 

        // X PART
        // moveInput * moveForce * Time.deltaTime
        // First, we will use the moveInput we calculated earlier as a way to determine the direction in which the player wants to move (-1 is left, 1 is right)
        // Then, we will amplify this value by multiplying it by the moveForce variable we created.
        // Lastly, we will multiply the result by Time.deltaTime, which is a property Unity used to determine how much time has passed since the last frame. 
        // The reason we are multiplying our movement with this time interval is to ensure that the movement will be equal on all devices.
        // If a high-end PC can manage a huge FPS, then the time interval between each frame will be smaller (thus making the per-frame impact of the movement smaller as well)
        // If a low-end PC struggles with getting even high FPS values, then the time between each frame will be much longer (thus making the per-frame impact of the movement bigger as a result of multiplication). 
        // The point is that Time.deltaTime is a way to ensure equality between different kinds of framerates (the framerate will not impact the movement speed).

        // Y PART
        // rb.velocity.y
        // If we are dealing with horizontal movement, why do we need to add a y-axis value here?
        // rb.velocity works with both the x axis and the y axis whether we like it or not. 
        // Even though you could set the y velocity to zero, that would reset the player's y velocity each frame, making jumping impossible.
        // If we instead assign it to the current value of rb.velocity.y, it won't do anything.
        // When we assign the y-velocity to the current y-velocity, we are essentially saying A = A. Nothing changes.
    }

    // This method handles jump input and jumping.
    void HandleJump()
    {
        // Set the jump input. Input.GetKeyDown returns true only for the first frame when you press the specified key. This means that holding the jump key won't do anything.
        jumpInput = Input.GetKeyDown(KeyCode.Space);

        // Here, transform.position refers to the player's exact center. We'll add a negative offset on the y axis to perform the ground check slightly below the player.
        Vector2 groundCheckStartPos = new Vector2(transform.position.x, transform.position.y + groundCheckYOffset);

        // A BoxCast is a square-shaped observer that listens for collisions that happen with objects on a certain layer.
        // A more simple version of this is the Raycast - an invisible ray that shoots down to try to collide with things and collect information about them.
        // First, we determine the origin of the BoxCast, then its size, then the angle, then the direction in which it will go, then how far down it can move, and finally the layer it can collide with. 
        // If the BoxCast below collides with something on the groundCheckLayer (we set this to the Ground layer), it will fill its collider with information.
        // As you can see below, we access this collider at the very end and check if has any collision information and therefore isn't null. 
        // If the BoxCast's collider isn't null, then it collided with the ground layer and it returns true. This is how we check if the player is on the ground.
        isGrounded = Physics2D.BoxCast(groundCheckStartPos, groundCheckBoxSize, 0f, Vector2.down, 0.1f, groundCheckLayer).collider != null;

        // Jump if jump input is received and the player is grounded
        // COYOTE TIME & JUMP BUFFER HINT: think about how you can use this condition ((isGrounded || coyoteTimeCounter > 0) && jumpBufferCounter > 0)
        if ((isGrounded || coyoteTimeCounter > 0) && jumpBufferCounter > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            coyoteTimeCounter = 0;
            jumpBufferCounter = 0; // Reset both counters after jumping.
        }
    }

    // This is a method that will dislpay the ground check BoxCast in the editor using a red color. 
    // If you want to know how this method works, I suggest copying it into ChatGPT and asking it to explain it to you 
    // and give any relevant Unity docs as a reference. Your prompt might be "Explain the following using Unity docs, include links".
    // If you want to know more about CircleCasts, Raycasts, BoxCasts, etc... Please look at the Unity docs (or also ask ChatGPT to find those docs for you).
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector2 groundCheckStartPos = new Vector2(transform.position.x, transform.position.y + groundCheckYOffset);
        // Use DrawWireCube to visualize the BoxCast. The cube size parameter is doubled because Gizmos.DrawWireCube defines size in terms of full width and height, not radius.
        Gizmos.DrawWireCube(groundCheckStartPos + Vector2.down * 0.1f, new Vector3(groundCheckBoxSize.x, groundCheckBoxSize.y, 0));
    }

    void HandleCoyoteTime()
    {
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTimeDuration;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    void HandleJumpBuffer()
    {
        // Handle jump input specifically for jump buffering here
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferLength; // Activate jump buffer when space is pressed.
        }

        if (jumpBufferCounter > 0)
        {
            jumpBufferCounter -= Time.deltaTime; // Decrease the jump buffer counter over time.
        }
    }
}
