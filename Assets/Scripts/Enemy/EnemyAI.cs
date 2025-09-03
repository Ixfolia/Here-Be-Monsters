using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;  // Assign in inspector or it will try to find automatically
    
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float stoppingDistance = 1f;
    private Vector2 startPosition;
    private bool isPlayerInRange;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        startPosition = transform.position;
        
        // Try to find player if not assigned in inspector
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogError("No player found! Make sure your player has the 'Player' tag.", this);
                enabled = false; // Disable the script to prevent further errors
                return;
            }
        }
    }

    private void Update()
    {
        // If player is destroyed during gameplay
        if (player == null)
        {
            // Try to find player again
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
            else
                return;
        }
        
        // Check if player is in detection radius
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        isPlayerInRange = distanceToPlayer <= detectionRadius;
        
        if (isPlayerInRange)
        {
            // Move towards player but maintain stopping distance
            if (distanceToPlayer > stoppingDistance)
            {
                Vector2 direction = (player.position - transform.position).normalized;
                rb.linearVelocity = direction * moveSpeed;
                
                // Flip sprite based on movement direction
                if (direction.x > 0)
                    spriteRenderer.flipX = false;
                else if (direction.x < 0)
                    spriteRenderer.flipX = true;
            }
            else
            {
                // Stop moving when in stopping distance
                rb.linearVelocity = Vector2.zero;
            }
        }
        else
        {
            // Optional: Add idle/wander behavior here
            rb.linearVelocity = Vector2.zero;
        }
    }
    
    // Visualize detection radius in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        if (isPlayerInRange && player != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
    
    // Handle collision with player (you can expand this for damage)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Implement player damage or other effects here
            Debug.Log("Player hit!");
        }
    }
}
