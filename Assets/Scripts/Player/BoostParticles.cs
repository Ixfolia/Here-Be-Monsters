using UnityEngine;

public class BoostParticles : MonoBehaviour
{
    [SerializeField] private ShipMovement shipMovement;
    [SerializeField] private GameObject particlePrefab;
    [SerializeField] private Transform[] emissionPoints; // Assign points where particles should spawn
    [SerializeField] private float emissionRate = 10f; // Particles per second
    [SerializeField] private float particleLifetime = 0.5f;
    [SerializeField] private float particleSpeed = 3f;
    [SerializeField] private Color particleColor = new Color(1f, 0.8f, 0.3f, 1f); // Orange-yellow color
    [SerializeField] private Vector2 particleSizeRange = new Vector2(0.1f, 0.3f);

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
            particle.transform.position = emissionPoint.position;
            particle.transform.rotation = emissionPoint.rotation * Quaternion.Euler(0, 0, 180f); // Point away from ship
            
            // Add and configure sprite renderer
            SpriteRenderer sr = particle.AddComponent<SpriteRenderer>();
            sr.sprite = CreateDefaultSprite();
            sr.color = particleColor;
            
            // Add rigidbody for movement
            Rigidbody2D rb = particle.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.linearVelocity = -emissionPoint.up * particleSpeed;
            
            // Set random size
            float size = Random.Range(particleSizeRange.x, particleSizeRange.y);
            particle.transform.localScale = new Vector3(size, size, 1f);
            
            // Add a self-destruct component
            var selfDestruct = particle.AddComponent<SelfDestruct>();
            selfDestruct.lifetime = particleLifetime;
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
}
