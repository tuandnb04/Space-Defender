using Core;
using Player;
using UI;
using UnityEngine;

namespace Environment
{
    public class StarPickup : MonoBehaviour
    {
        private const float StarChainTimeout = 1.6f;
        private static int _consecutiveStars;
        private static float _lastStarPickupTime;

        [Header("Settings")] public float fallSpeed = 2.0f;

        public float wobbleSpeed = 4f;
        public float wobbleAmount = 0.6f;
        public int starValue = 1;
        public int scoreBonus = 25;

        [Header("Magnetism")] public bool isAttracted;

        public float baseMagnetSpeed = 6f;
        public float magnetAcceleration = 18f;
        private float _bottomY = -6f;

        private float _currentSpeed;
        private PlayerController _playerController;
        private Transform _playerTransform;
        private float _spawnTime;

        private void Update()
        {
            if (_playerTransform is not null)
            {
                var diff = _playerTransform.position - transform.position;
                var sqrDist = diff.sqrMagnitude;
                var magnetRadius = _playerController ? PlayerController.GetMagnetRadius() : 2.2f;

                if (sqrDist <= magnetRadius * magnetRadius || isAttracted)
                {
                    isAttracted = true;
                    _currentSpeed += magnetAcceleration * Time.deltaTime;
                    transform.position = Vector3.MoveTowards(transform.position, _playerTransform.position,
                        _currentSpeed * Time.deltaTime);

                    if (sqrDist < 0.1225f) // 0.35^2
                        Collect();
                    return;
                }
            }

            // Normal falling with wobble
            var wobble = Mathf.Sin((Time.time - _spawnTime) * wobbleSpeed) * wobbleAmount * Time.deltaTime;
            transform.Translate(new Vector3(wobble, -fallSpeed * Time.deltaTime, 0f), Space.World);

            if (transform.position.y < _bottomY) Despawn();
        }

        private void OnEnable()
        {
            _spawnTime = Time.time;
            _currentSpeed = baseMagnetSpeed;
            isAttracted = false;

            var cam = Camera.main;
            if (cam != null) _bottomY = -cam.orthographicSize - 1.5f;

            var player = PlayerController.Instance;
            if (player == null) return;
            _playerController = player;
            _playerTransform = player.transform;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerController>() != null) Collect();
        }

        public void AttractToPlayerImmediately()
        {
            isAttracted = true;
            _currentSpeed = 9f;
        }

        private void Collect()
        {
            if (Time.time - _lastStarPickupTime > StarChainTimeout) _consecutiveStars = 0;

            _consecutiveStars++;
            _lastStarPickupTime = Time.time;

            // Play musical scale note (Do, Re, Mi, Fa, Sol, La, Si, High Do...)
            if (AudioManager.Instance) AudioManager.Instance.PlayStarPickup(_consecutiveStars - 1);

            // Award score and currency
            if (GameManager.Instance)
            {
                GameManager.Instance.AddScore(scoreBonus * Mathf.Min(_consecutiveStars, 8));
                GameManager.Instance.CollectStar(starValue);
            }

            // Overload gauge gain
            var player = PlayerController.Instance;
            if (player)
            {
                player.AddOverload(2.5f);
                if (player.floatingScorePrefab && _consecutiveStars % 3 == 0)
                {
                    var pitchNames = new[] { "DO!", "RE!", "MI!", "FA!", "SOL!", "LA!", "SI!", "CHORD!" };
                    var noteName = pitchNames[Mathf.Clamp(_consecutiveStars - 1, 0, pitchNames.Length - 1)];
                    FloatingScore.SpawnText(player.floatingScorePrefab, transform.position, noteName,
                        new Color(1f, 0.9f, 0.2f));
                }
            }

            Despawn();
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void Despawn()
        {
            ObjectPoolManager.Despawn(gameObject);
        }
    }
}