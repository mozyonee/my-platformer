using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerRespawn : MonoBehaviour
{
    Vector2 spawnPosition;
    Rigidbody2D body;

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        spawnPosition = transform.position;
    }

    public void SetCheckpoint(Vector2 position) => spawnPosition = position;

    public void Respawn()
    {
        transform.position = spawnPosition;
        body.linearVelocity = Vector2.zero;
    }
}