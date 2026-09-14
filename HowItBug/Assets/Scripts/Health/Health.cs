using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    public IEntity Entity { get; private set; }
    public float CurrentHealth { get; private set; }
    public bool IsDead => CurrentHealth <= 0;

    public event Action OnDeath;
    public event Action<float, IEntity> OnHealthChanged;

    public void Initialize(IEntity entity)
    {
        Entity = entity;
        CurrentHealth = entity.MaxHealth;
        NotifyHealthChanged();
    }

    public void TakeDamage(float damage)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth - damage, 0, Entity.MaxHealth);
        NotifyHealthChanged();

        if (CurrentHealth <= 0)
        {
            OnDeath?.Invoke();
        }
    }

    private void NotifyHealthChanged() => OnHealthChanged?.Invoke(CurrentHealth, Entity);


}
