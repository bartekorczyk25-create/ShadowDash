using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShadowDash.Player
{
    public sealed class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 5;
        [SerializeField] private float invulnerabilityDuration = 0.35f;
        [SerializeField] private bool reloadSceneOnDeath = true;

        public bool IsAlive => currentHealth > 0;

        private int currentHealth;
        private float invulnerabilityRemaining;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        private void Update()
        {
            if (invulnerabilityRemaining > 0f)
            {
                invulnerabilityRemaining -= Time.deltaTime;
            }
        }

        public void TakeDamage(int damage)
        {
            if (!IsAlive || damage <= 0 || invulnerabilityRemaining > 0f)
            {
                return;
            }

            currentHealth = Mathf.Max(currentHealth - damage, 0);
            invulnerabilityRemaining = invulnerabilityDuration;

            if (currentHealth == 0)
            {
                Die();
            }
        }

        private void Die()
        {
            if (reloadSceneOnDeath)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                return;
            }

            gameObject.SetActive(false);
        }
    }
}
