using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 7f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private LayerMask targetLayers;

    public Action<Projectile> OnFinished;

    private int damage;
    private float dieAt;

    public void Launch(Vector2 direction, int dmg)
    {
        damage = dmg;
        dieAt = Time.time + lifetime;
        transform.right = direction;
        GetComponent<Rigidbody2D>().linearVelocity = direction.normalized * speed;
    }

    private void Update()
    {
        if (Time.time >= dieAt) OnFinished?.Invoke(this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var go = other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : other.gameObject;
        if ((targetLayers.value & (1 << go.layer)) == 0
            && (targetLayers.value & (1 << other.gameObject.layer)) == 0)
            return;
        var target = go.GetComponent<IDamageable>() ?? other.GetComponentInParent<IDamageable>();
        if (target != null)
            target.TakeDamage(damage, transform.position);
        OnFinished?.Invoke(this);
    }
}