using UnityEngine;

public class EnemyLaser : MonoBehaviour
{
    public float speed = 7f;
    private float _bottomBound = -10f;

    private void Start()
    {
        var cam = Camera.main;
        if (cam != null)
        {
            _bottomBound = -cam.orthographicSize - 1.5f;
        }
    }

    private void Update()
    {
        transform.Translate(Vector3.down * (speed * Time.deltaTime));

        if (transform.position.y < _bottomBound)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleHit(collision.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleHit(collision.gameObject);
    }

    private void HandleHit(GameObject hitObj)
    {
        if (!hitObj.CompareTag("Player")) return;
        var player = hitObj.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage();
        }
        Destroy(gameObject);
    }
}