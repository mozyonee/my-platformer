using UnityEngine;

public class PlayerHealth : Health
{
    [SerializeField] private PlayerRespawn respawn;
    [Header("Канали подій")]
    [SerializeField] private IntEventSO healthChanged;
    [SerializeField] private IntEventSO maxHealthChanged;

    private void Start() => PushHud();

    public void PushHud()
    {
        maxHealthChanged.Raise(Max);
        healthChanged.Raise(Current);
    }

    protected override void OnHealthChanged() => healthChanged.Raise(Current);

    protected override void Die()
    {
        respawn.Respawn();
        ResetHealth();
    }
}
