using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

public class EnemyManager : NetworkBehaviour
{
    [SerializeField] private List<EnemyData> _enemiesData = new();
    [SerializeField] private int _poolSize = 10;

    [SerializeField] private GameObject _enemiesContainer;
    [SerializeField] private GameObject _corpsesContainer;

    private Dictionary<EnemyData, Queue<GameObject>> _enemiesPool = new();
    private Dictionary<EnemyData, Queue<GameObject>> _corpsesPool = new();

    private HashSet<EnemyController> _activeEnemies = new();

    public void TestSpawn(EnemyData enemyData)
    {
        SpawnEnemy(enemyData, new(0, 0.5f, 0));
    }

    public override void OnNetworkSpawn()
    {
        InitializePool();
    }

    private void InitializePool()
    {
        if (!IsServer) return;

        foreach(EnemyData enemyData in _enemiesData)
        {
            Queue<GameObject> enemies = new();
            Queue<GameObject> corpses = new();

            for(int i = 0; i < _poolSize;  i++)
            {
                GameObject enemy = Instantiate(enemyData.EnemyPrefab, _enemiesContainer.transform);
                enemy.SetActive(false);
                enemies.Enqueue(enemy);

                GameObject corpse = Instantiate(enemyData.CorpsePrefab, _corpsesContainer.transform);
                corpse.SetActive(false);
                corpses.Enqueue(corpse);
            }

            _enemiesPool.Add(enemyData, enemies);
            _corpsesPool.Add(enemyData, corpses);
        }
    }

    public void SpawnEnemy(EnemyData enemyData, Vector3 position)
    {
        if (!IsServer) return;
        GameObject enemy;
        if (_enemiesPool[enemyData].Any())
        {
            enemy = _enemiesPool[enemyData].Dequeue();
        }
        else
        {
            enemy = Instantiate(enemyData.EnemyPrefab, _enemiesContainer.transform);
        }

        enemy.transform.position = position;
        EnemyController controller = enemy.GetComponent<EnemyController>();
        _activeEnemies.Add(controller);

        controller.OnDeath += OnEnemyDeath;
        controller.Spawn();
    }

    public void OnEnemyDeath(EnemyController controller)
    {
        controller.OnDeath -= OnEnemyDeath;
        _activeEnemies.Remove(controller);

        controller.Despawn();
        _enemiesPool[controller.Data].Enqueue(controller.gameObject);

        SpawnCorpse(controller.Data, controller.gameObject.transform.position);
    }

    public void SpawnCorpse(EnemyData enemyData, Vector3 position)
    {
        if(!IsServer) return;
        GameObject corpse;
        if (_corpsesPool[enemyData].Any())
        {
            corpse = _corpsesPool[enemyData].Dequeue();
        }
        else
        {
            corpse = Instantiate(enemyData.CorpsePrefab, _corpsesContainer.transform);
        }

        corpse.transform.position = position;
        EnemyCorpse corpseController = corpse.GetComponent<EnemyCorpse>();
        corpseController.Spawn();
        corpseController.OnCorpseDespawned += OnCorpseDespawned;
    }

    public void OnCorpseDespawned(EnemyCorpse corpse)
    {
        corpse.OnCorpseDespawned -= OnCorpseDespawned;
        corpse.Despawn();
        _corpsesPool[corpse.Data].Enqueue(corpse.gameObject);
    }

}
