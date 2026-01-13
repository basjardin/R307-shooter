using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("UI References (Optional)")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private Text healthText;

    [Header("Death Settings")]
    [SerializeField] private bool respawnOnDeath = true;
    [SerializeField] private Vector3 respawnPosition = Vector3.zero;
    [SerializeField] private float respawnDelay = 3f;

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"Player took {damage} damage. Current health: {currentHealth}/{maxHealth}");

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"Player healed {amount}. Current health: {currentHealth}/{maxHealth}");

        UpdateHealthUI();
    }

    void Die()
    {
        isDead = true;
        Debug.Log("Player died!");

        // Disable player controls
        FirstPersonController controller = GetComponent<FirstPersonController>();
        if (controller != null)
        {
            controller.enabled = false;
        }

        // Optional: Show death screen, play animation, etc.

        if (respawnOnDeath)
        {
            Invoke(nameof(Respawn), respawnDelay);
        }
    }

    void Respawn()
    {
        isDead = false;
        currentHealth = maxHealth;

        // Teleport to respawn position
        CharacterController charController = GetComponent<CharacterController>();
        if (charController != null)
        {
            charController.enabled = false;
            transform.position = respawnPosition;
            charController.enabled = true;
        }
        else
        {
            transform.position = respawnPosition;
        }

        // Re-enable player controls
        FirstPersonController controller = GetComponent<FirstPersonController>();
        if (controller != null)
        {
            controller.enabled = true;
        }

        UpdateHealthUI();

        Debug.Log("Player respawned!");
    }

    void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            healthBar.value = currentHealth / maxHealth;
        }

        if (healthText != null)
        {
            healthText.text = $"{currentHealth:F0} / {maxHealth:F0}";
        }
    }

    // Public getters
    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    public bool IsDead()
    {
        return isDead;
    }

    // Set respawn position (call this at checkpoints)
    public void SetRespawnPosition(Vector3 position)
    {
        respawnPosition = position;
    }
}
