using UnityEngine;

public class TMPspawner : MonoBehaviour
{
    [SerializeField] private EnemyManager enemyManager;
    [SerializeField] private EnemyData enemyData;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            enemyManager.TestSpawn(enemyData);
        }
    }
}