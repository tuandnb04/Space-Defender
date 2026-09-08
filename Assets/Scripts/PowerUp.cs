using UnityEngine;

namespace SpaceDefender
{
    public enum PowerUpType
    {
        TripleShot,
        Shield,
        Health
    }

    public class PowerUp : MonoBehaviour
    {
        [Header("Power-Up Settings")]
        public PowerUpType powerUpType = PowerUpType.TripleShot;
        public float fallSpeed = 2.2f;
        public float wobbleSpeed = 3.5f;
        public float wobbleAmount = 0.8f;

        private float bottomY = -6f;
        private float spawnTime;

        private void Start()
        {
            spawnTime = Time.time;
            Camera cam = Camera.main;
            if (cam != null)
            {
                bottomY = -cam.orthographicSize - 1.5f;
            }
        }

        private void Update()
        {
            // Fall downwards with slight horizontal wobble
            float wobble = Mathf.Sin((Time.time - spawnTime) * wobbleSpeed) * wobbleAmount * Time.deltaTime;
            transform.Translate(new Vector3(wobble, -fallSpeed * Time.deltaTime, 0f), Space.World);

            // Out of bounds
            if (transform.position.y < bottomY)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            CheckPickup(collision.gameObject);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            CheckPickup(collision.gameObject);
        }

        private void CheckPickup(GameObject target)
        {
            PlayerController player = target.GetComponent<PlayerController>();
            if (player != null)
            {
                player.ApplyPowerUp(powerUpType);

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayPowerUp();
                }

                Destroy(gameObject);
            }
        }
    }
}
