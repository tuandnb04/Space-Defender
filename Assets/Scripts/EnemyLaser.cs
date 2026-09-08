using UnityEngine;

namespace SpaceDefender
{
    public class EnemyLaser : MonoBehaviour
    {
        public float speed = 7f;
        private float bottomBound = -10f;

        private void Start()
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                bottomBound = -cam.orthographicSize - 1.5f;
            }
        }

        private void Update()
        {
            transform.Translate(Vector3.down * speed * Time.deltaTime);

            if (transform.position.y < bottomBound)
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
            if (hitObj.CompareTag("Player"))
            {
                PlayerController player = hitObj.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.TakeDamage(1);
                }
                Destroy(gameObject);
            }
        }
    }
}
