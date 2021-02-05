using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class handles player movement and animation.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    /// <summary>
    /// Different inputs of the player.
    /// The GetMovementState() function handles to which KeyCodes the input reacts.
    /// NOTE: This could be improved by using Unity's input system for a direct binding of
    /// input action to input key, for which I did not have time.
    /// </summary>
    private enum MovementInput
    {
        Idle,
        WalkLeft,
        WalkRight,
        RunLeft,
        RunRight,
        Jump
    }

    /// <summary>
    /// The force of the jump of the player.
    /// </summary>
    [SerializeField] private float jumpForce;
    
    /// <summary>
    /// The movement speed of the player.
    /// </summary>
    [SerializeField] private float moveSpeed;
    
    /// <summary>
    /// The multiplier used on the normal movement speed of the player when running.
    /// </summary>
    [SerializeField] private float runSpeedMultiplier;
    
    /// <summary>
    /// Distance to check downwards for ground detection.
    /// </summary>
    [SerializeField] private float groundCheckDistance;
    
    /// <summary>
    /// Layer of the world for ground detection.
    /// </summary>
    [SerializeField] private LayerMask worldLayer;

    /// <summary>
    /// Rigidbody component of the player.
    /// </summary>
    private Rigidbody2D _rigidbody;
    
    /// <summary>
    /// SpriteRenderer of the player.
    /// </summary>
    private SpriteRenderer _spriteRenderer;
    
    /// <summary>
    /// Collider of the player.
    /// </summary>
    private BoxCollider2D _collider;
    
    /// <summary>
    /// Animator of the player.
    /// </summary>
    private Animator _animator;
    
    /// <summary>
    /// Current movement input of the player.
    /// </summary>
    private MovementInput _movementInput;
    
    /// <summary>
    /// Bottom left of player collider.
    /// Used for ground detection.
    /// </summary>
    private Vector2 _colliderBottomLeft;
    
    /// <summary>
    /// Bottom right of player collider.
    /// Used for ground detection.
    /// </summary>
    private Vector2 _colliderBottomRight;


    /// <summary>
    /// Gets components and sets up collider variables.
    /// </summary>
    void Start()
    {
        _collider = GetComponent<BoxCollider2D>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        var size = _collider.size;
        _colliderBottomLeft = -new Vector3(0.5f * size.x, 0.5f * size.y);
        _colliderBottomRight = -new Vector3(-0.5f * size.x, 0.5f * size.y);
    }

    
    /// <summary>
    /// Updates input and animation.
    /// </summary>
    void Update()
    {
        UpdateInput();
        UpdateAnimation();
    }

    
    /// <summary>
    /// Updates the current movement state and updates player movement to current state.
    /// </summary>
    private void UpdateInput()
    {
        _movementInput = GetMovementState();

        switch (_movementInput)
        {
            case MovementInput.Idle:
                break;
            case MovementInput.WalkLeft:
                _rigidbody.AddForce(Vector2.left * moveSpeed);
                break;
            case MovementInput.WalkRight:
                _rigidbody.AddForce(Vector2.right * moveSpeed);
                break;
            case MovementInput.RunLeft:
                _rigidbody.AddForce(Vector2.left * (moveSpeed * runSpeedMultiplier));
                break;
            case MovementInput.RunRight:
                _rigidbody.AddForce(Vector2.right * (moveSpeed * runSpeedMultiplier));
                break;
            case MovementInput.Jump:
                _rigidbody.AddForce(Vector2.up * jumpForce);
                break;
        }
    }

    /// <summary>
    /// Gets the new movement state by handling all input logic.
    /// </summary>
    /// <returns>Updates input logic.</returns>
    private MovementInput GetMovementState()
    {
        // JUMP
        if (Input.GetKeyDown(KeyCode.W) && IsOnGround())
        {
            return MovementInput.Jump;
        }

        // LEFT MOVEMENT
        if (Input.GetKey(KeyCode.A))
        {
            // CHECK FOR SPRINT
            if (Input.GetKey(KeyCode.LeftShift))
            {
                return MovementInput.RunLeft;
            }

            return MovementInput.WalkLeft;
        }

        // RIGHT MOVEMENT
        if (Input.GetKey(KeyCode.D))
        {
            // CHECK FOR SPRINT
            if (Input.GetKey(KeyCode.LeftShift))
            {
                return MovementInput.RunRight;
            }

            return MovementInput.WalkRight;
        }

        // DEFAULT STATE
        return MovementInput.Idle;
    }

    /// <summary>
    /// Checks if player is on ground by using two raycasts from lower edges of the collider.
    /// </summary>
    /// <returns>Is the player on the ground.</returns>
    private bool IsOnGround()
    {
        RaycastHit2D hitLeft = Physics2D.Raycast(_rigidbody.position + _colliderBottomLeft, -transform.up,
            groundCheckDistance,
            worldLayer.value);
        RaycastHit2D hitRight = Physics2D.Raycast(_rigidbody.position + _colliderBottomRight, -transform.up,
            groundCheckDistance, worldLayer.value);

        return hitLeft.transform != null || hitRight.transform != null;
    }

    /// <summary>
    /// Updates animations depending on player movement.
    /// </summary>
    private void UpdateAnimation()
    {
        // check for movement
        switch (_movementInput)
        {
            case MovementInput.WalkLeft:
            case MovementInput.WalkRight:
            case MovementInput.RunLeft:
            case MovementInput.RunRight:
                _animator.SetBool("moving", true);
                break;
            default:
                _animator.SetBool("moving", false);
                break;
        }

        // check for jump or fall
        _animator.SetFloat("y_Speed", _rigidbody.velocity.y);
        
        // check if sprite needs to be inverted
        if (Math.Abs(_rigidbody.velocity.x) > 0.05f)
        {
            _spriteRenderer.flipX = _rigidbody.velocity.x < 0f;
        }
    }
}