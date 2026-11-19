using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] Sprite activeSprite;

    bool claimed;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (claimed || !other.TryGetComponent<PlayerRespawn>(out var respawn))
            return;
        claimed = true;
        respawn.SetCheckpoint(transform.position);
        var sr = GetComponent<SpriteRenderer>();
        if (sr == null)
            return;
        if (activeSprite != null)
            sr.sprite = activeSprite;
        else
            sr.enabled = false;
        var col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;
    }
}
