using System;
using UnityEngine;

public class EnemyCorpse : MonoBehaviour
{
    public EnemyData Data;

    public event Action<EnemyCorpse> OnCorpseDespawned;

    private Timer _timer;

    private void Awake()
    {
        _timer = GetComponent<Timer>();
    }

    public void Initialize()
    {
        _timer.StartTimer(Data.CorpseLifeTime);
        _timer.OnTimerFinished += CorpseDespawned;
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
