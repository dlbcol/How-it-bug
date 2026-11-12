using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour
{

    public NetworkVariable<float> CurrentHealth = new(0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public NetworkVariable<float> MaxHealth = new(0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    public NetworkVariable<string> EntityName = new(string.Empty,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public IEntity Entity { get; private set; }
    public bool IsDead => CurrentHealth.Value <= 0f;

    public event Action OnDeath;
    public event Action<float, IEntity> OnHealthChanged;

    public override void OnNetworkSpawn()
    {
        CurrentHealth.OnValueChanged += NotifyHealthChanged;
    }

    public override void OnNetworkDespawn()
    {
        CurrentHealth.OnValueChanged -= NotifyHealthChanged;
    }

    public void Initialize(IEntity entity)
    {
        Entity = entity;
        CurrentHealth.Value = entity.MaxHealth;
        MaxHealth.Value = entity.MaxHealth;
        EntityName.Value = entity.Name;
    }

    public void TakeDamage(float damage)
    {
        if (!IsServer) return;
        if (IsDead) return;

        CurrentHealth.Value = Mathf.Clamp(CurrentHealth.Value - damage, 0, Entity.MaxHealth);

        if (CurrentHealth.Value <= 0)
        {
            OnDeath?.Invoke();
        }
    }

    private void NotifyHealthChanged(float previousValue, float newValue) => OnHealthChanged?.Invoke(newValue, Entity);


}
