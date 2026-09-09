using System;
using System.Collections;
using Core;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Player
{
    public partial class PlayerController
    {
        // ─── Ghost Trail Pool ───────────────────────────────────────────────
        // 4 ghost slots pre-allocated; no new GameObject on each dash.
        private const int GhostPoolSize = 4;
        private GhostSlot[] _ghostPool;

        private struct GhostSlot
        {
            public GameObject Go;
            public SpriteRenderer Sr;
        }

        private void InitGhostPool()
        {
            _ghostPool = new GhostSlot[GhostPoolSize];
            for (var i = 0; i < GhostPoolSize; i++)
            {
                var go = new GameObject($"DashGhost_{i}");
                go.SetActive(false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sortingOrder = _spriteRenderer ? _spriteRenderer.sortingOrder - 1 : 9;
                _ghostPool[i] = new GhostSlot { Go = go, Sr = sr };
            }
        }

        private void HandleMovement()
        {
            if (_isDashing) return;

            var horizontal = 0f;
            var vertical = 0f;

#if ENABLE_INPUT_SYSTEM
            var gamepad = Gamepad.current;
            if (gamepad != null)
            {
                var stick = gamepad.leftStick.ReadValue();
                if (stick.sqrMagnitude > 0.04f)
                {
                    horizontal += stick.x;
                    vertical += stick.y;
                }

                if (gamepad.dpad.left.isPressed) horizontal -= 1f;
                if (gamepad.dpad.right.isPressed) horizontal += 1f;
                if (gamepad.dpad.up.isPressed) vertical += 1f;
                if (gamepad.dpad.down.isPressed) vertical -= 1f;
            }

            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) horizontal -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) horizontal += 1f;
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) vertical += 1f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) vertical -= 1f;
            }
            else
#endif
            {
                try
                {
                    if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) horizontal -= 1f;
                    if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) horizontal += 1f;
                    if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) vertical += 1f;
                    if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) vertical -= 1f;
                }
                catch (InvalidOperationException)
                {
                }
            }

            var moveInput = new Vector2(horizontal, vertical);
            if (moveInput.sqrMagnitude > 1f) moveInput.Normalize();

            if (moveInput.sqrMagnitude > 0.01f)
            {
                _lastMoveDir = moveInput;
                SetThrusterRate(55f, 3.8f, new Color(0.3f, 1.2f, 2.2f, 0.95f));
            }
            else
            {
                SetThrusterRate(22f, 2.0f, new Color(0.2f, 1.0f, 1.8f, 0.85f));
            }

            var pos = transform.position;
            pos.x += moveInput.x * moveSpeed * Time.deltaTime;
            pos.y += moveInput.y * moveSpeed * Time.deltaTime;

            pos.x = Mathf.Clamp(pos.x, _minX, _maxX);
            pos.y = Mathf.Clamp(pos.y, _minY, _maxY);
            transform.position = pos;
        }

        private void HandleDashInput()
        {
            var dashRequested = false;

#if ENABLE_INPUT_SYSTEM
            var gamepad = Gamepad.current;
            if (gamepad != null)
                if (gamepad.buttonEast.wasPressedThisFrame || gamepad.leftShoulder.wasPressedThisFrame ||
                    gamepad.leftTrigger.wasPressedThisFrame)
                    dashRequested = true;

            var keyboard = Keyboard.current;
            if (keyboard != null)
                if (keyboard.leftShiftKey.wasPressedThisFrame || keyboard.rightShiftKey.wasPressedThisFrame)
                    dashRequested = true;
#else
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            dashRequested = true;
        }
#endif

            if (dashRequested && _dashCooldownTimer <= 0f && !_isDashing) StartCoroutine(DashRoutine());
        }

        private IEnumerator DashRoutine()
        {
            _isDashing = true;
            _dashCooldownTimer = dashCooldown;

            TriggerRumble(0.25f, 0.45f, 0.15f);

            if (AudioManager.Instance) AudioManager.Instance.PlayDash();

            // Ghost Trail effect
            if (_ghostTrailCoroutine != null) StopCoroutine(_ghostTrailCoroutine);
            _ghostTrailCoroutine = StartCoroutine(GhostTrailRoutine());

            // Neon Plasma Thruster Flare
            SetThrusterRate(130f, 6.8f, new Color(0.7f, 1.6f, 2.5f, 1.0f));

            var dashDir = _lastMoveDir.normalized;
            if (dashDir.sqrMagnitude < 0.1f) dashDir = Vector2.up;

            var elapsed = 0f;
            var dashSpeed = moveSpeed * dashSpeedMultiplier;

            // Visual flash (Cyan Hologram)
            if (_spriteRenderer) _spriteRenderer.color = new Color(0.2f, 1f, 1f, 0.7f);

            while (elapsed < dashDuration)
            {
                var pos = transform.position;
                pos.x += dashDir.x * dashSpeed * Time.deltaTime;
                pos.y += dashDir.y * dashSpeed * Time.deltaTime;
                pos.x = Mathf.Clamp(pos.x, _minX, _maxX);
                pos.y = Mathf.Clamp(pos.y, _minY, _maxY);
                transform.position = pos;

                if (hasAfterburner)
                    TriggerAfterburnerBlast(pos - (Vector3)(dashDir * 0.4f));

                elapsed += Time.deltaTime;
                yield return null;
            }

            _isDashing = false;

            // Reset thruster after dash
            SetThrusterRate(35f, 2.8f, new Color(0.2f, 1.1f, 2.0f, 0.9f));

            // Maintain i-frame for extra window (0.22s total)
            var remainingIFrame = Mathf.Max(0f, dashIFrameDuration - dashDuration);
            yield return new WaitForSeconds(remainingIFrame);

            if (_spriteRenderer && !_isInvulnerable) _spriteRenderer.color = Color.white;
        }

        private IEnumerator GhostTrailRoutine()
        {
            const int spawnCount = 4;
            var interval = dashDuration / spawnCount;

            for (var i = 0; i < spawnCount; i++)
            {
                SpawnGhostTrail();
                yield return new WaitForSeconds(interval);
            }
        }

        private int _nextGhostSlot;

        private void SpawnGhostTrail()
        {
            if (_spriteRenderer == null || _spriteRenderer.sprite == null) return;

            // Lazy-init pool on first dash (Awake order not guaranteed)
            if (_ghostPool == null) InitGhostPool();

            // Round-robin through the pre-allocated pool
            if (_ghostPool == null) return;
            var slot = _ghostPool[_nextGhostSlot % GhostPoolSize];
            _nextGhostSlot++;

            slot.Sr.sprite = _spriteRenderer.sprite;
            slot.Sr.color = new Color(0.3f, 0.85f, 1f, 0.5f);
            slot.Sr.sortingOrder = _spriteRenderer.sortingOrder - 1;
            slot.Go.transform.SetPositionAndRotation(transform.position, transform.rotation);
            slot.Go.transform.localScale = transform.localScale;
            slot.Go.SetActive(true);

            StartCoroutine(FadeGhostSlot(slot, 0.25f));
        }

        private static IEnumerator FadeGhostSlot(GhostSlot slot, float duration)
        {
            var elapsed = 0f;
            const float startAlpha = 0.5f;
            while (elapsed < duration)
            {
                if (slot.Sr != null)
                    slot.Sr.color = new Color(0.3f, 0.85f, 1f, Mathf.Lerp(startAlpha, 0f, elapsed / duration));
                elapsed += Time.deltaTime;
                yield return null;
            }
            if (slot.Go != null) slot.Go.SetActive(false);
        }
    }
}