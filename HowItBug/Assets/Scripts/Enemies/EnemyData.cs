using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject, IEntity
{
    [Header("Enemy Base Settings")]
    [SerializeField] private string _name = string.Empty;
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private GameObject _enemyPrefab;

    [Header("Corpse settings")]
    [SerializeField] private float _corpseLifeTime = 300f;
    [SerializeField] private float _corpsePrice = 50f;
    [SerializeField] private GameObject _corpsePrefab;

    public string Name => _name;
    public float MaxHealth => _maxHealth;
    public GameObject EnemyPrefab => _enemyPrefab;

    public float CorpseLifeTime => _corpseLifeTime;
    public float CorpsePrice => _corpsePrice;
    public GameObject CorpsePrefab => _corpsePrefab;
}
