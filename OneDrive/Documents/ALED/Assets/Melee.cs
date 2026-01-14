using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Melee : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float meleeDamage = 20f;
    [SerializeField] private float attackCooldown = 0.5f;

    [Header("Hitbox Settings")]
    [SerializeField] private Collider damageHitbox;
    
    [Header("References")]
    [SerializeField] private Animator animator;
    private Rigidbody rb;

    // Private variables
    private Dictionary<GameObject, float> enemyCooldowns = new Dictionary<GameObject, float>();

    private HashSet<GameObject> hitEnemiesThisAttack = new HashSet<GameObject>();

    // Animation parameter names
    private readonly string ANIM_ATTACK = "Attack";

    void Start()
    {
        // Get the damage hitbox collider if not assigned
        // if (damageHitbox == null)
        // {
        //     damageHitbox = GetComponent<Collider>();
        //     if (damageHitbox != null)
        //     {
        //         damageHitbox.isTrigger = true;
        //     }
        // }

        // Get animator if not assigned
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        // Make sure hitbox is a trigger
        // if (damageHitbox != null)
        // {
        //     damageHitbox.isTrigger = true;
        //     damageHitbox.enabled = true;
        //     Debug.Log($"[Melee] Hitbox found and configured: {damageHitbox.name}, isTrigger: {damageHitbox.isTrigger}, enabled: {damageHitbox.enabled}");
        // }
        // else
        // {
        //     Debug.LogError("[Melee] CRITICAL: Damage hitbox not found! This script needs a Collider component!");
        // }

        // Get or create Rigidbody for trigger detection
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            Debug.Log("[Melee] Rigidbody created");
        }
        
        // Configure Rigidbody for trigger detection
       
        rb.mass = 1f;
        // Don't freeze all - only freeze rotation
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        
        Debug.Log($"[Melee] Rigidbody configured: isKinematic: {rb.isKinematic}, useGravity: {rb.useGravity}");
        Debug.Log($"[Melee] Setup complete! Ready to detect collisions.");
    }

    void Update()
    {
        // VR: La collision avec l'ennemi déclenche automatiquement l'attaque
    }

    void OnTriggerStay(Collider other)
    {
        // En VR, on détecte simplement si le couteau touche l'ennemi
        AI_people aiEnemy = other.GetComponent<AI_people>();
        if (aiEnemy == null) return;

        // Vérifier le cooldown pour cet ennemi spécifique
        if (enemyCooldowns.ContainsKey(other.gameObject) && Time.time < enemyCooldowns[other.gameObject])
        {
            return; // Cet ennemi est toujours en cooldown
        }

        // Appliquer les dégâts
        aiEnemy.TakeDamage(meleeDamage);
        Debug.Log($"[Melee] HIT! Enemy hit for {meleeDamage} damage!");

        // Ajouter/mettre à jour le cooldown pour cet ennemi
        enemyCooldowns[other.gameObject] = Time.time + attackCooldown;
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log($"[Melee] OnTriggerExit: {other.gameObject.name}");
    }

    /// <summary>
    /// Set the damage value for melee attacks
    /// </summary>
    public void SetMeleeDamage(float damage)
    {
        meleeDamage = damage;
    }

    /// <summary>
    /// Get the current melee damage value
    /// </summary>
    public float GetMeleeDamage()
    {
        return meleeDamage;
    }

    /// <summary>
    /// Debug method to check current state
    /// </summary>
    // public void DebugStatus()
    // {
    //     Debug.Log("[Melee DEBUG STATUS]");
    //     Debug.Log($"  - damageHitbox exists: {damageHitbox != null}");
    //     if (damageHitbox != null)
    //     {
    //         Debug.Log($"    - name: {damageHitbox.gameObject.name}");
    //         Debug.Log($"    - isTrigger: {damageHitbox.isTrigger}");
    //         Debug.Log($"    - enabled: {damageHitbox.enabled}");
    //         Debug.Log($"    - type: {damageHitbox.GetType().Name}");
    //     }
    //     Debug.Log($"  - rb exists: {rb != null}");
    //     if (rb != null)
    //     {
    //         Debug.Log($"    - isKinematic: {rb.isKinematic}");
    //         Debug.Log($"    - useGravity: {rb.useGravity}");
    //     }

    //     Debug.Log($"  - meleeDamage: {meleeDamage}");
    // }
}
