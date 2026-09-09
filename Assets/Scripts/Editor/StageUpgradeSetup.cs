using System;
using System.Collections.Generic;
using System.IO;
using Combat;
using Core;
using Enemies;
using Environment;
using Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Editor
{
    public static class StageUpgradeSetup
    {
        private const string KenneyPath = "Assets/kenney_space-shooter-remastered";
        private const string PrefabsPath = "Assets/Prefabs";

        [MenuItem("Space Defender/Upgrade/Apply All 4 Stages", false, 0)]
        public static void ApplyAllStages()
        {
            Debug.Log("Starting Space Defender 4-Stage Upgrade Setup...");

            if (!Directory.Exists(PrefabsPath)) Directory.CreateDirectory(PrefabsPath);

            var defaultMat = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Line.mat");
            if (defaultMat == null)
            {
            }

            // 1. Create StarPickup Prefab
            var starSprite = LoadSprite($"{KenneyPath}/PNG/Power-ups/star_gold.png");
            if (starSprite == null) starSprite = LoadSprite($"{KenneyPath}/PNG/Power-ups/powerupYellow_star.png");
            var starPrefab = CreateStarPickupPrefab(starSprite);

            // 2. Create PowerCore Prefab
            var boltSprite = LoadSprite($"{KenneyPath}/PNG/Power-ups/powerupBlue_bolt.png");
            if (boltSprite == null) boltSprite = LoadSprite($"{KenneyPath}/PNG/Power-ups/bolt_gold.png");
            var powerCorePrefab = CreatePowerCorePrefab(boltSprite);

            // 3. Create HomingMissile Prefab
            var missileSprite = LoadSprite($"{KenneyPath}/PNG/Lasers/laserRed08.png");
            if (missileSprite == null) missileSprite = LoadSprite($"{KenneyPath}/PNG/Lasers/laserBlue08.png");
            var explosionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsPath}/ExplosionVFX.prefab");
            var missilePrefab = CreateHomingMissilePrefab(missileSprite, explosionPrefab);

            // 4. Create Splitting Meteor Prefabs (Small -> Med -> Big)
            var meteorSmallSprite = LoadSprite($"{KenneyPath}/PNG/Meteors/meteorBrown_small1.png");
            var meteorMedSprite = LoadSprite($"{KenneyPath}/PNG/Meteors/meteorBrown_med1.png");
            var meteorBigSprite = LoadSprite($"{KenneyPath}/PNG/Meteors/meteorBrown_big1.png");

            var meteorSmall = CreateMeteorPrefab("SplittingMeteor_Small", meteorSmallSprite, MeteorSizeTier.Small, 1,
                3.5f, 50f, null, null, starPrefab, explosionPrefab);
            var meteorMed = CreateMeteorPrefab("SplittingMeteor_Med", meteorMedSprite, MeteorSizeTier.Medium, 2, 2.8f,
                40f, null, meteorSmall, starPrefab, explosionPrefab);
            var meteorBig = CreateMeteorPrefab("SplittingMeteor_Big", meteorBigSprite, MeteorSizeTier.Big, 3, 2.2f, 30f,
                meteorMed, meteorSmall, starPrefab, explosionPrefab);

            // 5. Update Enemy Prefabs with Stars & PowerCore
            UpdateEnemyPrefabs(starPrefab, powerCorePrefab);

            // 6. Update Boss Prefab
            UpdateBossPrefab(starPrefab, powerCorePrefab);

            // 7. Update Player Prefab
            UpdatePlayerPrefab(missilePrefab);

            // 8. Update Scene GameObjects
            UpdateSceneObjects(missilePrefab, meteorBig);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("<color=#00FF88><b>All 4 Stages Upgrade Setup Completed Successfully!</b></color>");
        }

        private static Sprite LoadSprite(string path)
        {
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti != null && ti.textureType != TextureImporterType.Sprite)
            {
                ti.textureType = TextureImporterType.Sprite;
                ti.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static GameObject CreateStarPickupPrefab(Sprite sprite)
        {
            var path = $"{PrefabsPath}/StarPickup.prefab";
            var go = new GameObject("StarPickup");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 9;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.25f;

            var star = go.AddComponent<StarPickup>();
            star.starValue = 1;
            star.scoreBonus = 25;
            star.fallSpeed = 2.2f;

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreatePowerCorePrefab(Sprite sprite)
        {
            var path = $"{PrefabsPath}/PowerUp_PowerCore.prefab";
            var go = new GameObject("PowerUp_PowerCore");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 8;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.3f;

            var pup = go.AddComponent<PowerUp>();
            pup.powerUpType = PowerUpType.PowerCore;
            pup.fallSpeed = 2.0f;

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreateHomingMissilePrefab(Sprite sprite, GameObject explosionPrefab)
        {
            var path = $"{PrefabsPath}/HomingMissile.prefab";
            var go = new GameObject("HomingMissile");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 15;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.25f;

            var missile = go.AddComponent<HomingMissile>();
            missile.initialSpeed = 5f;
            missile.maxSpeed = 14f;
            missile.directDamage = 3;
            missile.blastRadius = 1.5f;
            missile.blastDamage = 2;
            missile.explosionPrefab = explosionPrefab;

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreateMeteorPrefab(string name, Sprite sprite, MeteorSizeTier tier, int hp,
            float speed, float rotSpeed, GameObject medPrefab, GameObject smallPrefab, GameObject starPrefab,
            GameObject explosionPrefab)
        {
            var path = $"{PrefabsPath}/{name}.prefab";
            var go = new GameObject(name);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 5;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.35f;

            var meteor = go.AddComponent<SplittingMeteor>();
            meteor.sizeTier = tier;
            meteor.maxHp = hp;
            meteor.currentHp = hp;
            meteor.fallSpeed = speed;
            meteor.rotationSpeed = rotSpeed;
            meteor.mediumMeteorPrefab = medPrefab;
            meteor.smallMeteorPrefab = smallPrefab;
            meteor.starPrefab = starPrefab;
            meteor.explosionPrefab = explosionPrefab;

            go.AddComponent<HitFlashEffect>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static void UpdateEnemyPrefabs(GameObject starPrefab, GameObject powerCorePrefab)
        {
            var enemyNames = new[] { "Enemy_Red", "Enemy_Green", "Enemy_Blue" };
            foreach (var name in enemyNames)
            {
                var path = $"{PrefabsPath}/{name}.prefab";
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                var enemy = prefab.GetComponent<Enemy>();
                if (enemy == null) continue;
                enemy.starPrefab = starPrefab;
                enemy.starDropCount = 2;
                enemy.chainExplosionRadius = 1.4f;
                enemy.chainDamage = 2;

                // Add PowerCore to drop candidates
                var currentDrops = new List<GameObject>(enemy.powerUpPrefabs ?? Array.Empty<GameObject>());
                if (powerCorePrefab != null && !currentDrops.Contains(powerCorePrefab))
                {
                    currentDrops.Add(powerCorePrefab);
                    enemy.powerUpPrefabs = currentDrops.ToArray();
                }

                if (prefab.GetComponent<HitFlashEffect>() == null) prefab.AddComponent<HitFlashEffect>();

                EditorUtility.SetDirty(prefab);
            }
        }

        private static void UpdateBossPrefab(GameObject starPrefab, GameObject powerCorePrefab)
        {
            var path = $"{PrefabsPath}/Boss_UFO.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) return;

            var boss = prefab.GetComponent<BossController>();
            if (boss == null) return;
            boss.starPrefab = starPrefab;
            boss.maxHp = 45;
            boss.currentHp = 45;

            var currentDrops = new List<GameObject>(boss.dropPowerUpPrefabs ?? Array.Empty<GameObject>());
            if (powerCorePrefab != null && !currentDrops.Contains(powerCorePrefab))
            {
                currentDrops.Add(powerCorePrefab);
                boss.dropPowerUpPrefabs = currentDrops.ToArray();
            }

            // Add drone prefab from Enemy_Blue
            var dronePrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsPath}/Enemy_Blue.prefab");
            if (dronePrefab != null) boss.dronePrefab = dronePrefab;

            if (boss.bossVariantSprites == null || boss.bossVariantSprites.Length < 4)
                boss.bossVariantSprites = new[]
                {
                    LoadSprite($"{KenneyPath}/PNG/ufoRed.png"),
                    LoadSprite($"{KenneyPath}/PNG/ufoBlue.png"),
                    LoadSprite($"{KenneyPath}/PNG/ufoGreen.png"),
                    LoadSprite($"{KenneyPath}/PNG/ufoYellow.png")
                };

            if (prefab.GetComponent<HitFlashEffect>() == null) prefab.AddComponent<HitFlashEffect>();

            EditorUtility.SetDirty(prefab);
        }

        private static void UpdatePlayerPrefab(GameObject missilePrefab)
        {
            var path = $"{PrefabsPath}/Player.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) return;

            var pc = prefab.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.homingMissilePrefab = missilePrefab;
                pc.damageSpriteTier1 = LoadSprite($"{KenneyPath}/PNG/Damage/playerShip1_damage1.png");
                pc.damageSpriteTier2 = LoadSprite($"{KenneyPath}/PNG/Damage/playerShip1_damage3.png");
                pc.dashSpeedMultiplier = 2.5f;
                pc.dashDuration = 0.15f;
                pc.dashCooldown = 1.2f;

                EditorUtility.SetDirty(prefab);
            }
        }

        private static void UpdateSceneObjects(GameObject missilePrefab, GameObject meteorBig)
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.isLoaded) return;

            // Ensure Core Managers exist in Scene
            EnsureManager<HitstopManager>("HitstopManager");
            EnsureManager<ObjectPoolManager>("ObjectPoolManager");
            EnsureManager<LeaderboardManager>("LeaderboardManager");

            // Update Player in scene
            var player = Object.FindAnyObjectByType<PlayerController>();
            if (player != null)
            {
                player.homingMissilePrefab = missilePrefab;
                player.damageSpriteTier1 = LoadSprite($"{KenneyPath}/PNG/Damage/playerShip1_damage1.png");
                player.damageSpriteTier2 = LoadSprite($"{KenneyPath}/PNG/Damage/playerShip1_damage3.png");
                EditorUtility.SetDirty(player.gameObject);
            }

            // Update EnemySpawner in scene
            var spawner = Object.FindAnyObjectByType<EnemySpawner>();
            if (spawner != null)
            {
                spawner.splittingMeteorPrefab = meteorBig;
                EditorUtility.SetDirty(spawner.gameObject);
            }

            // Update Background to Parallax
            SetupParallaxSceneBackground();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void EnsureManager<T>(string name) where T : MonoBehaviour
        {
            var existing = Object.FindAnyObjectByType<T>(FindObjectsInactive.Include);
            if (existing == null)
            {
                var go = new GameObject(name);
                go.AddComponent<T>();
                EditorUtility.SetDirty(go);
                Debug.Log($"Created manager: {name}");
            }
        }

        private static void SetupParallaxSceneBackground()
        {
            var bgObj = GameObject.Find("Background");
            if (bgObj == null) bgObj = new GameObject("Background");

            var parallax = bgObj.GetComponent<ParallaxBackground>();
            if (parallax == null) parallax = bgObj.AddComponent<ParallaxBackground>();

            // Layer 1: Far Nebula (dark purple)
            var nebulaSprite = LoadSprite($"{KenneyPath}/Backgrounds/darkPurple.png");
            if (nebulaSprite == null) nebulaSprite = LoadSprite($"{KenneyPath}/Backgrounds/black.png");

            var layer1Tile1 = FindOrCreateChild(bgObj.transform, "FarNebula_1", nebulaSprite, -10f, 0.4f);
            var layer1Tile2 = FindOrCreateChild(bgObj.transform, "FarNebula_2", nebulaSprite, -10f, 0.4f);
            parallax.farNebulaLayer.tile1 = layer1Tile1;
            parallax.farNebulaLayer.tile2 = layer1Tile2;
            parallax.farNebulaLayer.speedMultiplier = 0.35f;

            // Layer 2: Mid-Stars (blue starfield)
            var midSprite = LoadSprite($"{KenneyPath}/Backgrounds/blue.png");
            if (midSprite == null) midSprite = nebulaSprite;

            var layer2Tile1 = FindOrCreateChild(bgObj.transform, "MidStars_1", midSprite, -5f, 0.35f);
            var layer2Tile2 = FindOrCreateChild(bgObj.transform, "MidStars_2", midSprite, -5f, 0.35f);
            parallax.midStarsLayer.tile1 = layer2Tile1;
            parallax.midStarsLayer.tile2 = layer2Tile2;
            parallax.midStarsLayer.speedMultiplier = 1.0f;

            EditorUtility.SetDirty(bgObj);
        }

        private static Transform FindOrCreateChild(Transform parent, string name, Sprite sprite, float zPos,
            float alpha)
        {
            var child = parent.Find(name);
            if (child == null)
            {
                var go = new GameObject(name);
                go.transform.SetParent(parent);
                child = go.transform;
            }

            var sr = child.GetComponent<SpriteRenderer>();
            if (sr == null) sr = child.gameObject.AddComponent<SpriteRenderer>();
            if (sprite != null) sr.sprite = sprite;
            sr.color = new Color(1f, 1f, 1f, alpha);
            sr.sortingOrder = -20;
            child.localPosition = new Vector3(0f, 0f, zPos);

            return child;
        }
    }
}