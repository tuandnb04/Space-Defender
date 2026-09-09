using Core;
using Player;
using UnityEngine;

namespace Environment
{
    public enum PowerUpType
    {
        TripleShot,
        Shield,
        Health,
        PowerCore
    }

    public class PowerUp : MonoBehaviour
    {
        [Header("Power-Up Settings")] public PowerUpType powerUpType = PowerUpType.TripleShot;

        public float fallSpeed = 2.2f;
        public float wobbleSpeed = 3.5f;
        public float wobbleAmount = 0.8f;
        public float magnetSpeed = 7.0f;

        private float _bottomY = -6f;
        private bool _isAttracted;
        private PlayerController _playerController;
        private Transform _playerTransform;
        private float _spawnTime;
        public static int ActivePowerUpCount { get; private set; }

        private void Update()
        {
            if (!_playerTransform && PlayerController.Instance != null)
            {
                _playerController = PlayerController.Instance;
                _playerTransform = _playerController.transform;
            }

            if (_playerTransform != null)
            {
                var diff = _playerTransform.position - transform.position;
                var sqrDist = diff.sqrMagnitude;
                var magnetRadius = _playerController != null ? PlayerController.GetMagnetRadius() * 0.8f : 2.0f;

                if (sqrDist <= magnetRadius * magnetRadius || _isAttracted)
                {
                    _isAttracted = true;
                    transform.position = Vector3.MoveTowards(transform.position, _playerTransform.position,
                        magnetSpeed * Time.deltaTime);

                    if (sqrDist < 0.36f) CheckPickup(_playerTransform.gameObject);
                    return;
                }
            }

            // Fall downwards with slight horizontal wobble
            var wobble = Mathf.Sin((Time.time - _spawnTime) * wobbleSpeed) * wobbleAmount * Time.deltaTime;
            transform.Translate(new Vector3(wobble, -fallSpeed * Time.deltaTime, 0f), Space.World);

            // Out of bounds
            if (transform.position.y < _bottomY) Destroy(gameObject);
        }

        private void OnEnable()
        {
            ActivePowerUpCount++;
            _spawnTime = Time.time;
            _isAttracted = false;

            var cam = Camera.main;
            if (cam != null) _bottomY = -cam.orthographicSize - 1.5f;

            var player = PlayerController.Instance;
            if (player == null) return;
            _playerController = player;
            _playerTransform = player.transform;
        }

        private void OnDisable()
        {
            ActivePowerUpCount = Mathf.Max(0, ActivePowerUpCount - 1);
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
}