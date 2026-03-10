using UnityEngine;

public class LaserScript : MonoBehaviour
{
    #region Settings and Components
    [Header("Settings")]
    [SerializeField] private float speed = 20f;  // Movement speed of the laser

    [Header("Components")]
    [SerializeField] private Rigidbody2D rb;     // Reference to the physics component
    #endregion

    #region Laser Behavior
    public void InitializeDirection(Vector2 targetPosition)
    {
        // Calculate direction towards target and apply velocity
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * speed;
    }
    #endregion

    #region Collision & Trigger Events
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Destroy laser when colliding with UFO
        if (collision.gameObject.CompareTag("Ufo"))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Destroy laser when leaving the allowed shooting area
        if (other.CompareTag("GameViewArea"))
        {
            Destroy(gameObject);
        }
    }
    #endregion
}