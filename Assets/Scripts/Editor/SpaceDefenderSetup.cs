using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SpaceDefender.Editor
{
    public static class SpaceDefenderSetup
    {
        private const string KenneyBasePath = "Assets/kenney_space-shooter-remastered";
        private const string PrefabFolderPath = "Assets/Prefabs";

        [MenuItem("Space Defender/Test/Start Game", false, 10)]
        public static void TestStartGame()
        {
            if (GameManager.Instance != null) GameManager.Instance.StartGame();
        }

        [MenuItem("Space Defender/Test/Toggle Pause", false, 11)]
        public static void TestTogglePause()
        {
            if (GameManager.Instance != null) GameManager.Instance.TogglePause();
        }

        [MenuItem("Space Defender/Test/Take 1 Damage", false, 12)]
        public static void TestTakeDamage()
        {
            PlayerController pc = Object.FindAnyObjectByType<PlayerController>();
            if (pc != null) pc.TakeDamage(1);
        }

        [MenuItem("Space Defender/Test/Trigger Game Over", false, 13)]
        public static void TestGameOver()
        {
            if (GameManager.Instance != null) GameManager.Instance.GameOver();
        }

        [MenuItem("Space Defender/Test/Give Triple Shot", false, 14)]
        public static void TestGiveTripleShot()
        {
            PlayerController pc = Object.FindAnyObjectByType<PlayerController>();
            if (pc != null) pc.ApplyPowerUp(PowerUpType.TripleShot);
        }

        [MenuItem("Space Defender/Test/Give Shield", false, 15)]
        public static void TestGiveShield()
        {
            PlayerController pc = Object.FindAnyObjectByType<PlayerController>();
            if (pc != null) pc.ApplyPowerUp(PowerUpType.Shield);
        }

        [MenuItem("Space Defender/Test/Spawn Boss", false, 16)]
        public static void TestSpawnBoss()
        {
            EnemySpawner spawner = Object.FindAnyObjectByType<EnemySpawner>();
            if (spawner != null && spawner.bossPrefab != null)
            {
                Object.Instantiate(spawner.bossPrefab, new Vector3(0, 5.5f, 0), Quaternion.identity);
            }
        }

        [MenuItem("Space Defender/Test/Toggle AutoFire", false, 17)]
        public static void TestToggleAutoFire()
        {
            PlayerController pc = Object.FindAnyObjectByType<PlayerController>();
            if (pc != null) pc.autoFireForDemo = !pc.autoFireForDemo;
        }

        [MenuItem("Space Defender/Test/Open How To Play", false, 18)]
        public static void TestOpenHowToPlay()
        {
            if (UIManager.Instance != null) UIManager.Instance.ShowHowToPlay(true);
        }

        [MenuItem("Space Defender/Setup Game Scene", false, 1)]
        public static void BuildWholeGame()
        {
            Debug.Log("<color=cyan>[Space Defender]</color> Starting automated full game setup (Demo standard)...");

            EditorSettings.enterPlayModeOptionsEnabled = false;

            EnsureDirectory(PrefabFolderPath);

            // 1. Audio Clips
            AudioClip shootClip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{KenneyBasePath}/Bonus/sfx_laser1.ogg");
            AudioClip enemyShootClip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{KenneyBasePath}/Bonus/sfx_laser2.ogg");
            AudioClip explosionClip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{KenneyBasePath}/Bonus/sfx_zap.ogg");
            AudioClip shieldDownClip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{KenneyBasePath}/Bonus/sfx_shieldDown.ogg");
            AudioClip powerUpClip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{KenneyBasePath}/Bonus/sfx_shieldUp.ogg");
            AudioClip gameOverClip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{KenneyBasePath}/Bonus/sfx_lose.ogg");
            AudioClip buttonClickClip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{KenneyBasePath}/Bonus/sfx_twoTone.ogg");

            // 2. Font & UI Sprites
            Font gameFont = AssetDatabase.LoadAssetAtPath<Font>($"{KenneyBasePath}/Bonus/kenvector_future.ttf");
            Sprite greenBtnSprite = LoadSprite($"{KenneyBasePath}/PNG/UI/buttonGreen.png");
            Sprite blueBtnSprite = LoadSprite($"{KenneyBasePath}/PNG/UI/buttonBlue.png");
            Sprite redBtnSprite = LoadSprite($"{KenneyBasePath}/PNG/UI/buttonRed.png");
            Sprite heartSprite = LoadSprite($"{KenneyBasePath}/PNG/UI/playerLife1_red.png");
            if (heartSprite == null) heartSprite = LoadSprite($"{KenneyBasePath}/PNG/UI/playerLife1_blue.png");

            // 3. Create Floating Score Prefab
            GameObject floatingScorePrefab = CreateFloatingScorePrefab(gameFont);

            // 4. Create Power-up Prefabs
            GameObject pUpTriple = CreatePowerUpPrefab("PowerUp_TripleShot", $"{KenneyBasePath}/PNG/Power-ups/powerupYellow_bolt.png", PowerUpType.TripleShot);
            GameObject pUpShield = CreatePowerUpPrefab("PowerUp_Shield", $"{KenneyBasePath}/PNG/Power-ups/powerupBlue_shield.png", PowerUpType.Shield);
            GameObject pUpHealth = CreatePowerUpPrefab("PowerUp_Health", $"{KenneyBasePath}/PNG/Power-ups/powerupRed_star.png", PowerUpType.Health);
            GameObject[] powerUpPrefabs = new GameObject[] { pUpTriple, pUpShield, pUpHealth };

            // 5. Create Explosion Prefab
            GameObject explosionPrefab = CreateExplosionPrefab();

            // 6. Create Player Laser Prefab
            Sprite laserSprite = LoadSprite($"{KenneyBasePath}/PNG/Lasers/laserBlue01.png");
            GameObject laserPrefab = CreateLaserPrefab(laserSprite);

            // 7. Create Enemy Laser Prefab
            Sprite enemyLaserSprite = LoadSprite($"{KenneyBasePath}/PNG/Lasers/laserRed01.png");
            GameObject enemyLaserPrefab = CreateEnemyLaserPrefab(enemyLaserSprite);

            // 8. Create Boss UFO Prefab
            Sprite bossSprite = LoadSprite($"{KenneyBasePath}/PNG/ufoRed.png");
            GameObject bossPrefab = CreateBossPrefab(bossSprite, enemyLaserPrefab, explosionPrefab, floatingScorePrefab, powerUpPrefabs);

            // 9. Create Enemy Prefabs
            List<GameObject> enemyPrefabs = new List<GameObject>();

            // Ships (can shoot red lasers)
            GameObject enemyRed = CreateEnemyPrefab("Enemy_Red", $"{KenneyBasePath}/PNG/Enemies/enemyRed1.png", 3.2f, 0f, 15, explosionPrefab, true, enemyLaserPrefab, floatingScorePrefab, powerUpPrefabs);
            if (enemyRed != null) enemyPrefabs.Add(enemyRed);

            GameObject enemyGreen = CreateEnemyPrefab("Enemy_Green", $"{KenneyBasePath}/PNG/Enemies/enemyGreen1.png", 3.6f, 0f, 20, explosionPrefab, true, enemyLaserPrefab, floatingScorePrefab, powerUpPrefabs);
            if (enemyGreen != null) enemyPrefabs.Add(enemyGreen);

            GameObject enemyBlue = CreateEnemyPrefab("Enemy_Blue", $"{KenneyBasePath}/PNG/Enemies/enemyBlue1.png", 4.0f, 0f, 25, explosionPrefab, true, enemyLaserPrefab, floatingScorePrefab, powerUpPrefabs);
            if (enemyBlue != null) enemyPrefabs.Add(enemyBlue);

            // Meteors (do not shoot, but rotate)
            GameObject meteorBig = CreateEnemyPrefab("Meteor_Big", $"{KenneyBasePath}/PNG/Meteors/meteorBrown_big1.png", 2.5f, 45f, 10, explosionPrefab, false, null, floatingScorePrefab, powerUpPrefabs);
            if (meteorBig != null) enemyPrefabs.Add(meteorBig);

            GameObject meteorMed = CreateEnemyPrefab("Meteor_Med", $"{KenneyBasePath}/PNG/Meteors/meteorBrown_med1.png", 3.2f, -60f, 15, explosionPrefab, false, null, floatingScorePrefab, powerUpPrefabs);
            if (meteorMed != null) enemyPrefabs.Add(meteorMed);

            GameObject meteorGrey = CreateEnemyPrefab("Meteor_Grey", $"{KenneyBasePath}/PNG/Meteors/meteorGrey_big1.png", 2.8f, 30f, 10, explosionPrefab, false, null, floatingScorePrefab, powerUpPrefabs);
            if (meteorGrey != null) enemyPrefabs.Add(meteorGrey);

            // 10. Create Player Prefab
            Sprite playerSprite = LoadSprite($"{KenneyBasePath}/PNG/playerShip1_blue.png");
            GameObject playerPrefab = CreatePlayerPrefab(playerSprite, laserPrefab, explosionPrefab, floatingScorePrefab);

            // 11. Setup Scene GameObjects
            SetupScene(playerPrefab, laserPrefab, enemyPrefabs.ToArray(), bossPrefab, shootClip, enemyShootClip, explosionClip, shieldDownClip, powerUpClip, gameOverClip, buttonClickClip, gameFont, greenBtnSprite, blueBtnSprite, redBtnSprite, heartSprite);

            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();

            Debug.Log("<color=green>[Space Defender]</color> Setup completed successfully! All assets, prefabs, UI, and scene objects are ready.");
        }

        private static Sprite LoadSprite(string path)
        {
            Sprite spr = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (spr != null) return spr;

            Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(path);
            return allAssets.OfType<Sprite>().FirstOrDefault();
        }

        private static void EnsureDirectory(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = Path.GetDirectoryName(path).Replace('\\', '/');
                string folder = Path.GetFileName(path);
                AssetDatabase.CreateFolder(parent, folder);
            }
        }

        private static Material GetOrCreateParticleMaterial(string matName, string texPath)
        {
            string fullPath = $"{PrefabFolderPath}/{matName}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(fullPath);
            if (mat != null) return mat;

            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default");

            mat = new Material(shader);
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            if (tex != null)
            {
                mat.mainTexture = tex;
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
            }
            mat.SetInt("_Surface", 1);
            mat.SetInt("_Blend", 0);
            mat.renderQueue = 3000;

            AssetDatabase.CreateAsset(mat, fullPath);
            return mat;
        }

        private static GameObject CreateExplosionPrefab()
        {
            string path = $"{PrefabFolderPath}/ExplosionVFX.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            GameObject go = new GameObject("ExplosionVFX");
            ParticleSystem ps = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.duration = 0.5f;
            main.loop = false;
            main.startLifetime = 0.5f;
            main.startSpeed = 4f;
            main.startSize = 0.8f;
            main.startColor = new Color(1f, 0.6f, 0.1f, 1f);
            main.stopAction = ParticleSystemStopAction.Destroy;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 25) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.4f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(1f, 0.9f, 0.2f), 0f), new GradientColorKey(new Color(1f, 0.2f, 0.05f), 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = grad;

            Material particleMat = GetOrCreateParticleMaterial("ExplosionParticleMat", $"{KenneyBasePath}/PNG/Effects/star1.png");
            ParticleSystemRenderer psRenderer = go.GetComponent<ParticleSystemRenderer>();
            if (psRenderer != null)
            {
                psRenderer.sharedMaterial = particleMat;
                psRenderer.sortingOrder = 10;
            }

            go.AddComponent<AutoDestroy>().lifetime = 0.8f;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreateLaserPrefab(Sprite sprite)
        {
            string path = $"{PrefabFolderPath}/Laser.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            GameObject go = new GameObject("Laser");
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 5;

            CapsuleCollider2D col = go.AddComponent<CapsuleCollider2D>();
            col.isTrigger = true;

            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            Laser laser = go.AddComponent<Laser>();
            laser.speed = 12f;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreateEnemyLaserPrefab(Sprite sprite)
        {
            string path = $"{PrefabFolderPath}/EnemyLaser.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            GameObject go = new GameObject("EnemyLaser");
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 4;

            CapsuleCollider2D col = go.AddComponent<CapsuleCollider2D>();
            col.isTrigger = true;

            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            EnemyLaser laser = go.AddComponent<EnemyLaser>();
            laser.speed = 6.5f;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreateFloatingScorePrefab(Font font)
        {
            string path = $"{PrefabFolderPath}/FloatingScore.prefab";

            GameObject go = new GameObject("FloatingScore");
            TextMesh tm = go.AddComponent<TextMesh>();
            tm.text = "+10";
            if (font != null)
            {
                tm.font = font;
                MeshRenderer mr = go.GetComponent<MeshRenderer>();
                if (mr != null) mr.sharedMaterial = font.material;
            }
            tm.fontSize = 36;
            tm.characterSize = 0.15f;
            tm.alignment = TextAlignment.Center;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.color = new Color(1f, 0.92f, 0.23f, 1f);

            MeshRenderer renderer = go.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = 30;
            }

            FloatingScore fs = go.AddComponent<FloatingScore>();
            fs.floatSpeed = 2.0f;
            fs.fadeDuration = 0.75f;
            fs.defaultColor = new Color(1f, 0.92f, 0.23f, 1f);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreatePowerUpPrefab(string name, string spritePath, PowerUpType type)
        {
            string path = $"{PrefabFolderPath}/{name}.prefab";

            GameObject go = new GameObject(name);
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite(spritePath);
            sr.sortingOrder = 8;

            CircleCollider2D col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            if (sr.sprite != null)
            {
                col.radius = Mathf.Min(sr.sprite.bounds.extents.x, sr.sprite.bounds.extents.y) * 0.9f;
            }

            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            PowerUp pup = go.AddComponent<PowerUp>();
            pup.powerUpType = type;
            pup.fallSpeed = 2.2f;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreateBossPrefab(
            Sprite ufoSprite,
            GameObject enemyLaserPrefab,
            GameObject explosionPrefab,
            GameObject floatingScorePrefab,
            GameObject[] powerUpPrefabs)
        {
            string path = $"{PrefabFolderPath}/Boss_UFO.prefab";

            GameObject go = new GameObject("Boss_UFO");
            go.transform.localScale = new Vector3(1.5f, 1.5f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ufoSprite;
            sr.sortingOrder = 6;

            CircleCollider2D col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            if (ufoSprite != null)
            {
                col.radius = Mathf.Min(ufoSprite.bounds.extents.x, ufoSprite.bounds.extents.y) * 0.9f;
            }

            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            GameObject leftFire = new GameObject("LeftFirePoint");
            leftFire.transform.SetParent(go.transform);
            leftFire.transform.localPosition = new Vector3(-0.45f, -0.3f, 0f);

            GameObject rightFire = new GameObject("RightFirePoint");
            rightFire.transform.SetParent(go.transform);
            rightFire.transform.localPosition = new Vector3(0.45f, -0.3f, 0f);

            BossController boss = go.AddComponent<BossController>();
            boss.maxHP = 20;
            boss.currentHP = 20;
            boss.scoreValue = 100;
            boss.bossName = "RED UFO MOTHERSHIP";
            boss.enemyLaserPrefab = enemyLaserPrefab;
            boss.leftFirePoint = leftFire.transform;
            boss.rightFirePoint = rightFire.transform;
            boss.explosionPrefab = explosionPrefab;
            boss.floatingScorePrefab = floatingScorePrefab;
            boss.dropPowerUpPrefabs = powerUpPrefabs;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreateEnemyPrefab(
            string name,
            string spritePath,
            float speed,
            float rotSpeed,
            int score,
            GameObject explosionVFX,
            bool canShoot,
            GameObject enemyLaserPrefab,
            GameObject floatingScorePrefab,
            GameObject[] powerUpPrefabs)
        {
            string path = $"{PrefabFolderPath}/{name}.prefab";

            GameObject go = new GameObject(name);
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite(spritePath);
            sr.sortingOrder = 3;

            CircleCollider2D col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            if (sr.sprite != null)
            {
                col.radius = Mathf.Min(sr.sprite.bounds.extents.x, sr.sprite.bounds.extents.y) * 0.85f;
            }

            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            Enemy enemy = go.AddComponent<Enemy>();
            enemy.speed = speed;
            enemy.rotationSpeed = rotSpeed;
            enemy.scoreValue = score;
            enemy.explosionPrefab = explosionVFX;
            enemy.canShoot = canShoot;
            enemy.enemyLaserPrefab = enemyLaserPrefab;
            enemy.floatingScorePrefab = floatingScorePrefab;
            enemy.powerUpPrefabs = powerUpPrefabs;
            enemy.dropChance = 0.25f;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreatePlayerPrefab(
            Sprite sprite,
            GameObject laserPrefab,
            GameObject explosionVFX,
            GameObject floatingScorePrefab)
        {
            string path = $"{PrefabFolderPath}/Player.prefab";

            GameObject go = new GameObject("Player");
            go.tag = "Player";

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 5;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            if (sr.sprite != null)
            {
                col.size = new Vector2(sr.sprite.bounds.size.x * 0.7f, sr.sprite.bounds.size.y * 0.7f);
            }

            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            GameObject firePoint = new GameObject("FirePoint");
            firePoint.transform.SetParent(go.transform);
            firePoint.transform.localPosition = new Vector3(0, 0.6f, 0);

            // Shield visual child object
            GameObject shieldObj = new GameObject("ShieldVisual");
            shieldObj.transform.SetParent(go.transform);
            shieldObj.transform.localPosition = Vector3.zero;
            shieldObj.transform.localScale = new Vector3(1.15f, 1.15f, 1f);
            SpriteRenderer shieldSr = shieldObj.AddComponent<SpriteRenderer>();
            shieldSr.sprite = LoadSprite($"{KenneyBasePath}/PNG/Effects/shield1.png");
            shieldSr.color = new Color(0.3f, 0.85f, 1f, 0.85f);
            shieldSr.sortingOrder = 12;
            shieldObj.SetActive(false);

            // Thruster flame particles
            GameObject thruster = new GameObject("ThrusterParticles");
            thruster.transform.SetParent(go.transform);
            thruster.transform.localPosition = new Vector3(0, -0.45f, 0);
            thruster.transform.localEulerAngles = new Vector3(90, 0, 0);
            ParticleSystem ps = thruster.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.startLifetime = 0.25f;
            main.startSpeed = 2.2f;
            main.startSize = 0.22f;
            main.startColor = new Color(0.3f, 0.85f, 1f, 0.85f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 35;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 15;
            shape.radius = 0.1f;

            Material thrusterMat = GetOrCreateParticleMaterial("ThrusterParticleMat", $"{KenneyBasePath}/PNG/Effects/fire01.png");
            ParticleSystemRenderer thrusterRenderer = thruster.GetComponent<ParticleSystemRenderer>();
            if (thrusterRenderer != null)
            {
                thrusterRenderer.sharedMaterial = thrusterMat;
                thrusterRenderer.sortingOrder = 9;
            }

            PlayerController pc = go.AddComponent<PlayerController>();
            pc.moveSpeed = 9f;
            pc.padding = 0.6f;
            pc.fireRate = 0.22f;
            pc.autoFireForDemo = false;
            pc.laserPrefab = laserPrefab;
            pc.firePoint = firePoint.transform;
            pc.explosionPrefab = explosionVFX;
            pc.shieldVisual = shieldObj;
            pc.floatingScorePrefab = floatingScorePrefab;
            pc.maxLives = 3;
            pc.currentLives = 3;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static void SetupScene(
            GameObject playerPrefab,
            GameObject laserPrefab,
            GameObject[] enemyPrefabs,
            GameObject bossPrefab,
            AudioClip shootClip,
            AudioClip enemyShootClip,
            AudioClip explosionClip,
            AudioClip shieldDownClip,
            AudioClip powerUpClip,
            AudioClip gameOverClip,
            AudioClip buttonClickClip,
            Font gameFont,
            Sprite greenBtnSprite,
            Sprite blueBtnSprite,
            Sprite redBtnSprite,
            Sprite heartSprite)
        {
            // Camera
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                cam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
                camObj.AddComponent<AudioListener>();
            }
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.backgroundColor = new Color(0.04f, 0.04f, 0.12f, 1f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.transform.position = new Vector3(0, 0, -10);

            if (cam.GetComponent<CameraShake>() == null)
            {
                cam.gameObject.AddComponent<CameraShake>();
            }

            // Clean up old instances
            string[] cleanNames = { "Player", "BackgroundScroller", "EnemySpawner", "GameManager", "AudioManager", "UIManager", "Canvas", "EventSystem" };
            var roots = EditorSceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var root in roots)
            {
                if (cleanNames.Contains(root.name))
                {
                    Object.DestroyImmediate(root);
                }
            }

            // 1. Background Scroller with bg1.png
            GameObject bgScrollerObj = new GameObject("BackgroundScroller");
            BackgroundScroller scroller = bgScrollerObj.AddComponent<BackgroundScroller>();
            scroller.scrollSpeed = 1.8f;
            scroller.fitCameraWidth = true;

            Sprite bgSprite = LoadSprite($"{KenneyBasePath}/bg1.png");
            if (bgSprite == null) bgSprite = LoadSprite($"{KenneyBasePath}/Backgrounds/darkPurple.png");

            GameObject bg1 = new GameObject("Background_1");
            bg1.transform.SetParent(bgScrollerObj.transform);
            SpriteRenderer bgSr1 = bg1.AddComponent<SpriteRenderer>();
            bgSr1.sprite = bgSprite;
            bgSr1.sortingOrder = -20;
            bg1.transform.position = new Vector3(0, 0, 5);

            GameObject bg2 = new GameObject("Background_2");
            bg2.transform.SetParent(bgScrollerObj.transform);
            SpriteRenderer bgSr2 = bg2.AddComponent<SpriteRenderer>();
            bgSr2.sprite = bgSprite;
            bgSr2.sortingOrder = -20;
            bg2.transform.position = new Vector3(0, 10, 5);

            scroller.background1 = bg1.transform;
            scroller.background2 = bg2.transform;
            scroller.SetupBackgroundDimensions();

            // 2. Instantiate Player
            GameObject playerObj = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            playerObj.name = "Player";
            playerObj.transform.position = new Vector3(0, -3.8f, 0);
            playerObj.SetActive(true); // Active with thrusters visible on menu screen like Menu.png

            // 3. Enemy Spawner
            GameObject spawnerObj = new GameObject("EnemySpawner");
            EnemySpawner spawner = spawnerObj.AddComponent<EnemySpawner>();
            spawner.enemyPrefabs = enemyPrefabs;
            spawner.bossPrefab = bossPrefab;
            spawner.minSpawnDelay = 0.8f;
            spawner.maxSpawnDelay = 1.6f;

            // 4. Audio Manager
            GameObject audioObj = new GameObject("AudioManager");
            AudioManager audioMgr = audioObj.AddComponent<AudioManager>();
            audioMgr.shootClip = shootClip;
            audioMgr.enemyShootClip = enemyShootClip;
            audioMgr.explosionClip = explosionClip;
            audioMgr.shieldDownClip = shieldDownClip;
            audioMgr.powerUpClip = powerUpClip;
            audioMgr.gameOverClip = gameOverClip;
            audioMgr.buttonClickClip = buttonClickClip;

            // 5. Canvas & UI
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920); // Portrait 9:16 standard matching Demo.png
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();

            // Event System
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            eventSystem.AddComponent<StandaloneInputModule>();
#endif

            // UI Manager Object
            GameObject uiManagerObj = new GameObject("UIManager");
            UIManager uiManager = uiManagerObj.AddComponent<UIManager>();

            // ================== A. IN-GAME HUD ==================
            GameObject inGameHUD = new GameObject("InGameHUD");
            inGameHUD.transform.SetParent(canvasObj.transform, false);
            RectTransform hudRect = inGameHUD.AddComponent<RectTransform>();
            hudRect.anchorMin = Vector2.zero;
            hudRect.anchorMax = Vector2.one;
            hudRect.offsetMin = Vector2.zero;
            hudRect.offsetMax = Vector2.zero;

            // Score Text (Top-Left)
            GameObject scoreObj = new GameObject("ScoreText");
            scoreObj.transform.SetParent(inGameHUD.transform, false);
            Text scoreText = scoreObj.AddComponent<Text>();
            scoreText.text = "SCORE\n0000";
            if (gameFont != null) scoreText.font = gameFont;
            scoreText.fontSize = 36;
            scoreText.lineSpacing = 1.1f;
            scoreText.alignment = TextAnchor.UpperLeft;
            scoreText.color = new Color(0.2f, 0.9f, 1f, 1f);
            scoreObj.AddComponent<Outline>().effectColor = new Color(0, 0, 0, 0.9f);
            RectTransform scoreRect = scoreText.rectTransform;
            scoreRect.anchorMin = new Vector2(0, 1);
            scoreRect.anchorMax = new Vector2(0, 1);
            scoreRect.pivot = new Vector2(0, 1);
            scoreRect.anchoredPosition = new Vector2(40, -40);
            scoreRect.sizeDelta = new Vector2(350, 100);

            // High Score Text (Top-Left below Score)
            GameObject hsObj = new GameObject("HighScoreText");
            hsObj.transform.SetParent(inGameHUD.transform, false);
            Text hsText = hsObj.AddComponent<Text>();
            hsText.text = "BEST: 0000";
            if (gameFont != null) hsText.font = gameFont;
            hsText.fontSize = 24;
            hsText.alignment = TextAnchor.UpperLeft;
            hsText.color = new Color(1f, 0.85f, 0.3f, 1f);
            hsObj.AddComponent<Outline>().effectColor = new Color(0, 0, 0, 0.9f);
            RectTransform hsRect = hsText.rectTransform;
            hsRect.anchorMin = new Vector2(0, 1);
            hsRect.anchorMax = new Vector2(0, 1);
            hsRect.pivot = new Vector2(0, 1);
            hsRect.anchoredPosition = new Vector2(40, -150);
            hsRect.sizeDelta = new Vector2(350, 50);

            // Hearts (Top-Right)
            List<Image> heartList = new List<Image>();
            for (int i = 0; i < 3; i++)
            {
                GameObject heartObj = new GameObject($"Heart_{i + 1}");
                heartObj.transform.SetParent(inGameHUD.transform, false);
                Image hImg = heartObj.AddComponent<Image>();
                if (heartSprite != null) hImg.sprite = heartSprite;
                hImg.preserveAspect = true;

                RectTransform hRect = heartObj.GetComponent<RectTransform>();
                hRect.anchorMin = new Vector2(1, 1);
                hRect.anchorMax = new Vector2(1, 1);
                hRect.pivot = new Vector2(1, 1);
                hRect.anchoredPosition = new Vector2(-150 - (i * 55), -45);
                hRect.sizeDelta = new Vector2(48, 48);

                heartList.Add(hImg);
            }

            // Pause Button (Top-Right next to Hearts)
            GameObject pauseBtnObj = CreateButton(inGameHUD.transform, "PauseBtn", "||", gameFont, blueBtnSprite, new Vector2(-40, -45), new Vector2(70, 70), 32);
            RectTransform pBtnRect = pauseBtnObj.GetComponent<RectTransform>();
            pBtnRect.anchorMin = new Vector2(1, 1);
            pBtnRect.anchorMax = new Vector2(1, 1);
            pBtnRect.pivot = new Vector2(1, 1);
            Button pauseBtn = pauseBtnObj.GetComponent<Button>();

            // Boss Bar HUD (Top Center)
            GameObject bossBarObj = new GameObject("BossBarPanel");
            bossBarObj.transform.SetParent(inGameHUD.transform, false);
            Image bossBarBg = bossBarObj.AddComponent<Image>();
            bossBarBg.color = new Color(0.08f, 0.08f, 0.16f, 0.88f);
            RectTransform bbRect = bossBarObj.GetComponent<RectTransform>();
            bbRect.anchorMin = new Vector2(0.5f, 1f);
            bbRect.anchorMax = new Vector2(0.5f, 1f);
            bbRect.pivot = new Vector2(0.5f, 1f);
            bbRect.anchoredPosition = new Vector2(0, -40);
            bbRect.sizeDelta = new Vector2(500, 85);

            GameObject bTitleObj = new GameObject("BossTitle");
            bTitleObj.transform.SetParent(bossBarObj.transform, false);
            Text bTitleText = bTitleObj.AddComponent<Text>();
            bTitleText.text = "RED UFO MOTHERSHIP";
            if (gameFont != null) bTitleText.font = gameFont;
            bTitleText.fontSize = 20;
            bTitleText.alignment = TextAnchor.MiddleCenter;
            bTitleText.color = new Color(1f, 0.35f, 0.35f);
            bTitleObj.AddComponent<Outline>().effectColor = new Color(0, 0, 0, 0.9f);
            RectTransform btRect = bTitleText.rectTransform;
            btRect.anchoredPosition = new Vector2(0, 18);
            btRect.sizeDelta = new Vector2(480, 30);

            // Slider
            GameObject sliderObj = new GameObject("BossHPSlider");
            sliderObj.transform.SetParent(bossBarObj.transform, false);
            Slider hpSlider = sliderObj.AddComponent<Slider>();
            RectTransform sRect = sliderObj.GetComponent<RectTransform>();
            sRect.anchoredPosition = new Vector2(0, -16);
            sRect.sizeDelta = new Vector2(440, 24);

            GameObject sBgObj = new GameObject("Background");
            sBgObj.transform.SetParent(sliderObj.transform, false);
            Image sBgImg = sBgObj.AddComponent<Image>();
            sBgImg.color = new Color(0.35f, 0.08f, 0.08f);
            RectTransform sBgRect = sBgObj.GetComponent<RectTransform>();
            sBgRect.anchorMin = Vector2.zero;
            sBgRect.anchorMax = Vector2.one;
            sBgRect.offsetMin = Vector2.zero;
            sBgRect.offsetMax = Vector2.zero;

            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObj.transform, false);
            RectTransform faRect = fillArea.AddComponent<RectTransform>();
            faRect.anchorMin = Vector2.zero;
            faRect.anchorMax = Vector2.one;
            faRect.offsetMin = Vector2.zero;
            faRect.offsetMax = Vector2.zero;

            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(fillArea.transform, false);
            Image fillImg = fillObj.AddComponent<Image>();
            fillImg.color = new Color(1f, 0.2f, 0.25f);
            RectTransform fRect = fillObj.GetComponent<RectTransform>();
            fRect.anchorMin = Vector2.zero;
            fRect.anchorMax = Vector2.one;
            fRect.offsetMin = Vector2.zero;
            fRect.offsetMax = Vector2.zero;

            hpSlider.targetGraphic = sBgImg;
            hpSlider.fillRect = fRect;
            hpSlider.minValue = 0f;
            hpSlider.maxValue = 1f;
            hpSlider.value = 1f;

            // Text on slider
            GameObject hpTextObj = new GameObject("HPText");
            hpTextObj.transform.SetParent(sliderObj.transform, false);
            Text hpText = hpTextObj.AddComponent<Text>();
            hpText.text = "20 / 20";
            if (gameFont != null) hpText.font = gameFont;
            hpText.fontSize = 14;
            hpText.alignment = TextAnchor.MiddleCenter;
            hpText.color = Color.white;
            hpTextObj.AddComponent<Outline>().effectColor = new Color(0, 0, 0, 0.9f);
            RectTransform hpTextRect = hpText.rectTransform;
            hpTextRect.anchorMin = Vector2.zero;
            hpTextRect.anchorMax = Vector2.one;
            hpTextRect.offsetMin = Vector2.zero;
            hpTextRect.offsetMax = Vector2.zero;

            bossBarObj.SetActive(false);

            // ================== B. MAIN MENU PANEL ==================
            GameObject mainMenuPanel = new GameObject("MainMenuPanel");
            mainMenuPanel.transform.SetParent(canvasObj.transform, false);
            RectTransform menuRect = mainMenuPanel.AddComponent<RectTransform>();
            menuRect.anchorMin = Vector2.zero;
            menuRect.anchorMax = Vector2.one;
            menuRect.offsetMin = Vector2.zero;
            menuRect.offsetMax = Vector2.zero;

            // Logo Title
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(mainMenuPanel.transform, false);
            Text titleText = titleObj.AddComponent<Text>();
            titleText.text = "KENNEY\n<size=38>SPACE DEFENDER</size>";
            if (gameFont != null) titleText.font = gameFont;
            titleText.fontSize = 68;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.lineSpacing = 1.1f;
            titleText.color = Color.white;
            titleObj.AddComponent<Outline>().effectColor = new Color(0, 0, 0, 0.95f);
            RectTransform titleRect = titleText.rectTransform;
            titleRect.anchoredPosition = new Vector2(0, 360);
            titleRect.sizeDelta = new Vector2(900, 220);

            // Subtitle
            GameObject subObj = new GameObject("SubtitleText");
            subObj.transform.SetParent(mainMenuPanel.transform, false);
            Text subText = subObj.AddComponent<Text>();
            subText.text = "DEFEND THE GALAXY";
            if (gameFont != null) subText.font = gameFont;
            subText.fontSize = 26;
            subText.alignment = TextAnchor.MiddleCenter;
            subText.color = new Color(0.3f, 0.8f, 1f, 1f);
            subObj.AddComponent<Outline>().effectColor = new Color(0, 0, 0, 0.8f);
            RectTransform subRect = subText.rectTransform;
            subRect.anchoredPosition = new Vector2(0, 240);
            subRect.sizeDelta = new Vector2(600, 50);

            // Buttons
            GameObject playBtnObj = CreateButton(mainMenuPanel.transform, "PlayButton", "PLAY", gameFont, greenBtnSprite, new Vector2(0, 40), new Vector2(400, 100), 40);
            Button playBtn = playBtnObj.GetComponent<Button>();

            GameObject howToPlayBtnObj = CreateButton(mainMenuPanel.transform, "HowToPlayButton", "HOW TO PLAY", gameFont, blueBtnSprite, new Vector2(0, -90), new Vector2(400, 100), 32);
            Button howToPlayBtn = howToPlayBtnObj.GetComponent<Button>();

            GameObject exitBtnObj = CreateButton(mainMenuPanel.transform, "ExitButton", "EXIT", gameFont, redBtnSprite, new Vector2(0, -220), new Vector2(400, 100), 38);
            Button exitBtn = exitBtnObj.GetComponent<Button>();

            // How To Play Modal
            GameObject modalObj = new GameObject("HowToPlayModal");
            modalObj.transform.SetParent(mainMenuPanel.transform, false);
            Image modalBg = modalObj.AddComponent<Image>();
            modalBg.color = new Color(0.04f, 0.08f, 0.18f, 0.95f);
            RectTransform modalRect = modalObj.GetComponent<RectTransform>();
            modalRect.anchoredPosition = Vector2.zero;
            modalRect.sizeDelta = new Vector2(800, 850);

            GameObject modalTitle = new GameObject("ModalTitle");
            modalTitle.transform.SetParent(modalObj.transform, false);
            Text mt = modalTitle.AddComponent<Text>();
            mt.text = "HOW TO PLAY";
            if (gameFont != null) mt.font = gameFont;
            mt.fontSize = 44;
            mt.alignment = TextAnchor.MiddleCenter;
            mt.color = new Color(0.3f, 0.85f, 1f);
            modalTitle.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 340);
            modalTitle.GetComponent<RectTransform>().sizeDelta = new Vector2(700, 80);

            GameObject modalContent = new GameObject("ModalContent");
            modalContent.transform.SetParent(modalObj.transform, false);
            Text mc = modalContent.AddComponent<Text>();
            mc.text = "MOVE:\nA / D or Left / Right Arrows\n\nSHOOT:\nSpacebar or Left Mouse Click\n\nPOWER-UPS:\nBolt: Triple Laser (10s)\nShield: Absorbs 1 Hit\nStar: Restores 1 Heart\n\nMINI-BOSS:\nSurvive & defeat the UFO Mothership!\n\nPAUSE: Esc or Pause button";
            if (gameFont != null) mc.font = gameFont;
            mc.fontSize = 22;
            mc.lineSpacing = 1.25f;
            mc.alignment = TextAnchor.MiddleCenter;
            mc.color = Color.white;
            modalContent.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 30);
            modalContent.GetComponent<RectTransform>().sizeDelta = new Vector2(720, 520);

            GameObject closeModalBtnObj = CreateButton(modalObj.transform, "CloseModalBtn", "GOT IT!", gameFont, greenBtnSprite, new Vector2(0, -320), new Vector2(300, 80), 32);
            Button closeModalBtn = closeModalBtnObj.GetComponent<Button>();
            modalObj.SetActive(false);

            // ================== C. PAUSE PANEL ==================
            GameObject pausePanel = new GameObject("PausePanel");
            pausePanel.transform.SetParent(canvasObj.transform, false);
            Image pauseBg = pausePanel.AddComponent<Image>();
            pauseBg.color = new Color(0.03f, 0.05f, 0.12f, 0.9f);
            RectTransform pRect = pausePanel.GetComponent<RectTransform>();
            pRect.anchorMin = Vector2.zero;
            pRect.anchorMax = Vector2.one;
            pRect.offsetMin = Vector2.zero;
            pRect.offsetMax = Vector2.zero;

            GameObject pauseTitle = new GameObject("PauseTitle");
            pauseTitle.transform.SetParent(pausePanel.transform, false);
            Text pt = pauseTitle.AddComponent<Text>();
            pt.text = "PAUSED";
            if (gameFont != null) pt.font = gameFont;
            pt.fontSize = 64;
            pt.alignment = TextAnchor.MiddleCenter;
            pt.color = new Color(0.3f, 0.85f, 1f);
            pauseTitle.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 260);
            pauseTitle.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 100);

            GameObject resumeBtnObj = CreateButton(pausePanel.transform, "ResumeBtn", "RESUME", gameFont, greenBtnSprite, new Vector2(0, 90), new Vector2(380, 90), 36);
            Button resumeBtn = resumeBtnObj.GetComponent<Button>();

            GameObject restartBtnObj = CreateButton(pausePanel.transform, "RestartBtn", "RESTART", gameFont, blueBtnSprite, new Vector2(0, -30), new Vector2(380, 90), 34);
            Button restartBtn = restartBtnObj.GetComponent<Button>();

            GameObject pauseMenuBtnObj = CreateButton(pausePanel.transform, "PauseMenuBtn", "MAIN MENU", gameFont, redBtnSprite, new Vector2(0, -150), new Vector2(380, 90), 32);
            Button pauseMenuBtn = pauseMenuBtnObj.GetComponent<Button>();
            pausePanel.SetActive(false);

            // ================== D. GAME OVER PANEL ==================
            GameObject gameOverPanel = new GameObject("GameOverPanel");
            gameOverPanel.transform.SetParent(canvasObj.transform, false);
            Image goBg = gameOverPanel.AddComponent<Image>();
            goBg.color = new Color(0.04f, 0.02f, 0.08f, 0.92f);
            RectTransform goRect = gameOverPanel.GetComponent<RectTransform>();
            goRect.anchorMin = Vector2.zero;
            goRect.anchorMax = Vector2.one;
            goRect.offsetMin = Vector2.zero;
            goRect.offsetMax = Vector2.zero;

            GameObject goTitle = new GameObject("GameOverTitle");
            goTitle.transform.SetParent(gameOverPanel.transform, false);
            Text got = goTitle.AddComponent<Text>();
            got.text = "GAME OVER";
            if (gameFont != null) got.font = gameFont;
            got.fontSize = 72;
            got.alignment = TextAnchor.MiddleCenter;
            got.color = new Color(1f, 0.25f, 0.25f);
            goTitle.AddComponent<Outline>().effectColor = new Color(0, 0, 0, 0.95f);
            goTitle.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 320);
            goTitle.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 120);

            GameObject newRecordObj = new GameObject("NewRecordBadge");
            newRecordObj.transform.SetParent(gameOverPanel.transform, false);
            Text nrt = newRecordObj.AddComponent<Text>();
            nrt.text = "★ NEW HIGH SCORE! ★";
            if (gameFont != null) nrt.font = gameFont;
            nrt.fontSize = 32;
            nrt.alignment = TextAnchor.MiddleCenter;
            nrt.color = new Color(1f, 0.9f, 0.2f);
            newRecordObj.AddComponent<Outline>().effectColor = new Color(0.6f, 0.2f, 0f, 0.9f);
            newRecordObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 200);
            newRecordObj.GetComponent<RectTransform>().sizeDelta = new Vector2(700, 60);

            GameObject goFinalScoreObj = new GameObject("FinalScoreText");
            goFinalScoreObj.transform.SetParent(gameOverPanel.transform, false);
            Text fsText = goFinalScoreObj.AddComponent<Text>();
            fsText.text = "YOUR SCORE\n0";
            if (gameFont != null) fsText.font = gameFont;
            fsText.fontSize = 44;
            fsText.alignment = TextAnchor.MiddleCenter;
            fsText.lineSpacing = 1.15f;
            fsText.color = Color.white;
            goFinalScoreObj.AddComponent<Outline>().effectColor = new Color(0, 0, 0, 0.9f);
            goFinalScoreObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 70);
            goFinalScoreObj.GetComponent<RectTransform>().sizeDelta = new Vector2(700, 130);

            GameObject goHighScoreObj = new GameObject("GameOverHighScore");
            goHighScoreObj.transform.SetParent(gameOverPanel.transform, false);
            Text gohsText = goHighScoreObj.AddComponent<Text>();
            gohsText.text = "BEST: 0";
            if (gameFont != null) gohsText.font = gameFont;
            gohsText.fontSize = 32;
            gohsText.alignment = TextAnchor.MiddleCenter;
            gohsText.color = new Color(1f, 0.85f, 0.3f);
            goHighScoreObj.AddComponent<Outline>().effectColor = new Color(0, 0, 0, 0.9f);
            goHighScoreObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -30);
            goHighScoreObj.GetComponent<RectTransform>().sizeDelta = new Vector2(700, 60);

            GameObject replayBtnObj = CreateButton(gameOverPanel.transform, "ReplayBtn", "PLAY AGAIN", gameFont, greenBtnSprite, new Vector2(0, -150), new Vector2(400, 95), 36);
            Button replayBtn = replayBtnObj.GetComponent<Button>();

            GameObject goMenuBtnObj = CreateButton(gameOverPanel.transform, "GOMenuBtn", "MAIN MENU", gameFont, blueBtnSprite, new Vector2(0, -270), new Vector2(400, 95), 32);
            Button goMenuBtn = goMenuBtnObj.GetComponent<Button>();
            gameOverPanel.SetActive(false);

            // Wire UIManager
            uiManager.mainMenuPanel = mainMenuPanel;
            uiManager.playButton = playBtn;
            uiManager.howToPlayButton = howToPlayBtn;
            uiManager.howToPlayModal = modalObj;
            uiManager.closeHowToPlayButton = closeModalBtn;
            uiManager.exitButton = exitBtn;

            uiManager.inGameHUD = inGameHUD;
            uiManager.scoreText = scoreText;
            uiManager.highScoreText = hsText;
            uiManager.heartImages = heartList.ToArray();
            uiManager.pauseButton = pauseBtn;
            uiManager.bossBarPanel = bossBarObj;
            uiManager.bossHPSlider = hpSlider;
            uiManager.bossNameText = bTitleText;
            uiManager.bossHPText = hpText;

            uiManager.pausePanel = pausePanel;
            uiManager.resumeButton = resumeBtn;
            uiManager.restartButton = restartBtn;
            uiManager.pauseMainMenuButton = pauseMenuBtn;

            uiManager.gameOverPanel = gameOverPanel;
            uiManager.finalScoreText = fsText;
            uiManager.gameOverHighScoreText = gohsText;
            uiManager.newRecordObject = newRecordObj;
            uiManager.replayButton = replayBtn;
            uiManager.gameOverMainMenuButton = goMenuBtn;

            uiManager.ShowMainMenu();

            // 6. Game Manager
            GameObject gmObj = new GameObject("GameManager");
            GameManager gm = gmObj.AddComponent<GameManager>();
            gm.showMainMenuOnStart = true;
        }

        private static GameObject CreateButton(Transform parent, string name, string text, Font font, Sprite sprite, Vector2 pos, Vector2 size, int fontSize)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            Image img = btnObj.AddComponent<Image>();
            if (sprite != null)
            {
                img.sprite = sprite;
                img.type = Image.Type.Sliced;
            }
            else
            {
                img.color = new Color(0.2f, 0.5f, 0.9f);
            }

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.highlightedColor = new Color(1f, 0.95f, 0.6f);
            cb.pressedColor = new Color(0.75f, 0.75f, 0.75f);
            btn.colors = cb;

            RectTransform rt = btnObj.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            Text t = textObj.AddComponent<Text>();
            t.text = text;
            if (font != null) t.font = font;
            t.fontSize = fontSize;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = new Color(0.1f, 0.1f, 0.15f);

            RectTransform trt = textObj.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;

            return btnObj;
        }
    }
}
