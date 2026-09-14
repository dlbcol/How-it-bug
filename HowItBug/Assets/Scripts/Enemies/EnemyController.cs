using System;
using UnityEngine;

public class EnemyController : MonoBehaviour
{

    public EnemyData Data => _data;
    public event Action<EnemyController> OnDeath;

    [SerializeField] private EnemyData _data;


    private Health _health;

    private void Awake()
    {
        _health = GetComponent<Health>();

        _health.OnDeath += EnemyDied;
    }

    private void EnemyDied() => OnDeath?.Invoke(this);

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        _health.Initialize(_data);
    }

}
