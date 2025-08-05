using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    public float lifetime = 1f;
    private float timer = 0f;

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
