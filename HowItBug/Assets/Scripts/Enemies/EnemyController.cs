using System;
using UnityEngine;

public class EnemyController : MonoBehaviour
{

    public EnemyData Data;
    public event Action<EnemyController> OnDeath;

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
        _health.Initialize(Data);
    }

}
