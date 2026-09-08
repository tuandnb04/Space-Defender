using System.Collections.Generic;
using UnityEngine;

public class ShockwaveEffect : MonoBehaviour
{
    [Header("Expansion Settings")]
    public float maxRadius = 14f;
    public float duration = 0.55f;
    public int bossDamage = 8;

    [Header("Visual")]
    public SpriteRenderer spriteRenderer;
    public Color shockwaveColor = new Color(0.3f, 0.9f, 1f, 0.9f);

    private float _timer;
    private readonly HashSet<Collider2D> _hitColliders = new HashSet<Collider2D>();

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    private void Start()
    {
        transform.localScale = Vector3.zero;

        // Trigger heavy camera shake
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(0.45f, 0.35f);
        }
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        var progress = Mathf.Clamp01(_timer / duration);

        // Expand outward
        var currentRadius = Mathf.Lerp(0.5f, maxRadius, Mathf.Sin(progress * Mathf.PI * 0.5f));
        transform.localScale = new Vector3(currentRadius, currentRadius, 1f);

        // Fade alpha
        if (spriteRenderer)
        {
            var c = shockwaveColor;
            c.a = Mathf.Lerp(0.85f, 0f, progress * progress);
            spriteRenderer.color = c;
        }

        // Detect and clear enemies, boss, enemy lasers in current radius
        var colliders = Physics2D.OverlapCircleAll(transform.position, currentRadius * 0.5f);
        foreach (var col in colliders)
        {
            if (!col || _hitColliders.Contains(col)) continue;

            // Check for Enemy
            var enemy = col.GetComponent<Enemy>();
            if (enemy)
            {
                _hitColliders.Add(col);
                enemy.TakeHit();
                continue;
            }

            // Check for Boss
            var boss = col.GetComponent<BossController>();
            if (boss)
            {
                _hitColliders.Add(col);
                boss.TakeHit(bossDamage);
                continue;
            }

            // Check for Enemy Laser
            var elaser = col.GetComponent<EnemyLaser>();
            if (elaser)
            {
                _hitColliders.Add(col);
                Destroy(elaser.gameObject);
                continue;
            }

            var laser = col.GetComponent<Laser>();
            if (!laser || !laser.isEnemyLaser) continue;
            _hitColliders.Add(col);
            Destroy(laser.gameObject);
        }

        if (_timer >= duration)
        {
            Destroy(gameObject);
        }
    }
}