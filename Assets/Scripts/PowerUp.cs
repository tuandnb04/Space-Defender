using UnityEngine;

public enum PowerUpType
{
    TripleShot,
    Shield,
    Health
}

public class PowerUp : MonoBehaviour
{
    [Header("Power-Up Settings")] public PowerUpType powerUpType = PowerUpType.TripleShot;

    public float fallSpeed = 2.2f;
    public float wobbleSpeed = 3.5f;
    public float wobbleAmount = 0.8f;

    private float _bottomY = -6f;
    private float _spawnTime;

    private void Start()
    {
        _spawnTime = Time.time;
        var cam = Camera.main;
        if (cam != null) _bottomY = -cam.orthographicSize - 1.5f;
    }

    private void Update()
    {
        // Fall downwards with slight horizontal wobble
        var wobble = Mathf.Sin((Time.time - _spawnTime) * wobbleSpeed) * wobbleAmount * Time.deltaTime;
        transform.Translate(new Vector3(wobble, -fallSpeed * Time.deltaTime, 0f), Space.World);

        // Out of bounds
        if (transform.position.y < _bottomY) Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckPickup(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CheckPickup(collision.gameObject);
    }

    private void CheckPickup(GameObject target)
    {
        var player = target.GetComponentInParent<PlayerController>();
        if (player == null) return;
        player.ApplyPowerUp(powerUpType);

        if (AudioManager.Instance != null) AudioManager.Instance.PlayPowerUp();

        Destroy(gameObject);
    }
}