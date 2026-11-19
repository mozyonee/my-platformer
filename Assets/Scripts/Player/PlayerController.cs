using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Range(0f, 20f)] [SerializeField] float maxSpeed = 8f;
    [Range(0f, 200f)] [SerializeField] float acceleration = 70f;
    [Range(0f, 200f)] [SerializeField] float deceleration = 90f;
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundCheckRadius = 0.15f;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float jumpHeight = 3f;
    [SerializeField] float baseGravity = 3f;
    [SerializeField] float coyoteTime = 0.12f;
    [SerializeField] float jumpBufferTime = 0.15f;
    [SerializeField] float fallMultiplier = 2.2f;
    [SerializeField] float lowJumpMultiplier = 3.5f;
    [SerializeField] float stepInterval = 0.32f;
    [SerializeField] float hardLandingSpeed = 9f;

    Rigidbody2D body;
    PlayerControls controls;
    SpriteRenderer spriteRenderer;
    Animator animator;
    AudioSource audioSource;
    [SerializeField] AudioClip[] footstepClips;
    [SerializeField] Transform interactPoint;
    [SerializeField] float interactRadius = 0.6f;
    [SerializeField] LayerMask interactableLayer;

    CinemachineImpulseSource impulseSource;

    float moveInput;
    bool isGrounded;
    float coyoteCounter;
    float jumpBufferCounter;
    float stepTimer;
    float peakFallSpeed;
    bool wasGrounded;
    float stepResetTimer;

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        controls = new PlayerControls();
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();
    void OnDestroy()
    {
        if (controls != null)
            controls.Dispose();
    }

    void Update()
    {
        moveInput = controls.Player.Move.ReadValue<Vector2>().x;
        isGrounded = CheckGround();
        UpdateCoyoteAndJumpBuffer();
        TryJump();
        UpdateVisuals();
        UpdateFootsteps();
        CheckLandingShake();
        TryInteract();
        wasGrounded = isGrounded;
    }

    void FixedUpdate()
    {
        float target = moveInput * maxSpeed;
        float speed = Mathf.Abs(body.linearVelocity.x);
        float step = (Mathf.Abs(target) < 0.01f ? deceleration : acceleration) * Time.fixedDeltaTime;
        float newX = Mathf.MoveTowards(body.linearVelocity.x, target, step);
        body.linearVelocity = new Vector2(newX, body.linearVelocity.y);

        if (body.linearVelocity.y < 0f)
        {
            body.gravityScale = baseGravity * fallMultiplier;
            peakFallSpeed = Mathf.Max(peakFallSpeed, -body.linearVelocity.y);
        }
        else if (body.linearVelocity.y > 0f && !controls.Player.Jump.IsPressed())
            body.gravityScale = baseGravity * lowJumpMultiplier;
        else
            body.gravityScale = baseGravity;
    }

    bool CheckGround() =>
        Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

    void UpdateCoyoteAndJumpBuffer()
    {
        if (isGrounded && body.linearVelocity.y <= 0.01f)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.deltaTime;

        if (controls.Player.Jump.WasPressedThisFrame())
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;
    }

    void TryJump()
    {
        if (coyoteCounter > 0f && jumpBufferCounter > 0f)
        {
            Jump();
            coyoteCounter = 0f;
            jumpBufferCounter = 0f;
        }
    }

    void Jump()
    {
        body.gravityScale = baseGravity;
        float jumpVelocity = Mathf.Sqrt(2f * Mathf.Abs(Physics2D.gravity.y) * baseGravity * jumpHeight);
        body.linearVelocity = new Vector2(body.linearVelocity.x, jumpVelocity);
    }

    void UpdateVisuals()
    {
        if (animator != null)
            animator.SetFloat("Speed", Mathf.Abs(body.linearVelocity.x));
        // Kenney player art faces left; flip when moving right.
        if (Mathf.Abs(moveInput) > 0.01f)
            spriteRenderer.flipX = moveInput > 0f;
    }

    void UpdateFootsteps()
    {
        if (!isGrounded || Mathf.Abs(body.linearVelocity.x) <= 0.5f)
        {
            stepTimer = stepInterval;
            return;
        }
        stepTimer += Time.deltaTime;
        if (stepTimer >= stepInterval)
        {
            PlayFootstep();
            stepTimer = 0f;
        }
    }

    void PlayFootstep()
    {
        if (footstepClips == null || footstepClips.Length == 0 || audioSource == null)
            return;
        var clip = footstepClips[Random.Range(0, footstepClips.Length)];
        audioSource.pitch = 1f + Random.Range(-0.12f, 0.12f);
        audioSource.PlayOneShot(clip);
    }

    void CheckLandingShake()
    {
        if (!isGrounded)
        {
            peakFallSpeed = Mathf.Max(peakFallSpeed, -body.linearVelocity.y);
            return;
        }
        if (!wasGrounded && peakFallSpeed >= hardLandingSpeed && impulseSource != null)
            impulseSource.GenerateImpulseWithForce(0.3f);
        if (!wasGrounded)
            peakFallSpeed = 0f;
    }

    void TryInteract()
    {
        if (!controls.Player.Interact.WasPressedThisFrame() || interactPoint == null)
            return;
        var hit = Physics2D.OverlapCircle(interactPoint.position, interactRadius, interactableLayer);
        if (hit != null && hit.TryGetComponent<IInteractable>(out var target))
            target.Interact();
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;
        Gizmos.color = Application.isPlaying && isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}