using System;
using UnityEngine;

public class Laser : MonoBehaviour
{
    [Header("Laser Settings")] public float speed = 12f;

    public float topBoundaryOffset = 1.0f;
    public bool isEnemyLaser;

    private float _topY = 6f;
    private float _bottomY = -6f;

    private void Awake()
    {
        if (!isEnemyLaser) return;
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.flipY = true;
    }

    private void Start()
    {
        var cam = Camera.main;
        if (cam == null) return;
        _topY = cam.orthographicSize + topBoundaryOffset;
        _bottomY = -cam.orthographicSize - topBoundaryOffset;
    }

    private void Update()
    {
        var dir = isEnemyLaser ? -transform.up : transform.up;
        transform.Translate(dir * (speed * Time.deltaTime), Space.World);

        if (isEnemyLaser)
        {
            if (transform.position.y < _bottomY || Mathf.Abs(transform.position.x) > 10f)
                Destroy(gameObject);
        }
        else
        {
            if (transform.position.y > _topY || Mathf.Abs(transform.position.x) > 10f)
                Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Hit(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Hit(collision.gameObject);
    }

    private void Hit(GameObject target)
    {
        if (isEnemyLaser)
        {
            // Enemy laser ONLY damages the player, never enemies or bosses
            var player = target.GetComponentInParent<PlayerController>();
            if (player == null) return;
            player.TakeDamage();
            Destroy(gameObject);
            return;
        }

        // Player laser damages Enemy and Boss
        var enemy = target.GetComponentInParent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeHit();
            Destroy(gameObject);
            return;
        }

        var boss = target.GetComponentInParent<BossController>();
        if (boss == null) return;
        boss.TakeHit();
        Destroy(gameObject);
    }
}