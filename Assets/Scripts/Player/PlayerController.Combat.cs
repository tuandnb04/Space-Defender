using System;
using System.Collections;
using Combat;
using Core;
using Enemies;
using Environment;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public partial class PlayerController
    {
        private static readonly Collider2D[] OverlapBuffer = new Collider2D[16];

        private Coroutine _rumbleCoroutine;

        private void OnDisable()
        {
#if ENABLE_INPUT_SYSTEM
            var gamepad = Gamepad.current;
            gamepad?.SetMotorSpeeds(0f, 0f);
#endif
        }

        public void ApplyPermanentUpgrades()
        {
            var speedLevel = PlayerPrefs.GetInt("SD_UPGRADE_SPEED_LV", 0);
            moveSpeed = 9.0f * (1f + speedLevel * 0.06f);

            var hpBonus = PlayerPrefs.GetInt("SD_UPGRADE_MAXHP_LV", 0);
            maxLives = 3 + hpBonus;
        }

        public static float GetMagnetRadius()
        {
            var magnetLevel = PlayerPrefs.GetInt("SD_UPGRADE_MAGNET_LV", 0);
            var baseRadius = 2.2f + magnetLevel * 1.0f;
            if (PerkManager.Instance != null && PerkManager.Instance.HasPerk(PerkType.SuperMagnet))
                return baseRadius * 3.0f;
            return baseRadius;
        }

        public void ApplyPowerUp(PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.PowerCore:
                    weaponLevel = Mathf.Min(weaponLevel + 1, 5);
                    if (floatingScorePrefab != null)
                        FloatingScore.SpawnText(floatingScorePrefab, transform.position + Vector3.up * 0.8f,
                            $"WEAPON LV {weaponLevel}!", new Color(0.2f, 1f, 0.5f));
                    if (UIManager.Instance != null) UIManager.Instance.UpdateWeaponLevel(weaponLevel);
                    break;

                case PowerUpType.TripleShot:
                    weaponLevel = Mathf.Min(weaponLevel + 1, 5);
                    if (floatingScorePrefab != null)
                        FloatingScore.SpawnText(floatingScorePrefab, transform.position + Vector3.up * 0.8f,
                            "WEAPON UPGRADE!", new Color(1f, 0.9f, 0.1f));
                    if (UIManager.Instance != null) UIManager.Instance.UpdateWeaponLevel(weaponLevel);
                    break;

                case PowerUpType.Shield:
                    HasShield = true;
                    if (shieldVisual != null) shieldVisual.SetActive(true);
                    if (floatingScorePrefab != null)
                        FloatingScore.SpawnText(floatingScorePrefab, transform.position + Vector3.up * 0.8f,
                            "SHIELD ACTIVE!", new Color(0.2f, 0.8f, 1f));
                    break;

                case PowerUpType.Health:
                    if (currentLives < maxLives)
                    {
                        currentLives++;
                        UpdateDamageOverlay();
                        if (UIManager.Instance != null) UIManager.Instance.UpdateLives(currentLives);
                        if (PostProcessingManager.Instance != null)
                            PostProcessingManager.Instance.UpdateHealthVignette(currentLives, maxLives);
                    }

                    if (floatingScorePrefab != null)
                        FloatingScore.SpawnText(floatingScorePrefab, transform.position + Vector3.up * 0.8f, "+1 LIFE!",
                            new Color(0.3f, 1f, 0.4f));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public void TakeDamage(int damage = 1)
        {
            if (_isDead || IsInvulnerable) return;

            // Absorb hit with shield
            if (HasShield)
            {
                HasShield = false;
                if (shieldVisual) shieldVisual.SetActive(false);

                if (AudioManager.Instance) AudioManager.Instance.PlayShieldDown();
                if (CameraShake.Instance) CameraShake.Instance.Shake(0.2f, 0.15f);

                if (floatingScorePrefab)
                    FloatingScore.SpawnText(floatingScorePrefab, transform.position + Vector3.up * 0.8f,
                        "SHIELD BROKEN!",
                        Color.cyan);

                StartCoroutine(InvulnerabilityFlash(0.6f));
                return;
            }

            currentLives -= damage;
            TriggerRumble(0.6f, 0.75f, 0.25f);
            if (PostProcessingManager.Instance != null)
                PostProcessingManager.Instance.UpdateHealthVignette(currentLives, maxLives);

            // Weapon downgrade on damage
            if (weaponLevel > 1)
            {
                weaponLevel--;
                if (UIManager.Instance != null) UIManager.Instance.UpdateWeaponLevel(weaponLevel);
                if (floatingScorePrefab != null)
                    FloatingScore.SpawnText(floatingScorePrefab, transform.position + Vector3.up * 0.8f,
                        $"WEAPON DOWNGRADE (LV {weaponLevel})", Color.red);
            }

            UpdateDamageOverlay();

            if (CameraShake.Instance) CameraShake.Instance.Shake(0.35f, 0.25f);
            if (AudioManager.Instance) AudioManager.Instance.PlayShieldDown();
            if (UIManager.Instance) UIManager.Instance.UpdateLives(currentLives);

            if (currentLives <= 0)
                Die();
            else
                StartCoroutine(InvulnerabilityFlash(invulnerableDuration));
        }

        private void UpdateDamageOverlay()
        {
            if (damageOverlayRenderer == null) return;

            switch (currentLives)
            {
                case >= 3:
                    damageOverlayRenderer.enabled = false;
                    break;
                case 2:
                {
                    damageOverlayRenderer.enabled = true;
                    if (damageSpriteTier1 != null) damageOverlayRenderer.sprite = damageSpriteTier1;
                    break;
                }
                case 1:
                {
                    damageOverlayRenderer.enabled = true;
                    if (damageSpriteTier2 != null) damageOverlayRenderer.sprite = damageSpriteTier2;
                    break;
                }
            }
        }

        private void EnsureDamageOverlay()
        {
            if (damageOverlayRenderer != null) return;

            var child = transform.Find("DamageOverlay");
            if (child != null)
            {
                damageOverlayRenderer = child.GetComponent<SpriteRenderer>();
            }
            else
            {
                var overlayGo = new GameObject("DamageOverlay");
                overlayGo.transform.SetParent(transform);
                overlayGo.transform.localPosition = Vector3.zero;
                overlayGo.transform.localRotation = Quaternion.identity;
                overlayGo.transform.localScale = Vector3.one;

                damageOverlayRenderer = overlayGo.AddComponent<SpriteRenderer>();
                damageOverlayRenderer.sortingOrder = _spriteRenderer ? _spriteRenderer.sortingOrder + 1 : 11;
                damageOverlayRenderer.enabled = false;
            }

            // Load Kenney damage sprites if not assigned
            if (damageSpriteTier1 == null)
                damageSpriteTier1 = Resources.Load<Sprite>("playerShip1_damage1");
            if (damageSpriteTier2 == null)
                damageSpriteTier2 = Resources.Load<Sprite>("playerShip1_damage3");
        }

        private IEnumerator InvulnerabilityFlash(float duration)
        {
            _isInvulnerable = true;
            var elapsed = 0f;
            const float interval = 0.12f;

            while (elapsed < duration)
            {
                if (_spriteRenderer)
                {
                    var c = _spriteRenderer.color;
                    c.a = Mathf.Approximately(c.a, 1f) ? 0.3f : 1f;
                    _spriteRenderer.color = c;
                }

                yield return new WaitForSeconds(interval);
                elapsed += interval;
            }

            if (_spriteRenderer)
            {
                var c = _spriteRenderer.color;
                c.a = 1f;
                _spriteRenderer.color = c;
            }

            _isInvulnerable = false;
        }

        private void HandleBombInput()
        {
            var bombPressed = false;
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            if (keyboard != null && keyboard.bKey.wasPressedThisFrame) bombPressed = true;
            if (mouse != null && mouse.rightButton.wasPressedThisFrame) bombPressed = true;
#else
        if (Input.GetKeyDown(KeyCode.B) || Input.GetMouseButtonDown(1)) bombPressed = true;
#endif
            if (bombPressed) UseBomb();
        }

        public void UseBomb()
        {
            if (_isDead || currentBombs <= 0) return;
            if (GameManager.Instance && (!GameManager.Instance.IsGameStarted || GameManager.Instance.IsGameOver ||
                                         GameManager.Instance.IsPaused)) return;

            currentBombs--;

            if (UIManager.Instance) UIManager.Instance.UpdateBombs(currentBombs);

            if (shockwavePrefab) Instantiate(shockwavePrefab, transform.position, Quaternion.identity);

            if (PostProcessingManager.Instance != null)
                PostProcessingManager.Instance.TriggerEmpShockwaveGlitch(0.88f, 0.45f);

            if (AudioManager.Instance) AudioManager.Instance.PlayEmpBomb();

            if (AchievementManager.Instance) AchievementManager.Instance.UnlockAchievement("NUKE_HERO");

            if (floatingScorePrefab)
                FloatingScore.SpawnText(floatingScorePrefab, transform.position + Vector3.up * 0.8f, "EMP SHOCKWAVE!",
                    new Color(0.3f, 0.9f, 1f));
        }

        public void AddBomb(int amount = 1)
        {
            currentBombs = Mathf.Min(currentBombs + amount, maxBombs);
            if (UIManager.Instance) UIManager.Instance.UpdateBombs(currentBombs);
        }

        private void Die()
        {
            if (_isDead) return;
            _isDead = true;

            if (CameraShake.Instance) CameraShake.Instance.Shake(0.5f, 0.35f);

            if (explosionPrefab) Instantiate(explosionPrefab, transform.position, Quaternion.identity);

            if (GameManager.Instance) GameManager.Instance.GameOver();

            gameObject.SetActive(false);
        }

        public void OnPerkAcquired(PerkType perkType)
        {
            switch (perkType)
            {
                case PerkType.CompanionDrone:
                    SpawnCompanionDrone();
                    break;
                case PerkType.AfterburnerBlast:
                    hasAfterburner = true;
                    break;
                case PerkType.TeslaArc:
                case PerkType.ClusterRockets:
                case PerkType.SuperMagnet:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(perkType), perkType, null);
            }
        }

        private void SpawnCompanionDrone()
        {
            var droneGo = new GameObject("CompanionDrone_Active")
            {
                transform =
                {
                    position = transform.position
                }
            };
            var sr = droneGo.AddComponent<SpriteRenderer>();
            if (_spriteRenderer != null && _spriteRenderer.sprite != null)
            {
                sr.sprite = _spriteRenderer.sprite;
                sr.color = new Color(0.2f, 1f, 0.7f, 0.9f);
            }

            sr.sortingOrder = 14;
            droneGo.transform.localScale = Vector3.one * 0.45f;

            var drone = droneGo.AddComponent<CompanionDrone>();
            drone.plasmaPrefab = laserPrefab;
            var existingDrones = FindObjectsByType<CompanionDrone>(FindObjectsInactive.Exclude);
            drone.angleOffset = (existingDrones.Length - 1) * 180f;
        }

        private static void TriggerAfterburnerBlast(Vector3 pos)
        {
            HitSparkEffect.SpawnSpark(pos, new Color(1f, 0.45f, 0.1f));

            var count = Physics2D.OverlapCircleNonAlloc(pos, 0.95f, OverlapBuffer);
            for (var i = 0; i < count; i++)
            {
                var h = OverlapBuffer[i];
                if (h == null) continue;

                var enemyLaser = h.GetComponent<Laser>();
                if (enemyLaser != null && enemyLaser.isEnemyLaser)
                {
                    ObjectPoolManager.Despawn(enemyLaser.gameObject);
                    HitSparkEffect.SpawnSpark(enemyLaser.transform.position, Color.yellow);
                    continue;
                }

                var enemy = h.GetComponentInParent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeHitWithDamage(2);
                    continue;
                }

                var meteor = h.GetComponentInParent<SplittingMeteor>();
                if (meteor != null) meteor.TakeHitWithDamage(2);
            }
        }

        private void TriggerRumble(float lowFrequency, float highFrequency, float duration)
        {
#if ENABLE_INPUT_SYSTEM
            var gamepad = Gamepad.current;
            if (gamepad == null) return;

            if (_rumbleCoroutine != null) StopCoroutine(_rumbleCoroutine);
            _rumbleCoroutine = StartCoroutine(RumbleRoutine(gamepad, lowFrequency, highFrequency, duration));
#endif
        }

#if ENABLE_INPUT_SYSTEM
        private IEnumerator RumbleRoutine(Gamepad gamepad, float low, float high, float duration)
        {
            gamepad.SetMotorSpeeds(low, high);
            yield return new WaitForSeconds(duration);
            gamepad.SetMotorSpeeds(0f, 0f);
            _rumbleCoroutine = null;
        }
#endif
    }
}