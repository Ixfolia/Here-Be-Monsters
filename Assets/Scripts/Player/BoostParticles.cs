using UnityEngine;

public class BoostParticles : MonoBehaviour
{
    [SerializeField] private ShipMovement shipMovement;
    [SerializeField] private GameObject particlePrefab;
    [SerializeField] private Transform[] emissionPoints; // Assign points where particles should spawn
    [SerializeField] private float emissionRate = 10f; // Particles per second
    [SerializeField] private float particleLifetime = 0.5f;
    [Header("Particle Movement")]
    [SerializeField] private float particleSpeed = 3f;
    [SerializeField] private float speedVariation = 0.5f;
    [SerializeField] private float spreadAngle = 30f; // Degrees of spread for particle direction
    [SerializeField] private float maxLifetimeVariation = 0.2f; // Random variation in particle lifetime
    
    [Header("Particle Appearance")]
    [SerializeField] private Color particleColor = new Color(1f, 0.8f, 0.3f, 1f); // Orange-yellow color
    [SerializeField] private Vector2 particleSizeRange = new Vector2(0.1f, 0.3f);
    [SerializeField] private bool fadeOutOverLifetime = true;

    private float timeSinceLastEmission = 0f;
    private float timeBetweenParticles;

    private void Start()
    {
        if (shipMovement == null)
            shipMovement = GetComponent<ShipMovement>();
            
        timeBetweenParticles = 1f / emissionRate;
        
        // If no emission points are assigned, create one at the back of the ship
        if (emissionPoints == null || emissionPoints.Length == 0)
        {
            GameObject point = new GameObject("EmissionPoint");
            point.transform.SetParent(transform);
            point.transform.localPosition = new Vector3(0, -0.5f, 0); // Adjust based on your ship's size
            emissionPoints = new Transform[] { point.transform };
        }
    }

    private void Update()
    {
        if (shipMovement == null || !shipMovement.IsBoosting) return;

        timeSinceLastEmission += Time.deltaTime;

        if (timeSinceLastEmission >= timeBetweenParticles)
        {
            timeSinceLastEmission = 0f;
            EmitParticles();
        }
    }

    private void EmitParticles()
    {
        if (emissionPoints == null || emissionPoints.Length == 0) return;
        
        foreach (var point in emissionPoints)
        {
            if (point == null) continue;
            
            if (particlePrefab == null)
                CreateDefaultParticle(point);
            else
            {
                GameObject particle = Instantiate(particlePrefab, point.position, point.rotation);
                Destroy(particle, particleLifetime);
            }
        }
    }

    private void CreateDefaultParticle(Transform emissionPoint)
    {
        if (emissionPoint == null) return;
        
        GameObject particle = new GameObject("BoostParticle");
        try
        {
            // Random position around the emission point
            Vector2 randomOffset = Random.insideUnitCircle * 0.1f; // Small random offset from emission point
            particle.transform.position = (Vector2)emissionPoint.position + randomOffset;
            
            // Random rotation (facing away from ship with some spread)
            float randomAngle = Random.Range(-spreadAngle * 0.5f, spreadAngle * 0.5f);
            particle.transform.rotation = Quaternion.Euler(0, 0, emissionPoint.eulerAngles.z + 180f + randomAngle);
            
            // Add and configure sprite renderer
            SpriteRenderer sr = particle.AddComponent<SpriteRenderer>();
            sr.sprite = CreateDefaultSprite();
            sr.color = particleColor;
            
            // Add rigidbody for movement
            Rigidbody2D rb = particle.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            
            // Randomize speed and direction
            float randomSpeed = Random.Range(particleSpeed * (1f - speedVariation), particleSpeed * (1f + speedVariation));
            Vector2 direction = Quaternion.Euler(0, 0, randomAngle) * -emissionPoint.up;
            rb.linearVelocity = direction * randomSpeed;
            
            // Random size
            float size = Random.Range(particleSizeRange.x, particleSizeRange.y);
            particle.transform.localScale = new Vector3(size, size, 1f);
            
            // Add a self-destruct component with random lifetime variation
            var selfDestruct = particle.AddComponent<SelfDestruct>();
            float lifetimeVariation = Random.Range(-maxLifetimeVariation, maxLifetimeVariation);
            selfDestruct.lifetime = Mathf.Max(0.1f, particleLifetime + lifetimeVariation);
            
            // Add fade out component if enabled
            if (fadeOutOverLifetime)
            {
                var fadeComponent = particle.AddComponent<FadeComponent>();
                fadeComponent.Initialize(sr, selfDestruct.lifetime * 0.7f);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error creating particle: {e.Message}");
            Destroy(particle);
        }
    }

    private Sprite CreateDefaultSprite()
    {
        // Create a simple white square texture
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        
        // Create a sprite from the texture
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
    }

    // Nested class to handle fade effect
    private class FadeComponent : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;
        private float fadeDuration;
        private float fadeTimer = 0f;
        private Color initialColor;
        private bool isFading = false;

        public void Initialize(SpriteRenderer renderer, float duration)
        {
            spriteRenderer = renderer;
            fadeDuration = duration;
            initialColor = spriteRenderer.color;
            isFading = true;
        }

        private void Update()
        {
            if (!isFading) return;
            
            fadeTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(fadeTimer / fadeDuration);
            
            // Update alpha based on progress
            if (spriteRenderer != null)
            {
                Color newColor = spriteRenderer.color;
                newColor.a = Mathf.Lerp(initialColor.a, 0f, progress);
                spriteRenderer.color = newColor;
            }
            
            // Destroy this component when fade is complete
            if (progress >= 1f)
            {
                isFading = false;
                Destroy(this);
            }
        }
    }
}
