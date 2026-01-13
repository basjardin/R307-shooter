using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class AI_people : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("Movement Settings")]
    [SerializeField] private float wanderRadius = 20f;
    [SerializeField] private float wanderTimer = 5f;
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float wanderSpeed = 2f;

    [Header("Combat Settings")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float detectionRange = 10f;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Rigidbody rb;

    [Header("Gravity Settings")]
    [SerializeField] private float gravityMultiplier = 2f;
    [SerializeField] private float groundCheckDistance = 0.3f;
    [SerializeField] private LayerMask groundLayer = -1;
    [SerializeField] private bool enableGravity = true;

    // Private variables
    private Transform player;
    private float timer;
    private float nextAttackTime;
    private bool isActivated = false; // Changed from isPlayerInZone
    private bool isDead = false;
    private bool isGrounded = false;
    private bool isFalling = false;

    // Animation parameter names
    private readonly string ANIM_IDLE = "Idle";
    private readonly string ANIM_WALK = "Walk";
    private readonly string ANIM_RUN = "Run";
    private readonly string ANIM_ATTACK = "Attack";
    private readonly string ANIM_DEATH = "Death";

    // AI States
    private enum AIState
    {
        Wandering,
        Chasing,
        Attacking,
        Dead
    }
    private AIState currentState = AIState.Wandering;

    void Start()
    {
        // Initialize health
        currentHealth = maxHealth;

        // Get or add Rigidbody
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }
        }

        // Configure Rigidbody for gravity
        rb.useGravity = enableGravity;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Get or add NavMeshAgent
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
            if (agent == null)
            {
                agent = gameObject.AddComponent<NavMeshAgent>();
            }
        }

        // Configure NavMeshAgent
        agent.autoBraking = true;
        agent.updatePosition = true; // Keep true for normal movement
        agent.updateRotation = true;

        // Get or add Animator
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        // Set initial speed
        agent.speed = wanderSpeed;

        // Start wandering
        timer = wanderTimer;
    }

    void Update()
    {
        if (isDead) return;

        // Check if grounded and handle falling
        CheckGrounded();
        HandleFalling();

        // Only update AI behavior when grounded and not falling
        if (isGrounded && !isFalling)
        {
            // Update timer for wandering
            timer += Time.deltaTime;

            // State machine
            switch (currentState)
            {
                case AIState.Wandering:
                    WanderBehavior();
                    CheckForPlayer();
                    break;

                case AIState.Chasing:
                    ChaseBehavior();
                    break;

                case AIState.Attacking:
                    AttackBehavior();
                    break;
            }
        }

        // Update animations based on movement
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        // Apply custom gravity if needed
        if (enableGravity && !isGrounded && rb != null)
        {
            Vector3 gravity = Physics.gravity * gravityMultiplier;
            rb.AddForce(gravity, ForceMode.Acceleration);
        }
    }

    #region Movement Behaviors

    void WanderBehavior()
    {
        // Set a new random destination every wanderTimer seconds
        if (timer >= wanderTimer)
        {
            Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
            agent.SetDestination(newPos);
            timer = 0;
        }

        agent.speed = wanderSpeed;
    }

    void ChaseBehavior()
    {
        if (player == null)
        {
            currentState = AIState.Wandering;
            return;
        }

        // Check if player is still in detection range
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > detectionRange)
        {
            // Player escaped, return to wandering and deactivate
            currentState = AIState.Wandering;
            agent.speed = wanderSpeed;
            isActivated = false;
            player = null;
            return;
        }

        // Chase the player
        agent.SetDestination(player.position);
        agent.speed = chaseSpeed;

        // Check if in attack range
        if (distanceToPlayer <= attackRange)
        {
            currentState = AIState.Attacking;
            agent.isStopped = true;
        }
    }

    void AttackBehavior()
    {
        if (player == null)
        {
            currentState = AIState.Wandering;
            agent.isStopped = false;
            return;
        }

        // Look at player
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // If player moved out of attack range, chase again
        if (distanceToPlayer > attackRange)
        {
            currentState = AIState.Chasing;
            agent.isStopped = false;
            return;
        }

        // Attack if cooldown is ready
        if (Time.time >= nextAttackTime)
        {
            PerformAttack();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    void CheckForPlayer()
    {
        // Only start chasing if activated by player
        if (isActivated && player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= detectionRange)
            {
                currentState = AIState.Chasing;
            }
        }
    }

    Vector3 RandomNavSphere(Vector3 origin, float distance, int layermask)
    {
        Vector3 randomDirection = Random.insideUnitSphere * distance;
        randomDirection += origin;

        NavMeshHit navHit;
        NavMesh.SamplePosition(randomDirection, out navHit, distance, layermask);

        return navHit.position;
    }

    #endregion

    #region Combat

    void PerformAttack()
    {
        // Trigger attack animation
        if (animator != null)
        {
            animator.SetTrigger(ANIM_ATTACK);
        }

        // Deal damage to player
        if (player != null)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
                Debug.Log($"AI attacked player for {attackDamage} damage!");
            }
            else
            {
                Debug.LogWarning("Player doesn't have PlayerHealth component!");
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"AI took {damage} damage. Current health: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        currentState = AIState.Dead;

        // Stop movement
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        // Play death animation
        if (animator != null)
        {
            animator.SetTrigger(ANIM_DEATH);
        }

        // Disable collider
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        Debug.Log("AI died!");

        // Optional: Destroy after animation
        Destroy(gameObject, 3f);
    }

    /// <summary>
    /// Check if this AI is dead
    /// </summary>
    public bool IsDead()
    {
        return isDead;
    }

    #endregion

    #region Animations

    void UpdateAnimations()
    {
        if (animator == null || isDead) return;

        // Get current velocity
        float velocity = agent.velocity.magnitude;

        // Reset all animation states
        animator.SetBool(ANIM_IDLE, false);
        animator.SetBool(ANIM_WALK, false);
        animator.SetBool(ANIM_RUN, false);

        // Set animation based on state and velocity
        switch (currentState)
        {
            case AIState.Wandering:
                if (velocity > 0.1f)
                {
                    animator.SetBool(ANIM_WALK, true);
                }
                else
                {
                    animator.SetBool(ANIM_IDLE, true);
                }
                break;

            case AIState.Chasing:
                animator.SetBool(ANIM_RUN, true);
                break;

            case AIState.Attacking:
                animator.SetBool(ANIM_IDLE, true);
                // Attack animation is triggered in PerformAttack()
                break;
        }
    }

    #endregion

    #region Manual Activation

    /// <summary>
    /// Called by the player when they press the activation key near this AI
    /// </summary>
    /// <param name="playerTransform">The player's transform</param>
    public void ActivateByPlayer(Transform playerTransform)
    {
        if (isDead) return;

        player = playerTransform;
        isActivated = true;

        Debug.Log($"AI {gameObject.name} has been activated by the player!");

        // Immediately start chasing if in range
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= detectionRange)
        {
            currentState = AIState.Chasing;
        }
    }

    /// <summary>
    /// Deactivate the AI (return to wandering)
    /// </summary>
    public void Deactivate()
    {
        isActivated = false;
        player = null;
        currentState = AIState.Wandering;
        agent.speed = wanderSpeed;

        Debug.Log($"AI {gameObject.name} has been deactivated.");
    }

    /// <summary>
    /// Check if this AI is currently activated
    /// </summary>
    public bool IsActivated()
    {
        return isActivated;
    }

    #endregion

    #region Gravity & Ground Detection

    void CheckGrounded()
    {
        if (rb == null) return;

        // Check if grounded using raycast
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        isGrounded = Physics.Raycast(rayOrigin, Vector3.down, groundCheckDistance, groundLayer);

        // Debug visualization
        Debug.DrawRay(rayOrigin, Vector3.down * groundCheckDistance, isGrounded ? Color.green : Color.red);
    }

    void HandleFalling()
    {
        if (rb == null) return;

        // Check if we should be falling
        bool shouldBeFalling = !isGrounded && enableGravity;

        if (shouldBeFalling && !isFalling)
        {
            // Start falling
            StartFalling();
        }
        else if (!shouldBeFalling && isFalling)
        {
            // Land on ground
            LandOnGround();
        }
    }

    void StartFalling()
    {
        isFalling = true;
        
        // Completely disable NavMeshAgent during fall
        if (agent != null)
        {
            agent.enabled = false;
        }

        // Enable Rigidbody physics
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.FreezeRotation; // Only freeze rotation
        }

        Debug.Log("AI started falling!");
    }

    void LandOnGround()
    {
        isFalling = false;

        // Stop all Rigidbody movement
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // Re-enable NavMeshAgent after Rigidbody is stopped
        if (agent != null)
        {
            agent.enabled = true;
            agent.isStopped = false;
            
            // Wait a frame before setting destination to avoid conflicts
            StartCoroutine(SetNavMeshPositionAfterDelay());
        }

        Debug.Log("AI landed on ground!");
    }
    
    private System.Collections.IEnumerator SetNavMeshPositionAfterDelay()
    {
        yield return new WaitForEndOfFrame();
        
        if (agent != null && agent.enabled)
        {
            // Find nearest valid NavMesh position
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 1.0f, NavMesh.AllAreas))
            {
                transform.position = hit.position;
                agent.Warp(hit.position);
            }
            
            // Reset destination to current position
            agent.SetDestination(transform.position);
        }
    }

    #endregion

    #region Gizmos (for debugging in editor)

    private void OnDrawGizmosSelected()
    {
        // Draw wander radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);

        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Draw detection range
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Draw ground check
        if (enableGravity)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
            Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * groundCheckDistance);
        }
    }

    #endregion
}
