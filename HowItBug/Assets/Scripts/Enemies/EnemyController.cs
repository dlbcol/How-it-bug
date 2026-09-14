using UnityEngine;

public class EnemyController : MonoBehaviour
{

    [SerializeField] private EnemyData _data;

    private Health _health;

    private void Awake()
    {
        _health = GetComponent<Health>();
    }


    void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        _health.Initialize(_data);
    }

}
