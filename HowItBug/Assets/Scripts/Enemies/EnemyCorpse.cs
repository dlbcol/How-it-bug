using System;
using UnityEngine;

public class EnemyCorpse : MonoBehaviour
{
    // TODO: Stop timer when player picks up corpse
    // TODO: Reset timer when player drops corpse

    public EnemyData Data;

    [SerializeField] private float toppleForce = 2f;

    public event Action<EnemyCorpse> OnCorpseDespawned;

    private Timer _timer;
    private Rigidbody _rigidbody;

    private void Awake()
    {
        _timer = GetComponent<Timer>();
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void Initialize()
    {
        _timer.StartTimer(Data.CorpseLifeTime);
        _timer.OnTimerFinished += CorpseDespawned;

        ToppleOver();
    }

    private void ToppleOver()
    {
        Vector3 toppleDirection = new Vector3(
            UnityEngine.Random.Range(-1f, 1f),
            0f,
            UnityEngine.Random.Range(-1f, 1f)
        ).normalized;

        _rigidbody.AddTorque(
            toppleDirection * toppleForce,
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