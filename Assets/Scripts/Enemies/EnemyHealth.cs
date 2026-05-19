using UnityEngine;

namespace ShadowDash.Enemies
{
    public sealed class EnemyHealth : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 3;
        [SerializeField] private bool destroyOnDeath = true;

        public bool IsAlive => currentHealth > 0;

        private int currentHealth;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(int damage)
        {
            if (!IsAlive || damage <= 0)
            {
                return;
            }

            currentHealth = Mathf.Max(currentHealth - damage, 0);

            if (currentHealth == 0)
            {
                Die();
            }
        }

        private void Die()
        {
            if (destroyOnDeath)
            {
                Destroy(gameObject);
                return;
            }

            gameObject.SetActive(false);
        }
    }
}
