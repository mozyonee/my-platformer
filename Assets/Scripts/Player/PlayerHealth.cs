using UnityEngine;

public class PlayerHealth : Health
{
    [SerializeField] private PlayerRespawn respawn;

    protected override void Die()
    {
        respawn.Respawn();
        ResetHealth();
    }
}