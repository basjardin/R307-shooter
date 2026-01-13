using System.Collections;

namespace UnityEngine.XR.Content.Interaction
{
    /// <summary>
    /// Apply forward force to instantiated prefab
    /// </summary>
    public class LaunchProjectile : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The projectile that's created")]
        GameObject m_ProjectilePrefab = null;

        [SerializeField]
        [Tooltip("The point that the project is created")]
        Transform m_StartPoint = null;

        [SerializeField]
        [Tooltip("The speed at which the projectile is launched")]
        float m_LaunchSpeed = 1.0f;

        [SerializeField]
        [Tooltip("If true, the weapon will fire automatically while StartFiring is active")]
        bool m_Automatic = false;

        [SerializeField]
        [Tooltip("Number of projectiles fired per second when automatic is enabled")]
        float m_FireRate = 5.0f;

        [SerializeField]
        [Tooltip("Audio clip to play when firing a projectile")]
        AudioClip m_FireAudioClip = null;

        [SerializeField]
        [Tooltip("Damage value for each projectile")]
        float m_ProjectileDamage = 10.0f;

        bool m_IsFiring;
        float m_NextFireTime;

        public void Fire()
        {
            // Enforce fire rate cooldown so spamming can't exceed the configured rate
            float interval = 1f / Mathf.Max(0.0001f, m_FireRate);
            if (Time.time < m_NextFireTime)
                return;

            GameObject newObject = Instantiate(m_ProjectilePrefab, m_StartPoint.position, m_StartPoint.rotation, null);

            if (newObject.TryGetComponent(out Rigidbody rigidBody))
                ApplyForce(rigidBody);

            // Add AudioSource to projectile and play fire audio
            if (m_FireAudioClip != null)
            {
                AudioSource projectileAudioSource = newObject.AddComponent<AudioSource>();
                projectileAudioSource.PlayOneShot(m_FireAudioClip);
            }

            // Add damage stat to projectile
            newObject.AddComponent<ProjectileDamage>().SetDamage(m_ProjectileDamage);

            m_NextFireTime = Time.time + interval;
        }

        /// <summary>
        /// Begin firing. If automatic mode is enabled this will start repeated firing at the configured rate,
        /// otherwise it will just fire a single projectile.
        /// </summary>
        public void StartFiring()
        {
            m_IsFiring = true;
            if (m_Automatic)
            {
                // Try to fire immediately when starting to hold (if cooldown allows)
                if (Time.time >= m_NextFireTime)
                    Fire();
            }
            else
            {
                Fire();
            }
        }

        /// <summary>
        /// Stop automatic firing (if active).
        /// </summary>
        public void StopFiring()
        {
            m_IsFiring = false;
        }

        void Update()
        {
            // Handle automatic firing while the button is held
            if (m_IsFiring && m_Automatic)
            {
                if (Time.time >= m_NextFireTime)
                    Fire();
            }
        }

        /// <summary>
        /// Set the fire rate (shots per second). Value is clamped to >= 0.
        /// </summary>
        public void SetFireRate(float fireRate)
        {
            m_FireRate = Mathf.Max(0f, fireRate);
        }

        /// <summary>
        /// Enable or disable automatic firing. Disabling will stop any active automatic firing.
        /// </summary>
        public void SetAutomatic(bool automatic)
        {
            m_Automatic = automatic;
            if (!m_Automatic)
                StopFiring();
        }

        void ApplyForce(Rigidbody rigidBody)
        {
            Vector3 force = m_StartPoint.forward * m_LaunchSpeed;
            rigidBody.AddForce(force);
        }

        /// <summary>
        /// Set the damage value for projectiles. Value is clamped to >= 0.
        /// </summary>
        public void SetProjectileDamage(float damage)
        {
            m_ProjectileDamage = Mathf.Max(0f, damage);
        }
    }

    /// <summary>
    /// Component that stores damage information for a projectile
    /// </summary>
    public class ProjectileDamage : MonoBehaviour
    {
        float m_Damage = 10.0f;

        public void SetDamage(float damage)
        {
            m_Damage = damage;
        }

        public float GetDamage()
        {
            return m_Damage;
        }
    }
}
