using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    [Header("Scroll Settings")]
    public float scrollSpeed = 1.8f;
    public bool fitCameraWidth = true;

    [Header("Background Sprites")]
    public Transform background1;
    public Transform background2;

    private float _bgHeight;

    private void Start()
    {
        SetupBackgroundDimensions();
    }

    public void SetupBackgroundDimensions()
    {
        var cam = Camera.main;
        if (cam != null && fitCameraWidth && background1 != null)
        {
            var camHeight = cam.orthographicSize * 2f;
            var camWidth = camHeight * cam.aspect;

            var sr1 = background1.GetComponent<SpriteRenderer>();
            if (sr1 != null && sr1.sprite != null)
            {
                var spriteWidth = sr1.sprite.rect.width / sr1.sprite.pixelsPerUnit;
                var spriteHeight = sr1.sprite.rect.height / sr1.sprite.pixelsPerUnit;

                // Scale to fit width, ensuring minimum height covers camera
                var scaleX = camWidth / spriteWidth;
                var scaleY = scaleX; // Keep aspect ratio
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
            _bgHeight = (sr.sprite.rect.height / sr.sprite.pixelsPerUnit) * background1.localScale.y;
        }
        else
        {
            _bgHeight = 16f;
        }

        if (background1 == null || background2 == null) return;
        background1.position = new Vector3(0f, 0f, 5f);
        background2.position = new Vector3(0f, _bgHeight, 5f);
    }

    private void Update()
    {
        var movement = scrollSpeed * Time.deltaTime;

        if (background1)
        {
            background1.position += Vector3.down * movement;
        }

        if (background2)
        {
            background2.position += Vector3.down * movement;
        }

        if (!background1 || !background2) return;
        // When background1 moves completely below camera view
        if (background1.position.y <= -_bgHeight)
        {
            background1.position = new Vector3(background1.position.x, background2.position.y + _bgHeight, background1.position.z);
        }

        // When background2 moves completely below camera view
        if (background2.position.y <= -_bgHeight)
        {
            background2.position = new Vector3(background2.position.x, background1.position.y + _bgHeight, background2.position.z);
        }
    }
}