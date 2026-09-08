using UnityEngine;

public class Laser : MonoBehaviour
{
    [Header("Laser Settings")]
    public float speed = 12f;
    public float topBoundaryOffset = 1.0f;
    public bool isEnemyLaser;

    private float _topY = 6f;

    private void Start()
    {
        var cam = Camera.main;
        if (cam != null)
        {
            _topY = cam.orthographicSize + topBoundaryOffset;
        }
    }

    private void Update()
    {
        transform.Translate(transform.up * (speed * Time.deltaTime), Space.World);

        if (transform.position.y > _topY || Mathf.Abs(transform.position.x) > 10f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Hit(collision.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Hit(collision.gameObject);
    }

    private void Hit(GameObject target)
    {
        var enemy = target.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeHit();
            Destroy(gameObject);
            return;
        }

        var boss = target.GetComponent<BossController>();
        if (!boss) return;
        boss.TakeHit();
        Destroy(gameObject);
    }
}