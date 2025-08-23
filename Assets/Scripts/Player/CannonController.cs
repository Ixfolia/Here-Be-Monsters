using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CannonController : MonoBehaviour
{
    // Cannon Settings
    [Header("Cannon Settings")]
    [SerializeField] private GameObject bulletPrefab;  // Assign your bullet prefab
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float fireRate = 0.2f;  // Time between shots
    
    [Header("Trail Effect")]
    [SerializeField] private GameObject trailParticlePrefab;  // Assign a simple particle prefab
    [SerializeField] private float trailEmissionRate = 10f;  // Particles per second
    [SerializeField] private float trailParticleLifetime = 0.5f;
    [SerializeField] private Color trailParticleColor = new Color(1f, 0.8f, 0.3f, 0.7f);
    [SerializeField] private Vector2 trailParticleSize = new Vector2(0.1f, 0.2f);
    [SerializeField] private float trailParticleSpeed = 2f;
    
    [Header("Aiming Line")]
    [SerializeField] private GameObject aimParticlePrefab;  // Assign a particle prefab for the aim line
    [SerializeField] private int aimLineParticles = 20;     // Number of particles in the aim line
    [SerializeField] private float aimLineLength = 5f;      // Length of the aim line
    [SerializeField] private Color aimLineColor = new Color(1f, 1f, 1f, 0.5f);  // Semi-transparent white
    private List<GameObject> aimParticles = new List<GameObject>();
    
    private Camera mainCamera;
    private float nextFireTime = 0f;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        AimAtMouse();
        
        // Handle aiming line when right mouse button is held
        if (Input.GetMouseButtonDown(1)) // Right mouse button down
        {
            CreateAimLine();
        }
        else if (Input.GetMouseButtonUp(1)) // Right mouse button released
        {
            ClearAimLine();
        }
        
        if (Input.GetMouseButton(1)) // While right mouse button is held
        {
            UpdateAimLine();
        }
        
        // Handle shooting
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + 1f / fireRate;
        }
    }

    private void AimAtMouse()
    {
        // Get the mouse position in world coordinates
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;  // Ensure z is 0 for 2D
        
        // Calculate direction to mouse
        Vector2 direction = (mousePosition - transform.position).normalized;
        
        // Calculate the angle in degrees
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // Apply rotation (adjust offset if needed based on your sprite's default orientation)
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle - 90f); // -90 if your cannon points up
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void Shoot()
    {
        if (bulletPrefab == null)
        {
            Debug.LogWarning("No bullet prefab assigned to the cannon!");
            return;
        }

        // Create bullet
        GameObject bullet = Instantiate(bulletPrefab, transform.position, transform.rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = transform.up * bulletSpeed;
            
            // Add trail effect to the bullet
            if (trailParticlePrefab != null)
            {
                // Create a container for the trail particles
                GameObject trailContainer = new GameObject("BulletTrail");
                trailContainer.transform.SetParent(bullet.transform);
                trailContainer.transform.localPosition = Vector3.zero;
                
                // Add and configure the trail particle system
                var trailSystem = trailContainer.AddComponent<TrailParticles>();
                trailSystem.Initialize(
                    bullet.transform,
                    trailParticlePrefab,
                    trailEmissionRate,
                    trailParticleLifetime,
                    trailParticleColor,
                    trailParticleSize,
                    trailParticleSpeed
                );
                
                // Clean up when the bullet is destroyed
                Destroy(trailContainer, trailParticleLifetime * 2f);
            }
        }
        else
        {
            Debug.LogWarning("Bullet prefab is missing a Rigidbody2D component!");
        }
    }
    
// Add this new class at the end of the file
[System.Serializable]
public class TrailParticles : MonoBehaviour
{
    private Transform followTarget;
    private GameObject particlePrefab;
    private float emissionRate;
    private float particleLifetime;
    private Color particleColor;
    private Vector2 particleSize;
    private float particleSpeed;
    
    private float timeSinceLastEmission;
    private float timeBetweenParticles;
    
    public void Initialize(Transform target, GameObject prefab, float rate, float lifetime, Color color, Vector2 size, float speed)
    {
        followTarget = target;
        particlePrefab = prefab;
        emissionRate = rate;
        particleLifetime = lifetime;
        particleColor = color;
        particleSize = size;
        particleSpeed = speed;
        
        timeBetweenParticles = 1f / emissionRate;
        timeSinceLastEmission = 0f;
    }
    
    private void Update()
    {
        if (followTarget == null) return;
        
        timeSinceLastEmission += Time.deltaTime;
        
        if (timeSinceLastEmission >= timeBetweenParticles)
        {
            timeSinceLastEmission = 0f;
            EmitParticle();
        }
    }
    
    private void EmitParticle()
    {
        if (particlePrefab == null) return;
        
        // Create particle at the bullet's position
        GameObject particle = Instantiate(particlePrefab, followTarget.position, Quaternion.identity);
        
        // Set up particle properties
        SpriteRenderer sr = particle.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = particleColor;
            float size = Random.Range(particleSize.x, particleSize.y);
            particle.transform.localScale = new Vector3(size, size, 1f);
        }
        
        // Add physics
        Rigidbody2D rb = particle.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Make the particle move in the opposite direction of the bullet
            Vector2 direction = -followTarget.up;
            rb.linearVelocity = direction * particleSpeed;
        }
        
        // Destroy after lifetime
        Destroy(particle, particleLifetime);
    }
}
}
