using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private float climbSpeed = 4f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;

    private Animator animator;
    private bool isGrounded;
    private Rigidbody2D rb;
    private GameManager gameManager;
    private bool isOnLadder = false;
    private bool isClimbing = false;
    private float defaultGravityScale;
    private AudioManager audioManager;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        defaultGravityScale = rb.gravityScale;
    }
    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        audioManager=FindAnyObjectByType<AudioManager>();
    }

    void Update()
    {
        if (gameManager.IsGameOver()||gameManager.IsGameWin()) return;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
        HandleMovement();
        HandleJump();
        HandleClimb();
        UpdateAnimation();
    }

    private void HandleMovement()
    {
        float moveInput = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        if (moveInput > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput < 0) transform.localScale = new Vector3(-1, 1, 1);
    }

    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            audioManager.PlayJumpSound();
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isClimbing = false;
            rb.gravityScale = defaultGravityScale;
        }
    }

    private void HandleClimb()
    {
        float verticalInput = Input.GetAxis("Vertical");

        if (isOnLadder && Mathf.Abs(verticalInput) > 0.1f)
        {
            isClimbing = true;
        }

        if (isClimbing)
        {
            rb.gravityScale = 0f; 
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, verticalInput * climbSpeed);
            if (!isOnLadder || (isGrounded && Mathf.Abs(verticalInput) < 0.1f))
            {
                isClimbing = false;
                rb.gravityScale = defaultGravityScale;
            }
        }
        else
        {
            rb.gravityScale = defaultGravityScale;
        }
    }

    private void UpdateAnimation()
    {
        bool isRunning = Mathf.Abs(rb.linearVelocity.x) > 0.1f && !isClimbing;
        bool isJumping = !isGrounded && !isClimbing;
        bool isClimbingAnim = isClimbing && Mathf.Abs(rb.linearVelocity.y) > 0.1f;

        animator.SetBool("isRunning", isRunning);
        animator.SetBool("isJumping", isJumping);
        animator.SetBool("isClimbing", isClimbingAnim);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            isOnLadder = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            isOnLadder = false;
            isClimbing = false;
            rb.gravityScale = defaultGravityScale;
        }
    }
}