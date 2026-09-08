using UnityEngine;

namespace SpaceDefender
{
    public class Laser : MonoBehaviour
    {
        [Header("Laser Settings")]
        public float speed = 12f;
        public float topBoundaryOffset = 1.0f;

        private float topY = 6f;

        private void Start()
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                topY = cam.orthographicSize + topBoundaryOffset;
            }
        }

        private void Update()
        {
            transform.Translate(transform.up * (speed * Time.deltaTime), Space.World);

            if (transform.position.y > topY || Mathf.Abs(transform.position.x) > 10f)
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
            Enemy enemy = target.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeHit();
                Destroy(gameObject);
                return;
            }

            BossController boss = target.GetComponent<BossController>();
            if (boss != null)
            {
                boss.TakeHit();
                Destroy(gameObject);
                return;
            }
        }
    }
}
