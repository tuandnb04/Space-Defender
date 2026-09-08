using UnityEngine;

namespace SpaceDefender
{
    public class BackgroundScroller : MonoBehaviour
    {
        [Header("Scroll Settings")]
        public float scrollSpeed = 1.8f;
        public bool fitCameraWidth = true;

        [Header("Background Sprites")]
        public Transform background1;
        public Transform background2;

        private float bgHeight;

        private void Start()
        {
            SetupBackgroundDimensions();
        }

        public void SetupBackgroundDimensions()
        {
            Camera cam = Camera.main;
            if (cam != null && fitCameraWidth && background1 != null)
            {
                float camHeight = cam.orthographicSize * 2f;
                float camWidth = camHeight * cam.aspect;

                SpriteRenderer sr1 = background1.GetComponent<SpriteRenderer>();
                if (sr1 != null && sr1.sprite != null)
                {
                    float spriteWidth = sr1.sprite.rect.width / sr1.sprite.pixelsPerUnit;
                    float spriteHeight = sr1.sprite.rect.height / sr1.sprite.pixelsPerUnit;

                    // Scale to fit width, ensuring minimum height covers camera
                    float scaleX = camWidth / spriteWidth;
                    float scaleY = scaleX; // Keep aspect ratio
                    if (spriteHeight * scaleY < camHeight)
                    {
                        scaleY = camHeight / spriteHeight;
                        scaleX = scaleY;
                    }

                    background1.localScale = new Vector3(scaleX, scaleY, 1f);
                    if (background2 != null)
                    {
                        background2.localScale = new Vector3(scaleX, scaleY, 1f);
                    }
                }
            }

            SpriteRenderer sr = null;
            if (background1 != null) sr = background1.GetComponent<SpriteRenderer>();

            if (sr != null && sr.sprite != null)
            {
                bgHeight = (sr.sprite.rect.height / sr.sprite.pixelsPerUnit) * background1.localScale.y;
            }
            else
            {
                bgHeight = 16f;
            }

            if (background1 != null && background2 != null)
            {
                background1.position = new Vector3(0f, 0f, 5f);
                background2.position = new Vector3(0f, bgHeight, 5f);
            }
        }

        private void Update()
        {
            float movement = scrollSpeed * Time.deltaTime;

            if (background1 != null)
            {
                background1.position += Vector3.down * movement;
            }

            if (background2 != null)
            {
                background2.position += Vector3.down * movement;
            }

            if (background1 != null && background2 != null)
            {
                // When background1 moves completely below camera view
                if (background1.position.y <= -bgHeight)
                {
                    background1.position = new Vector3(background1.position.x, background2.position.y + bgHeight, background1.position.z);
                }

                // When background2 moves completely below camera view
                if (background2.position.y <= -bgHeight)
                {
                    background2.position = new Vector3(background2.position.x, background1.position.y + bgHeight, background2.position.z);
                }
            }
        }
    }
}
