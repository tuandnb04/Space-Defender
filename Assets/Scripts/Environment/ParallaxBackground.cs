using System;
using UnityEngine;

namespace Environment
{
    public class ParallaxBackground : MonoBehaviour
    {
        [Header("Base Settings")] public float baseScrollSpeed = 1.8f;

        public bool fitCameraWidth = true;

        [Header("Parallax Layers")]
        public ParallaxLayer farNebulaLayer = new() { name = "Far Nebula", speedMultiplier = 0.35f };

        public ParallaxLayer midStarsLayer = new() { name = "Mid Stars", speedMultiplier = 1.0f };
        public ParallaxLayer nearDustLayer = new() { name = "Near Dust", speedMultiplier = 2.2f };

        private void Start()
        {
            SetupLayer(farNebulaLayer, 10f);
            SetupLayer(midStarsLayer, 5f);
            SetupLayer(nearDustLayer, 1f);
        }

        private void Update()
        {
            UpdateLayer(farNebulaLayer);
            UpdateLayer(midStarsLayer);
            UpdateLayer(nearDustLayer);
        }

        private void SetupLayer(ParallaxLayer layer, float zPos)
        {
            if (layer == null || layer.tile1 == null) return;

            var cam = Camera.main;
            if (cam != null && fitCameraWidth)
            {
                var camHeight = cam.orthographicSize * 2f;
                var camWidth = camHeight * cam.aspect;

                var sr = layer.tile1.GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite != null)
                {
                    var spriteWidth = sr.sprite.rect.width / sr.sprite.pixelsPerUnit;
                    var spriteHeight = sr.sprite.rect.height / sr.sprite.pixelsPerUnit;

                    var scaleX = camWidth / spriteWidth;
                    var scaleY = scaleX;
                    if (spriteHeight * scaleY < camHeight)
                    {
                        scaleY = camHeight / spriteHeight;
                        scaleX = scaleY;
                    }

                    layer.tile1.localScale = new Vector3(scaleX, scaleY, 1f);
                    if (layer.tile2 != null) layer.tile2.localScale = new Vector3(scaleX, scaleY, 1f);

                    layer.tileHeight = spriteHeight * scaleY;
                }
            }

            if (layer.tile1 != null) layer.tile1.position = new Vector3(0f, 0f, zPos);
            if (layer.tile2 != null) layer.tile2.position = new Vector3(0f, layer.tileHeight, zPos);
        }

        private void UpdateLayer(ParallaxLayer layer)
        {
            if (layer == null || !layer.tile1 || !layer.tile2) return;

            var movement = baseScrollSpeed * layer.speedMultiplier * Time.deltaTime;
            layer.tile1.position += Vector3.down * movement;
            layer.tile2.position += Vector3.down * movement;

            var h = layer.tileHeight;
            if (layer.tile1.position.y <= -h)
                layer.tile1.position =
                    new Vector3(layer.tile1.position.x, layer.tile2.position.y + h, layer.tile1.position.z);
            if (layer.tile2.position.y <= -h)
                layer.tile2.position =
                    new Vector3(layer.tile2.position.x, layer.tile1.position.y + h, layer.tile2.position.z);
        }

        [Serializable]
        public class ParallaxLayer
        {
            public string name = "Layer";
            public Transform tile1;
            public Transform tile2;
            public float speedMultiplier = 1.0f;
            public float tileHeight = 16f;
        }
    }
}