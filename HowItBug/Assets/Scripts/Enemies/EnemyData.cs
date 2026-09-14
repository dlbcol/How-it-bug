using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject, IEntity
{
    [SerializeField] private string _name = string.Empty;
    [SerializeField] private float _maxHealth = 100;

    public string Name => _name;
    public float MaxHealth => _maxHealth;
}
