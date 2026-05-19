using ShadowDash.Enemies;
using UnityEngine;

namespace ShadowDash.Combat
{
    public sealed class EnemyTargeting : MonoBehaviour
    {
        public EnemyHealth FindNearestEnemy(Vector2 origin, float range)
        {
            EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsInactive.Exclude);
            EnemyHealth nearestEnemy = null;
            float nearestSqrDistance = range * range;

            foreach (EnemyHealth enemy in enemies)
            {
                if (!enemy.IsAlive)
                {
                    continue;
                }

                float sqrDistance = ((Vector2)enemy.transform.position - origin).sqrMagnitude;
                if (sqrDistance <= nearestSqrDistance)
                {
                    nearestEnemy = enemy;
                    nearestSqrDistance = sqrDistance;
                }
            }

            return nearestEnemy;
        }
    }
}
