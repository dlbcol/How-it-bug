using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject, IEntity
{
    [SerializeField] private string _name = string.Empty;
    [SerializeField] private float _maxHealth = 100;
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private GameObject _corpsePrefab;

    public string Name => _name;
    public float MaxHealth => _maxHealth;
    public GameObject EnemyPrefab => _enemyPrefab;
    public GameObject CorpsePrefab => _corpsePrefab;
}
