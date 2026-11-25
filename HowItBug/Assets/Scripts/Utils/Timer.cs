using System;
using System.Collections;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public event Action OnTimerFinished;

    private Coroutine _timerCoroutine;

    public void StartTimer(float duration)
    {
        StopTimer();
        _timerCoroutine = StartCoroutine(TimerRoutine(duration));
    }

    public void StopTimer()
    {
        if (_timerCoroutine != null)
        {
            StopCoroutine(_timerCoroutine);
            _timerCoroutine = null;
        }
    }

    private IEnumerator TimerRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);

        _timerCoroutine = null;
        OnTimerFinished?.Invoke();
    }

}
