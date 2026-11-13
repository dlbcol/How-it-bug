using System;
using Unity.Netcode;
using UnityEngine;

public class EnemyController : NetworkBehaviour
{

    public EnemyData Data;
    public event Action<EnemyController> OnDeath;

    private Health _health;
    private NetworkObject _networkObject;

    private void Awake()
    {
        _health = GetComponent<Health>();
        _networkObject = GetComponent<NetworkObject>();

        _health.OnDeath += EnemyDied;
    }

    public void Spawn()
    {
        gameObject.SetActive(true);
        _networkObject.Spawn();
        _health.Initialize(Data);
    }

    public void Despawn()
    {
        if (!IsServer) return;
        _networkObject.Despawn(false);
        gameObject.SetActive(false);
    }

    private void EnemyDied()
    {
        if (!IsServer) return;
        OnDeath?.Invoke(this);
    }

}
