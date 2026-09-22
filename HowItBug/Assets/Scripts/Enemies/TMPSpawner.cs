using UnityEngine;

public class TMPspawner : MonoBehaviour
{
    [SerializeField] private Health enemyHealth;
    [SerializeField] private EnemyManager enemyManager;
    [SerializeField] private EnemyData enemyData;

    /// <summary>
    /// Temp spawner for testing purposes. Press 'T' to deal damage to the enemy and 'G' to spawn a new enemy.
    /// TODO: Remove the gameobject that has this script when the enemy spawning system is fully implemented.
    /// </summary>
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(10);
            }
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            enemyManager.TestSpawn(enemyData);

            EnemyController enemy = FindAnyObjectByType<EnemyController>();

            if (enemy != null)
            {
                enemyHealth = enemy.GetComponent<Health>();
            }
        }
    }
}