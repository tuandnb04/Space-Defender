using System.Collections;
using Combat;
using Core;
using UI;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Player
{
    public partial class PlayerController
    {
        private void HandleShooting()
        {
            var shoot = false;

#if ENABLE_INPUT_SYSTEM
            var gamepad = Gamepad.current;
            if (gamepad != null)
                if (gamepad.rightTrigger.isPressed || gamepad.buttonSouth.isPressed)
                    shoot = true;

            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            if (keyboard != null)
            {
                if (keyboard.spaceKey.isPressed) shoot = true;
                if (keyboard.tKey.wasPressedThisFrame) autoFireForDemo = !autoFireForDemo;
            }

            if (mouse != null && mouse.leftButton.isPressed) shoot = true;
#else
        if (Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0)) shoot = true;
        if (Input.GetKeyDown(KeyCode.T)) autoFireForDemo = !autoFireForDemo;
#endif

            if (autoFireForDemo) shoot = true;

            var actualFireRate = isFeverActive ? fireRate * 0.5f : weaponLevel == 5 ? fireRate * 0.8f : fireRate;

            if (!shoot || !(Time.time >= _nextFireTime)) return;
            _nextFireTime = Time.time + actualFireRate;
            Shoot();
        }

        private void Shoot()
        {
            var spawnPos = firePoint ? firePoint.position : transform.position + Vector3.up * 0.6f;
            if (!laserPrefab) return;

            // Weapon Levels 1 to 5
            switch (weaponLevel)
            {
                case 1:
                    SpawnPlayerLaser(spawnPos, Quaternion.identity);
                    break;
                case 2:
                    SpawnPlayerLaser(spawnPos + Vector3.left * 0.18f, Quaternion.identity);
                    SpawnPlayerLaser(spawnPos + Vector3.right * 0.18f, Quaternion.identity);
                    break;
                case 3:
                    SpawnPlayerLaser(spawnPos, Quaternion.identity);
                    SpawnPlayerLaser(spawnPos + Vector3.left * 0.25f, Quaternion.Euler(0, 0, 14f));
                    SpawnPlayerLaser(spawnPos + Vector3.right * 0.25f, Quaternion.Euler(0, 0, -14f));
                    break;
                case 4:
                    SpawnPlayerLaser(spawnPos + Vector3.left * 0.16f, Quaternion.identity);
                    SpawnPlayerLaser(spawnPos + Vector3.right * 0.16f, Quaternion.identity);
                    SpawnPlayerLaser(spawnPos + Vector3.left * 0.38f, Quaternion.Euler(0, 0, 22f));
                    SpawnPlayerLaser(spawnPos + Vector3.right * 0.38f, Quaternion.Euler(0, 0, -22f));
                    break;
                default:
                    SpawnPlayerLaser(spawnPos, Quaternion.identity);
                    SpawnPlayerLaser(spawnPos + Vector3.left * 0.22f, Quaternion.Euler(0, 0, 14f));
                    SpawnPlayerLaser(spawnPos + Vector3.right * 0.22f, Quaternion.Euler(0, 0, -14f));
                    SpawnPlayerLaser(spawnPos + Vector3.left * 0.44f, Quaternion.Euler(0, 0, 28f));
                    SpawnPlayerLaser(spawnPos + Vector3.right * 0.44f, Quaternion.Euler(0, 0, -28f));
                    break;
            }

            if (AudioManager.Instance) AudioManager.Instance.PlayShoot();
            TriggerRumble(0.04f, 0.08f, 0.05f);
        }

        private void SpawnPlayerLaser(Vector3 pos, Quaternion rot)
        {
            var laserObj = ObjectPoolManager.Spawn(laserPrefab, pos, rot);
            var laser = laserObj ? laserObj.GetComponent<Laser>() : null;
            if (laser == null) return;
            laser.isEnemyLaser = false;
            laser.isFeverLaser = isFeverActive;
            if (!isFeverActive) return;
            laser.damage = 2;
            var sr = laser.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = new Color(0.2f, 1f, 0.9f);
        }

        private void HandleSecondaryMissiles()
        {
            if (homingMissilePrefab == null) return;
            if (weaponLevel < 2 && !isFeverActive) return;

            var interval = isFeverActive ? missileInterval * 0.5f : missileInterval;
            if (!(Time.time >= _nextMissileTime)) return;
            _nextMissileTime = Time.time + interval;
            LaunchMissiles();
        }

        private void LaunchMissiles()
        {
            var leftWing = transform.position + new Vector3(-0.48f, 0.1f, 0f);
            var rightWing = transform.position + new Vector3(0.48f, 0.1f, 0f);

            ObjectPoolManager.Spawn(homingMissilePrefab, leftWing, Quaternion.identity);
            ObjectPoolManager.Spawn(homingMissilePrefab, rightWing, Quaternion.identity);

            if (AudioManager.Instance != null) AudioManager.Instance.PlayMissileLaunch();
        }

        public void AddOverload(float amount)
        {
            if (isFeverActive) return;

            currentOverload = Mathf.Clamp(currentOverload + amount, 0f, maxOverload);

            if (UIManager.Instance != null) UIManager.Instance.UpdateOverload(OverloadRatio);

            if (currentOverload >= maxOverload) ActivateFeverMode(feverDuration);
        }

        public void RegisterGraze(Vector3 laserPos)
        {
            AddOverload(3.5f);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayGraze();

            if (floatingScorePrefab != null)
                FloatingScore.SpawnText(floatingScorePrefab, CockpitPosition + Vector3.up * 0.4f, "GRAZE!",
                    new Color(0.3f, 1f, 0.8f));
        }

        private void ActivateFeverMode(float duration)
        {
            if (_feverCoroutine != null) StopCoroutine(_feverCoroutine);
            _feverCoroutine = StartCoroutine(FeverRoutine(duration));
        }

        private IEnumerator FeverRoutine(float duration)
        {
            isFeverActive = true;
            _feverTimer = duration;

            if (AudioManager.Instance != null) AudioManager.Instance.PlayFeverActivate();

            if (floatingScorePrefab != null)
                FloatingScore.SpawnText(floatingScorePrefab, transform.position + Vector3.up * 1.0f, "FEVER OVERDRIVE!",
                    Color.cyan);

            if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.3f, 0.15f);

            if (PostProcessingManager.Instance != null) PostProcessingManager.Instance.SetFeverBloom(true);
            if (AudioManager.Instance != null) AudioManager.Instance.SetMusicFeverMode(true);

            // Throttle UI updates to ~10/s (UI canvas rebuild is expensive at 60fps)
            const float uiUpdateInterval = 0.1f;
            var nextUiUpdate = 0f;

            while (_feverTimer > 0f)
            {
                _feverTimer -= Time.deltaTime;
                var overloadRatio = _feverTimer / duration; // local calc, no property overhead
                currentOverload = overloadRatio * maxOverload;

                // Rainbow cycle visual (every frame is fine, it's just a color set)
                if (_spriteRenderer != null)
                {
                    var hue = Mathf.PingPong(Time.time * 3f, 1f);
                    _spriteRenderer.color = Color.HSVToRGB(hue, 0.8f, 1f);
                }

                // UI throttle: push overload bar at 10fps, not 60fps
                if (Time.time >= nextUiUpdate)
                {
                    nextUiUpdate = Time.time + uiUpdateInterval;
                    if (UIManager.Instance != null) UIManager.Instance.UpdateOverload(overloadRatio);
                }

                yield return null;
            }

            isFeverActive = false;
            currentOverload = 0f;
            if (_spriteRenderer != null) _spriteRenderer.color = Color.white;

            if (PostProcessingManager.Instance != null) PostProcessingManager.Instance.SetFeverBloom(false);
            if (AudioManager.Instance != null) AudioManager.Instance.SetMusicFeverMode(false);

            if (UIManager.Instance != null) UIManager.Instance.UpdateOverload(0f);
            _feverCoroutine = null;
        }

        private void UpdateFeverMode()
        {
            if (UIManager.Instance != null) UIManager.Instance.UpdateDashCooldown(DashCooldownRatio);
        }
    }
}