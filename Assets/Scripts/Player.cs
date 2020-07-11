using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public bool isGrounded;
    public bool isSprinting;

    private Transform cam;
    private World world;

    public float walkSpeed = 3f;
    public float sprintSpeed = 6f;
    public float jumpForce = 5f;
    public float gravity = -9.8f;

    public float playerWidthRadius = .4f;

    private float horizontal;
    private float vertical;
    private float mouseHorizontal;
    private float mouseVertical;
    private float mouseSensitivity = 5.0f;
    private Vector3 velocity;
    private float verticalMomentum = 0;
    private bool jumpRequest;


    private void Start()
    {
        cam = GameObject.Find("Main Camera").transform;
        world = GameObject.Find("World").GetComponent<World>();
    }

    private void FixedUpdate()
    {
        CalcualteVelocity();
        if (jumpRequest)
            Jump();

        transform.Rotate(Vector3.up * mouseHorizontal * mouseSensitivity);
        cam.Rotate(Vector3.right * -mouseVertical * mouseSensitivity);
        transform.Translate(velocity, Space.World);
    }

    private void Update()
    {
        GetPlayerInputs();
    }

    void Jump()
    {
        verticalMomentum = jumpForce;
        isGrounded = false;
        jumpRequest = false;
    }

    private void CalcualteVelocity()
    {
        // Affect vertical momentum with gravity
        if (verticalMomentum > gravity)
            verticalMomentum += Time.fixedDeltaTime * gravity; // use fixedDeltaTime for physics calculations

        // Sprinting check
        if (isSprinting)
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * sprintSpeed;
        else
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * walkSpeed;

        // Apply vertical momentum (falling/jumping)
        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;

        // if there is movement along x or z axis and there is something in the way
        // block movement in that direction
        if ((velocity.x > 0 && right) || (velocity.x < 0 && left))
            velocity.x = 0;
        if ((velocity.z > 0 && front) || (velocity.z < 0 && back))
            velocity.z = 0;

        // check if we can move in +/- y direction
        if (velocity.y < 0)
            velocity.y = checkDownSpeed(velocity.y);
        if (velocity.y > 0)
            velocity.y = checkUpSpeed(velocity.y);

    }

    private void GetPlayerInputs()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        mouseHorizontal = Input.GetAxis("Mouse X");
        mouseVertical = Input.GetAxis("Mouse Y");

        if (Input.GetButtonDown("Sprint"))
            isSprinting = true;
        if (Input.GetButtonUp("Sprint"))
            isSprinting = false;
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            jumpRequest = true;
        }
    }

    private float checkDownSpeed(float downSpeed)
    {
        // if there is a solid voxel under the player
        // need to check 4 voxels around the player, as the player may be over 4 voxels at once
        if (
            (world.CheckForVoxel(transform.position.x - playerWidthRadius, transform.position.y + downSpeed, transform.position.z - playerWidthRadius) && (!left && !back)) ||
            (world.CheckForVoxel(transform.position.x + playerWidthRadius, transform.position.y + downSpeed, transform.position.z - playerWidthRadius) && (!right && !back)) ||
            (world.CheckForVoxel(transform.position.x - playerWidthRadius, transform.position.y + downSpeed, transform.position.z + playerWidthRadius) && (!right && !front)) ||
            (world.CheckForVoxel(transform.position.x + playerWidthRadius, transform.position.y + downSpeed, transform.position.z + playerWidthRadius) && (!left && !front))
        )
        {
            isGrounded = true;
            return 0;
        }
        else
        {
            isGrounded = false;
            return downSpeed;
        }
    }

    private float checkUpSpeed(float upSpeed)
    {
        // if there is a solid voxel under the player
        // need to check 4 voxels around the player, as the player may be over 4 voxels at once
        if (
            (world.CheckForVoxel(transform.position.x - playerWidthRadius, transform.position.y + 2f + upSpeed, transform.position.z - playerWidthRadius) && (!left && !back)) ||
            (world.CheckForVoxel(transform.position.x + playerWidthRadius, transform.position.y + 2f + upSpeed, transform.position.z - playerWidthRadius) && (!right && !back)) ||
            (world.CheckForVoxel(transform.position.x - playerWidthRadius, transform.position.y + 2f + upSpeed, transform.position.z + playerWidthRadius) && (!right && !front)) ||
            (world.CheckForVoxel(transform.position.x + playerWidthRadius, transform.position.y + 2f + upSpeed, transform.position.z + playerWidthRadius) && (!left && !front))
        )
        {
            verticalMomentum = 0; // set to 0 so the player falls when their head hits a block while jumping
            return 0;
        }
        else
        {
            return upSpeed;
        }
    }

    public bool front
    {
        get
        {
            return
                world.CheckForVoxel(transform.position.x, transform.position.y, transform.position.z + playerWidthRadius) ||   // check feet
                world.CheckForVoxel(transform.position.x, transform.position.y + 1f, transform.position.z + playerWidthRadius); // check head
        }
    }

    public bool back
    {
        get
        {
            return
                world.CheckForVoxel(transform.position.x, transform.position.y, transform.position.z - playerWidthRadius) ||   // check feet
                world.CheckForVoxel(transform.position.x, transform.position.y + 1f, transform.position.z - playerWidthRadius); // check head
        }
    }

    public bool left
    {
        get
        {
            return
                world.CheckForVoxel(transform.position.x - playerWidthRadius, transform.position.y, transform.position.z) ||   // check feet
                world.CheckForVoxel(transform.position.x - playerWidthRadius, transform.position.y + 1f, transform.position.z); // check head
        }
    }

    public bool right
    {
        get
        {
            return
                world.CheckForVoxel(transform.position.x + playerWidthRadius, transform.position.y, transform.position.z) ||   // check feet
                world.CheckForVoxel(transform.position.x + playerWidthRadius, transform.position.y + 1f, transform.position.z); // check head
        }
    }

}
