using System.Collections;
using UnityEngine;

namespace Combat
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class HitFlashEffect : MonoBehaviour
    {
        private static Material _sharedFlashMaterial;
        private static readonly int FlashAmountProp = Shader.PropertyToID("_FlashAmount");
        private static readonly int FlashColorProp = Shader.PropertyToID("_FlashColor");
        private Coroutine _flashCoroutine;
        private MaterialPropertyBlock _propBlock;
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _propBlock = new MaterialPropertyBlock();

            EnsureFlashMaterial();
            if (!_spriteRenderer || !_sharedFlashMaterial) return;
            _spriteRenderer.sharedMaterial = _sharedFlashMaterial;
        }

        private void OnDisable()
        {
            if (_spriteRenderer != null)
            {
                _spriteRenderer.GetPropertyBlock(_propBlock);
                _propBlock.SetFloat(FlashAmountProp, 0f);
                _spriteRenderer.SetPropertyBlock(_propBlock);
            }

            _flashCoroutine = null;
        }

        private static void EnsureFlashMaterial()
        {
            if (_sharedFlashMaterial) return;

            var shader = Shader.Find("Universal Render Pipeline/2D/SpriteHitFlash");
            if (!shader) shader = Shader.Find("Sprites/Default");

            if (shader)
                _sharedFlashMaterial = new Material(shader) { name = "SpriteHitFlash_Shared" };
        }

        public void Flash(float duration = 0.08f, Color? flashColor = null)
        {
            if (!gameObject.activeInHierarchy || !_spriteRenderer) return;

            if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
            _flashCoroutine = StartCoroutine(FlashRoutine(duration, flashColor ?? Color.white));
        }

        private IEnumerator FlashRoutine(float duration, Color flashColor)
        {
            // Set flash ON
            _spriteRenderer.GetPropertyBlock(_propBlock);
            _propBlock.SetFloat(FlashAmountProp, 1f);
            _propBlock.SetColor(FlashColorProp, flashColor);
            _spriteRenderer.SetPropertyBlock(_propBlock);

            // Manual timer — avoids WaitForSeconds heap allocation on every flash
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Set flash OFF
            _spriteRenderer.GetPropertyBlock(_propBlock);
            _propBlock.SetFloat(FlashAmountProp, 0f);
            _spriteRenderer.SetPropertyBlock(_propBlock);

            _flashCoroutine = null;
        }
    }
}