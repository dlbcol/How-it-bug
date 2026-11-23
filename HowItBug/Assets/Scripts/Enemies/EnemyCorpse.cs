using System;
using UnityEngine;
using Unity.Netcode;

public class EnemyCorpse : NetworkBehaviour
{
    // TODO: Stop timer when player picks up corpse
    // TODO: Reset timer when player drops corpse

    public EnemyData Data;
    public event Action<EnemyCorpse> OnCorpseDespawned;

    [SerializeField] private float toppleForce = 2f;

    private Timer _timer;
    private Rigidbody _rigidbody;
    private NetworkObject _networkObject;

    private void Awake()
    {
        _timer = GetComponent<Timer>();
        _rigidbody = GetComponent<Rigidbody>();
        _networkObject = GetComponent<NetworkObject>();
    }

    public void Spawn()
    {
        gameObject.SetActive(true);
        _networkObject.Spawn();

        _timer.StartTimer(Data.CorpseLifeTime);
        _timer.OnTimerFinished += CorpseDespawned;
        ToppleOver();
    }

    public void Despawn()
    {
        if (!IsServer) return;

        _networkObject.Despawn(false);
        gameObject.SetActive(false);
    }

    /// <summary>
    /// DEBUG: For testing purposes, pressing the Y key will apply a force to the corpse's rigidbody
    /// TODO: Remove this method when the corpse is fully implemented and tested
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            _rigidbody.AddForce(Vector3.forward * 10f, ForceMode.Impulse);
        }
    }

    /// <summary>
    /// Applies different amounts of force and torque to the corpse's rigidbody to make it topple over in a random direction.
    /// TODO: Make this more realistic by using the enemy's last movement direction and the player's position to determine the topple direction.
    /// </summary>
    private void ToppleOver()
    {
        Vector3 toppleDirection = new Vector3(
            UnityEngine.Random.Range(-1f, 1f),
            0f,
            UnityEngine.Random.Range(-1f, 1f)
        ).normalized;

        _rigidbody.AddForce(
            toppleDirection * 2f + Vector3.up * 0.5f,
            ForceMode.Impulse
        );

        _rigidbody.AddTorque(
            toppleDirection * 5f,
            ForceMode.Impulse
        );
    }

    public void StopLifeTimer()
    {
        _timer.StopTimer();
    }

    public void ResetLifeTimer()
    {
        _timer.StartTimer(Data.CorpseLifeTime);
    }

    private void CorpseDespawned()
    {
        _timer.OnTimerFinished -= CorpseDespawned;
        OnCorpseDespawned?.Invoke(this);
    }
}