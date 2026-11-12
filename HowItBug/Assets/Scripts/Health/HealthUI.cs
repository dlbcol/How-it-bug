using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    //TODO: Show health bar only while player sees enemy

    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _healthText;
    [SerializeField] private Image _healthBar;
    [SerializeField] private Canvas _healthCanvas;

    private Health _health;

    private void Awake()
    {
        _health = GetComponentInParent<Health>();
        _health.OnHealthChanged += UpdateHealthBar;
        UpdateHealthBar(_health.CurrentHealth.Value, _health.Entity);
    }

    private void OnDestroy()
    {
        _health.OnHealthChanged -= UpdateHealthBar;
    }

    private void UpdateHealthBar(float currentHealth, IEntity entity)
    {
        _nameText.text = _health.EntityName.Value;
        _healthBar.fillAmount = currentHealth / _health.MaxHealth.Value;
        _healthText.text = $"{currentHealth}/{_health.MaxHealth.Value}";
    }

    public void Show()
    {
        _healthCanvas.enabled = true;
    }

    public void Hide()
    {
        _healthCanvas.enabled= false;
    }

}
