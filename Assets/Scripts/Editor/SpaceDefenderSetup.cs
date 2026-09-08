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

        [MenuItem("Space Defender/Test/Use EMP Bomb", false, 18)]
        public static void TestUseBomb()
        {
            PlayerController pc = Object.FindAnyObjectByType<PlayerController>();
            if (pc != null) pc.UseBomb();
        }

        [MenuItem("Space Defender/Test/Add 5x Combo", false, 18)]
        public static void TestAddCombo()
        {
            if (ComboManager.Instance != null)
            {
                for (int i = 0; i < 5; i++)
                {
                    ComboManager.Instance.RegisterKill(20, Vector3.zero);
                }
            }
        }

        [MenuItem("Space Defender/Test/Open Hangar", false, 19)]
        public static void TestOpenHangar()
        {
            UIManager ui = UIManager.Instance != null ? UIManager.Instance : Object.FindAnyObjectByType<UIManager>();
            if (ui != null) ui.ShowHangar(true);
        }

        [MenuItem("Space Defender/Test/Open Achievements", false, 20)]
        public static void TestOpenAchievements()
        {
            UIManager ui = UIManager.Instance != null ? UIManager.Instance : Object.FindAnyObjectByType<UIManager>();
            if (ui != null) ui.ShowAchievements(true);
        }

        [MenuItem("Space Defender/Test/Open Settings", false, 21)]
        public static void TestOpenSettings()
        {
            UIManager ui = UIManager.Instance != null ? UIManager.Instance : Object.FindAnyObjectByType<UIManager>();
            if (ui != null) ui.ShowSettings(true);
        }

        [MenuItem("Space Defender/Test/Return To Main Menu", false, 22)]
        public static void TestReturnToMainMenu()
        {
            GameManager gm = GameManager.Instance != null ? GameManager.Instance : Object.FindAnyObjectByType<GameManager>();
            if (gm != null) gm.ReturnToMainMenu();
        }

        [MenuItem("Space Defender/Setup Game Scene", false, 1)]
        public static void SetupGameScene()
        {
            if (!Directory.Exists(PrefabFolderPath))
            {
                Directory.CreateDirectory(PrefabFolderPath);
            }

            // 1. Audio Clips
            AudioClip shootClip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{KenneyBasePath}/Bonus/sfx_laser1.ogg");
            AudioClip enemyShootClip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{KenneyBasePath}/Bonus/sfx_laser2.ogg");
            AudioClip explosionClip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{KenneyBasePath}/Bonus/sfx_zap.ogg");
            AudioClip shieldDownClip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{KenneyBasePath}/Bonus/sfx_shieldDown.ogg");
            AudioClip powerUpClip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{KenneyBasePath}/Bonus/sfx_shieldUp.ogg");
            AudioClip gameOverClip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{KenneyBasePath}/Bonus/sfx_lose.ogg");
            AudioClip buttonClickClip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{KenneyBasePath}/Bonus/sfx_twoTone.ogg");
            AudioClip empBombClip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{KenneyBasePath}/Bonus/sfx_zap.ogg");
            AudioClip comboClip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{KenneyBasePath}/Bonus/sfx_twoTone.ogg");

            // 2. Font & UI Sprites
            Font gameFont = AssetDatabase.LoadAssetAtPath<Font>($"{KenneyBasePath}/Bonus/kenvector_future.ttf");
            Sprite greenBtnSprite = LoadSprite($"{KenneyBasePath}/PNG/UI/buttonGreen.png");
            Sprite blueBtnSprite = LoadSprite($"{KenneyBasePath}/PNG/UI/buttonBlue.png");
            Sprite redBtnSprite = LoadSprite($"{KenneyBasePath}/PNG/UI/buttonRed.png");
            Sprite yellowBtnSprite = LoadSprite($"{KenneyBasePath}/PNG/UI/buttonYellow.png");
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

            // 9. Create Shockwave Prefab
            Sprite shockwaveSprite = LoadSprite($"{KenneyBasePath}/PNG/Effects/shield3.png");
            if (shockwaveSprite == null) shockwaveSprite = LoadSprite($"{KenneyBasePath}/PNG/Effects/shield1.png");
            GameObject shockwavePrefab = CreateShockwavePrefab(shockwaveSprite);

            // 10. Create Enemy Prefabs
            List<GameObject> enemyPrefabs = new List<GameObject>();
            GameObject enemyRed = CreateEnemyPrefab("Enemy_Red", $"{KenneyBasePath}/PNG/Enemies/enemyRed1.png", 3.2f, 0f, 15, explosionPrefab, true, enemyLaserPrefab, floatingScorePrefab, powerUpPrefabs);
            if (enemyRed != null) enemyPrefabs.Add(enemyRed);

            GameObject enemyGreen = CreateEnemyPrefab("Enemy_Green", $"{KenneyBasePath}/PNG/Enemies/enemyGreen1.png", 3.6f, 0f, 20, explosionPrefab, true, enemyLaserPrefab, floatingScorePrefab, powerUpPrefabs);
            if (enemyGreen != null) enemyPrefabs.Add(enemyGreen);

            GameObject enemyBlue = CreateEnemyPrefab("Enemy_Blue", $"{KenneyBasePath}/PNG/Enemies/enemyBlue1.png", 4.0f, 0f, 25, explosionPrefab, true, enemyLaserPrefab, floatingScorePrefab, powerUpPrefabs);
            if (enemyBlue != null) enemyPrefabs.Add(enemyBlue);

            GameObject meteorBig = CreateEnemyPrefab("Meteor_Big", $"{KenneyBasePath}/PNG/Meteors/meteorBrown_big1.png", 2.5f, 45f, 10, explosionPrefab, false, null, floatingScorePrefab, powerUpPrefabs);
            if (meteorBig != null) enemyPrefabs.Add(meteorBig);

            GameObject meteorMed = CreateEnemyPrefab("Meteor_Med", $"{KenneyBasePath}/PNG/Meteors/meteorBrown_med1.png", 3.2f, -60f, 15, explosionPrefab, false, null, floatingScorePrefab, powerUpPrefabs);
            if (meteorMed != null) enemyPrefabs.Add(meteorMed);

            GameObject meteorGrey = CreateEnemyPrefab("Meteor_Grey", $"{KenneyBasePath}/PNG/Meteors/meteorGrey_big1.png", 2.8f, 30f, 10, explosionPrefab, false, null, floatingScorePrefab, powerUpPrefabs);
            if (meteorGrey != null) enemyPrefabs.Add(meteorGrey);

            // 11. Load Ship Sprites for Hangar & Player
            Sprite shipBlue = LoadSprite($"{KenneyBasePath}/PNG/playerShip1_blue.png");
            Sprite shipOrange = LoadSprite($"{KenneyBasePath}/PNG/playerShip1_orange.png");
            Sprite shipGreen = LoadSprite($"{KenneyBasePath}/PNG/playerShip1_green.png");
            Sprite shipRed = LoadSprite($"{KenneyBasePath}/PNG/playerShip1_red.png");
            Sprite[] shipSprites = new Sprite[] { shipBlue, shipOrange, shipGreen, shipRed };

            // 12. Create Player Prefab
            GameObject playerPrefab = CreatePlayerPrefab(shipBlue, laserPrefab, explosionPrefab, floatingScorePrefab, shockwavePrefab, shipSprites);

            // 13. Setup Scene GameObjects
            SetupScene(
                playerPrefab,
                laserPrefab,
                enemyPrefabs.ToArray(),
                bossPrefab,
                shockwavePrefab,
                shipSprites,
                shootClip,
                enemyShootClip,
                explosionClip,
                shieldDownClip,
                powerUpClip,
                gameOverClip,
                buttonClickClip,
                empBombClip,
                comboClip,
                gameFont,
                greenBtnSprite,
                blueBtnSprite,
                redBtnSprite,
                yellowBtnSprite,
                heartSprite);

            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            Debug.Log("[SpaceDefenderSetup] Elite Arcade Master Scene Setup Completed Successfully!");
        }

        private static Sprite LoadSprite(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static GameObject CreateFloatingScorePrefab(Font font)
        {
            string path = $"{PrefabFolderPath}/FloatingScore.prefab";
            GameObject go = new GameObject("FloatingScore");

            TextMesh tm = go.AddComponent<TextMesh>();
            tm.text = "+10";
            if (font != null) tm.font = font;
            tm.fontSize = 28;
            tm.characterSize = 0.12f;
            tm.alignment = TextAlignment.Center;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.color = new Color(0.3f, 1f, 0.4f, 1f);

            MeshRenderer mr = go.GetComponent<MeshRenderer>();
            if (mr != null && font != null && font.material != null)
            {
                mr.sharedMaterial = font.material;
                mr.sortingOrder = 25;
            }

            FloatingScore fs = go.AddComponent<FloatingScore>();
            fs.floatSpeed = 2.4f;
            fs.fadeDuration = 0.75f;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreateShockwavePrefab(Sprite shockwaveSprite)
        {
            string path = $"{PrefabFolderPath}/Shockwave.prefab";
            GameObject go = new GameObject("Shockwave");

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = shockwaveSprite;
            sr.sortingOrder = 18;
            sr.color = new Color(0.3f, 0.9f, 1f, 0.9f);

            ShockwaveEffect se = go.AddComponent<ShockwaveEffect>();
            se.spriteRenderer = sr;
            se.maxRadius = 14f;
            se.duration = 0.55f;
            se.bossDamage = 8;

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
            col.radius = 0.35f;

            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            PowerUp pu = go.AddComponent<PowerUp>();
            pu.powerUpType = type;
            pu.fallSpeed = 2.2f;
            pu.wobbleAmount = 0.4f;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreateExplosionPrefab()
        {
            string path = $"{PrefabFolderPath}/ExplosionVFX.prefab";
            GameObject go = new GameObject("ExplosionVFX");

            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.6f;
            main.loop = false;
            main.startLifetime = 0.45f;
            main.startSpeed = 4.5f;
            main.startSize = 0.35f;
            main.startColor = new Color(1f, 0.6f, 0.1f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 25) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.25f;

            Material mat = GetOrCreateParticleMaterial("ExplosionParticleMat", $"{KenneyBasePath}/PNG/Effects/star1.png");
            ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = mat;
                renderer.sortingOrder = 15;
            }

            go.AddComponent<AutoDestroy>().lifetime = 0.7f;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static Material GetOrCreateParticleMaterial(string matName, string texturePath)
        {
            string matPath = $"Assets/{matName}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
                if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
                if (shader == null) shader = Shader.Find("Mobile/Particles/Additive");
                if (shader == null) shader = Shader.Find("Sprites/Default");

                mat = new Material(shader);
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                if (tex != null) mat.mainTexture = tex;

                AssetDatabase.CreateAsset(mat, matPath);
            }
            return mat;
        }

        private static GameObject CreateLaserPrefab(Sprite sprite)
        {
            string path = $"{PrefabFolderPath}/Laser.prefab";
            GameObject go = new GameObject("Laser");

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 10;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            if (sprite != null) col.size = sprite.bounds.size;

            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            Laser laser = go.AddComponent<Laser>();
            laser.speed = 14f;
            laser.isEnemyLaser = false;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreateEnemyLaserPrefab(Sprite sprite)
        {
            string path = $"{PrefabFolderPath}/EnemyLaser.prefab";
            GameObject go = new GameObject("EnemyLaser");

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 9;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            if (sprite != null) col.size = sprite.bounds.size;

            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            Laser laser = go.AddComponent<Laser>();
            laser.speed = 6.5f;
            laser.isEnemyLaser = true;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreateBossPrefab(
            Sprite sprite,
            GameObject enemyLaserPrefab,
            GameObject explosionVFX,
            GameObject floatingScorePrefab,
            GameObject[] dropPowerUps)
        {
            string path = $"{PrefabFolderPath}/Boss_UFO.prefab";
            GameObject go = new GameObject("Boss_UFO");

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 7;

            CircleCollider2D col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            if (sprite != null) col.radius = sprite.bounds.size.x * 0.45f;

            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            GameObject leftFP = new GameObject("LeftFirePoint");
            leftFP.transform.SetParent(go.transform);
            leftFP.transform.localPosition = new Vector3(-0.6f, -0.4f, 0f);

            GameObject rightFP = new GameObject("RightFirePoint");
            rightFP.transform.SetParent(go.transform);
            rightFP.transform.localPosition = new Vector3(0.6f, -0.4f, 0f);

            BossController boss = go.AddComponent<BossController>();
            boss.maxHp = 20;
            boss.scoreValue = 100;
            boss.bossName = "RED UFO MOTHERSHIP";
            boss.entrySpeed = 2.0f;
            boss.targetY = 2.8f;
            boss.patrolSpeed = 1.8f;
            boss.patrolAmplitude = 2.5f;
            boss.enemyLaserPrefab = enemyLaserPrefab;
            boss.attackInterval = 1.4f;
            boss.leftFirePoint = leftFP.transform;
            boss.rightFirePoint = rightFP.transform;
            boss.explosionPrefab = explosionVFX;
            boss.floatingScorePrefab = floatingScorePrefab;
            boss.dropPowerUpPrefabs = dropPowerUps;

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
            sr.sortingOrder = 6;

            PolygonCollider2D col = go.AddComponent<PolygonCollider2D>();
            col.isTrigger = true;

            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            Enemy enemy = go.AddComponent<Enemy>();
            enemy.speed = speed;
            enemy.rotationSpeed = rotSpeed;
            enemy.scoreValue = score;
            enemy.explosionPrefab = explosionVFX;
            enemy.floatingScorePrefab = floatingScorePrefab;
            enemy.powerUpPrefabs = powerUpPrefabs;
            enemy.dropChance = 0.25f;
            enemy.canShoot = canShoot;
            enemy.enemyLaserPrefab = enemyLaserPrefab;
            enemy.minShootDelay = 1.4f;
            enemy.maxShootDelay = 2.8f;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreatePlayerPrefab(
            Sprite sprite,
            GameObject laserPrefab,
            GameObject explosionVFX,
            GameObject floatingScorePrefab,
            GameObject shockwavePrefab,
            Sprite[] shipSprites)
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
            pc.moveSpeed = 9.5f;
            pc.padding = 0.6f;
            pc.fireRate = 0.22f;
            pc.autoFireForDemo = false;
            pc.laserPrefab = laserPrefab;
            pc.firePoint = firePoint.transform;
            pc.explosionPrefab = explosionVFX;
            pc.shieldVisual = shieldObj;
            pc.floatingScorePrefab = floatingScorePrefab;
            pc.shockwavePrefab = shockwavePrefab;
            pc.shipSprites = shipSprites;
            pc.maxBombs = 3;
            pc.currentBombs = 2;
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
            GameObject shockwavePrefab,
            Sprite[] shipSprites,
            AudioClip shootClip,
            AudioClip enemyShootClip,
            AudioClip explosionClip,
            AudioClip shieldDownClip,
            AudioClip powerUpClip,
            AudioClip gameOverClip,
            AudioClip buttonClickClip,
            AudioClip empBombClip,
            AudioClip comboClip,
            Font gameFont,
            Sprite greenBtnSprite,
            Sprite blueBtnSprite,
            Sprite redBtnSprite,
            Sprite yellowBtnSprite,
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
            string[] cleanNames = { "Player", "BackgroundScroller", "EnemySpawner", "GameManager", "AudioManager", "UIManager", "Canvas", "EventSystem", "ComboManager", "AchievementManager" };
            var roots = EditorSceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var root in roots)
            {
                if (cleanNames.Contains(root.name) || root.name.Contains("(Clone)"))
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
            playerObj.SetActive(true);

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
            audioMgr.empBombClip = empBombClip;
            audioMgr.comboClip = comboClip;

            // 5. Combo Manager
            GameObject comboObj = new GameObject("ComboManager");
            comboObj.AddComponent<ComboManager>();

            // 6. Achievement Manager
            GameObject achObj = new GameObject("AchievementManager");
            achObj.AddComponent<AchievementManager>();

            // 7. Canvas & UI
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
            canvas.planeDistance = 5f;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();

            // Event System
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();

            // UI Manager
            GameObject uiMgrObj = new GameObject("UIManager");
            UIManager uiManager = uiMgrObj.AddComponent<UIManager>();
            uiManager.hangarShipSprites = shipSprites;

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
            scoreText.fontSize = 32;
            scoreText.lineSpacing = 1.15f;
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

            // Bomb HUD Button (Top-Right below Hearts)
            GameObject bombBtnObj = CreateButton(inGameHUD.transform, "BombBtn", "BOMB [B]\nx2", gameFont, redBtnSprite, new Vector2(-40, -145), new Vector2(230, 80), 20);
            RectTransform bBtnRect = bombBtnObj.GetComponent<RectTransform>();
            bBtnRect.anchorMin = new Vector2(1, 1);
            bBtnRect.anchorMax = new Vector2(1, 1);
            bBtnRect.pivot = new Vector2(1, 1);
            Button bombBtn = bombBtnObj.GetComponent<Button>();
            Text bombText = bombBtnObj.GetComponentInChildren<Text>();

            // Combo HUD (Top Center below Boss Bar area)
            GameObject comboPanel = new GameObject("ComboPanel");
            comboPanel.transform.SetParent(inGameHUD.transform, false);
            RectTransform cpRect = comboPanel.AddComponent<RectTransform>();
            cpRect.anchorMin = new Vector2(0.5f, 1f);
            cpRect.anchorMax = new Vector2(0.5f, 1f);
            cpRect.pivot = new Vector2(0.5f, 1f);
            cpRect.anchoredPosition = new Vector2(0, -140);
            cpRect.sizeDelta = new Vector2(340, 70);

            Image cpBg = comboPanel.AddComponent<Image>();
            cpBg.color = new Color(0.06f, 0.08f, 0.16f, 0.85f);

            GameObject comboTextObj = new GameObject("ComboText");
            comboTextObj.transform.SetParent(comboPanel.transform, false);
            Text cText = comboTextObj.AddComponent<Text>();
            cText.text = "COMBO x2!\n(2 HITS)";
            if (gameFont != null) cText.font = gameFont;
            cText.fontSize = 20;
            cText.alignment = TextAnchor.MiddleCenter;
            cText.color = new Color(1f, 0.85f, 0.2f);
            comboTextObj.AddComponent<Outline>().effectColor = new Color(0, 0, 0, 0.9f);
            RectTransform ctRect = cText.rectTransform;
            ctRect.anchoredPosition = new Vector2(0, 10);
            ctRect.sizeDelta = new Vector2(320, 45);

            // Combo Slider bar
            GameObject comboSliderObj = new GameObject("ComboSlider");
            comboSliderObj.transform.SetParent(comboPanel.transform, false);
            Slider cSlider = comboSliderObj.AddComponent<Slider>();
            RectTransform csRect = comboSliderObj.GetComponent<RectTransform>();
            csRect.anchoredPosition = new Vector2(0, -22);
            csRect.sizeDelta = new Vector2(300, 12);

            GameObject csFillArea = new GameObject("Fill Area");
            csFillArea.transform.SetParent(comboSliderObj.transform, false);
            RectTransform csfaRect = csFillArea.AddComponent<RectTransform>();
            csfaRect.anchorMin = Vector2.zero;
            csfaRect.anchorMax = Vector2.one;
            csfaRect.offsetMin = Vector2.zero;
            csfaRect.offsetMax = Vector2.zero;

            GameObject csFill = new GameObject("Fill");
            csFill.transform.SetParent(csFillArea.transform, false);
            Image csFillImg = csFill.AddComponent<Image>();
            csFillImg.color = new Color(1f, 0.75f, 0.1f);
            RectTransform csfRect = csFill.GetComponent<RectTransform>();
            csfRect.anchorMin = Vector2.zero;
            csfRect.anchorMax = Vector2.one;
            csfRect.offsetMin = Vector2.zero;
            csfRect.offsetMax = Vector2.zero;

            cSlider.fillRect = csfRect;
            cSlider.minValue = 0f;
            cSlider.maxValue = 1f;
            cSlider.value = 1f;

            comboPanel.SetActive(false);

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

            // Boss Slider
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
            titleRect.anchoredPosition = new Vector2(0, 480);
            titleRect.sizeDelta = new Vector2(900, 220);

            // Subtitle
            GameObject subObj = new GameObject("SubtitleText");
            subObj.transform.SetParent(mainMenuPanel.transform, false);
            Text subText = subObj.AddComponent<Text>();
            subText.text = "ARCADE MASTER EDITION";
            if (gameFont != null) subText.font = gameFont;
            subText.fontSize = 26;
            subText.alignment = TextAnchor.MiddleCenter;
            subText.color = new Color(0.3f, 0.85f, 1f, 1f);
            subObj.AddComponent<Outline>().effectColor = new Color(0, 0, 0, 0.8f);
            RectTransform subRect = subText.rectTransform;
            subRect.anchoredPosition = new Vector2(0, 360);
            subRect.sizeDelta = new Vector2(600, 50);

            // Main Menu Buttons
            GameObject playBtnObj = CreateButton(mainMenuPanel.transform, "PlayButton", "PLAY", gameFont, greenBtnSprite, new Vector2(0, 190), new Vector2(420, 95), 38);
            Button playBtn = playBtnObj.GetComponent<Button>();

            GameObject hangarBtnObj = CreateButton(mainMenuPanel.transform, "HangarButton", "HANGAR", gameFont, blueBtnSprite, new Vector2(0, 85), new Vector2(420, 85), 32);
            Button hangarBtn = hangarBtnObj.GetComponent<Button>();

            GameObject achBtnObj = CreateButton(mainMenuPanel.transform, "AchievementsButton", "ACHIEVEMENTS", gameFont, yellowBtnSprite, new Vector2(0, -15), new Vector2(420, 85), 28);
            Button achBtn = achBtnObj.GetComponent<Button>();

            GameObject howToPlayBtnObj = CreateButton(mainMenuPanel.transform, "HowToPlayButton", "HOW TO PLAY", gameFont, blueBtnSprite, new Vector2(0, -115), new Vector2(420, 85), 28);
            Button howToPlayBtn = howToPlayBtnObj.GetComponent<Button>();

            GameObject settingsBtnObj = CreateButton(mainMenuPanel.transform, "SettingsButton", "SETTINGS", gameFont, blueBtnSprite, new Vector2(0, -215), new Vector2(420, 85), 30);
            Button settingsBtn = settingsBtnObj.GetComponent<Button>();

            GameObject exitBtnObj = CreateButton(mainMenuPanel.transform, "ExitButton", "EXIT", gameFont, redBtnSprite, new Vector2(0, -320), new Vector2(420, 85), 32);
            Button exitBtn = exitBtnObj.GetComponent<Button>();

            // How To Play Modal
            GameObject modalObj = new GameObject("HowToPlayModal");
            modalObj.transform.SetParent(mainMenuPanel.transform, false);
            Image modalBg = modalObj.AddComponent<Image>();
            modalBg.color = new Color(0.04f, 0.08f, 0.18f, 0.96f);
            RectTransform modalRect = modalObj.GetComponent<RectTransform>();
            modalRect.anchoredPosition = Vector2.zero;
            modalRect.sizeDelta = new Vector2(860, 920);

            GameObject modalTitle = new GameObject("ModalTitle");
            modalTitle.transform.SetParent(modalObj.transform, false);
            Text mt = modalTitle.AddComponent<Text>();
            mt.text = "HOW TO PLAY";
            if (gameFont != null) mt.font = gameFont;
            mt.fontSize = 44;
            mt.alignment = TextAnchor.MiddleCenter;
            mt.color = new Color(0.3f, 0.85f, 1f);
            modalTitle.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 380);
            modalTitle.GetComponent<RectTransform>().sizeDelta = new Vector2(700, 80);

            GameObject modalContent = new GameObject("ModalContent");
            modalContent.transform.SetParent(modalObj.transform, false);
            Text mc = modalContent.AddComponent<Text>();
            mc.text = "CONTROLS:\n- A / D or Left / Right Arrows: Move Ship\n- Space / Left Click: Fire Lasers\n- B / Right Click: Detonate EMP Nuke Bomb\n- Esc / P: Pause Match\n\nPOWER-UPS:\n- Bolt: 3-Way Triple Shot (10s)\n- Shield: Absorbs 1 hit without losing lives\n- Star: Restores +1 Life Heart\n\nCOMBOS & SCORING:\n- Defeat enemies within 2.2s to build Combo x2 to x5\n\nBOSS BATTLE:\n- Cross 80 points to summon the Giant Red UFO Mothership!";
            if (gameFont != null) mc.font = gameFont;
            mc.fontSize = 20;
            mc.lineSpacing = 1.3f;
            mc.alignment = TextAnchor.MiddleLeft;
            mc.color = Color.white;
            modalContent.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 10);
            modalContent.GetComponent<RectTransform>().sizeDelta = new Vector2(760, 600);

            GameObject closeModalBtnObj = CreateButton(modalObj.transform, "CloseModalBtn", "GOT IT!", gameFont, greenBtnSprite, new Vector2(0, -380), new Vector2(300, 80), 30);
            Button closeModalBtn = closeModalBtnObj.GetComponent<Button>();
            modalObj.SetActive(false);

            // ================== C. SETTINGS MODAL ==================
            GameObject settingsModal = new GameObject("SettingsModal");
            settingsModal.transform.SetParent(canvasObj.transform, false);
            Image setOverlay = settingsModal.AddComponent<Image>();
            setOverlay.color = new Color(0.04f, 0.08f, 0.2f, 0.96f);
            settingsModal.AddComponent<Outline>().effectColor = new Color(0.2f, 0.8f, 1f, 0.8f);
            RectTransform smRect = settingsModal.GetComponent<RectTransform>();
            smRect.anchorMin = new Vector2(0.5f, 0.5f);
            smRect.anchorMax = new Vector2(0.5f, 0.5f);
            smRect.anchoredPosition = Vector2.zero;
            smRect.sizeDelta = new Vector2(880, 1000);

            GameObject setTitleObj = new GameObject("SettingsTitle");
            setTitleObj.transform.SetParent(settingsModal.transform, false);
            Text stt = setTitleObj.AddComponent<Text>();
            stt.text = "AUDIO SETTINGS";
            if (gameFont != null) stt.font = gameFont;
            stt.fontSize = 44;
            stt.alignment = TextAnchor.MiddleCenter;
            stt.color = new Color(0.3f, 0.85f, 1f);
            setTitleObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 300);
            setTitleObj.GetComponent<RectTransform>().sizeDelta = new Vector2(700, 80);

            // BGM Volume Slider
            GameObject bgmLabelObj = new GameObject("BGMLabel");
            bgmLabelObj.transform.SetParent(settingsModal.transform, false);
            Text bgmLbl = bgmLabelObj.AddComponent<Text>();
            bgmLbl.text = "MUSIC (BGM)";
            if (gameFont != null) bgmLbl.font = gameFont;
            bgmLbl.fontSize = 24;
            bgmLbl.alignment = TextAnchor.MiddleLeft;
            bgmLbl.color = Color.white;
            bgmLabelObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 170);
            bgmLabelObj.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 40);

            Slider bgmSlider = CreateGenericSlider(settingsModal.transform, "BGMSlider", new Vector2(0, 120), new Vector2(500, 30));

            // SFX Volume Slider
            GameObject sfxLabelObj = new GameObject("SFXLabel");
            sfxLabelObj.transform.SetParent(settingsModal.transform, false);
            Text sfxLbl = sfxLabelObj.AddComponent<Text>();
            sfxLbl.text = "EFFECTS (SFX)";
            if (gameFont != null) sfxLbl.font = gameFont;
            sfxLbl.fontSize = 24;
            sfxLbl.alignment = TextAnchor.MiddleLeft;
            sfxLbl.color = Color.white;
            sfxLabelObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 30);
            sfxLabelObj.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 40);

            Slider sfxSlider = CreateGenericSlider(settingsModal.transform, "SFXSlider", new Vector2(0, -20), new Vector2(500, 30));

            // Mute Toggle
            GameObject muteObj = new GameObject("MuteToggle");
            muteObj.transform.SetParent(settingsModal.transform, false);
            Toggle muteToggle = muteObj.AddComponent<Toggle>();
            RectTransform mtRect = muteObj.GetComponent<RectTransform>();
            mtRect.anchoredPosition = new Vector2(-150, -110);
            mtRect.sizeDelta = new Vector2(40, 40);

            GameObject muteBg = new GameObject("Background");
            muteBg.transform.SetParent(muteObj.transform, false);
            Image mBgImg = muteBg.AddComponent<Image>();
            mBgImg.color = new Color(0.2f, 0.2f, 0.3f);
            muteBg.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 40);

            GameObject muteCheck = new GameObject("Checkmark");
            muteCheck.transform.SetParent(muteBg.transform, false);
            Image mChkImg = muteCheck.AddComponent<Image>();
            mChkImg.color = new Color(0.2f, 0.9f, 0.3f);
            muteCheck.GetComponent<RectTransform>().sizeDelta = new Vector2(26, 26);

            muteToggle.graphic = mChkImg;
            muteToggle.targetGraphic = mBgImg;

            GameObject muteLblObj = new GameObject("Label");
            muteLblObj.transform.SetParent(settingsModal.transform, false);
            Text mLbl = muteLblObj.AddComponent<Text>();
            mLbl.text = "MUTE ALL AUDIO";
            if (gameFont != null) mLbl.font = gameFont;
            mLbl.fontSize = 22;
            mLbl.alignment = TextAnchor.MiddleLeft;
            mLbl.color = Color.white;
            muteLblObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(60, -110);
            muteLblObj.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 40);

            GameObject closeSetBtnObj = CreateButton(settingsModal.transform, "CloseSettingsBtn", "CLOSE", gameFont, blueBtnSprite, new Vector2(0, -240), new Vector2(300, 80), 30);
            Button closeSetBtn = closeSetBtnObj.GetComponent<Button>();
            settingsModal.SetActive(false);

            // ================== D. HANGAR MODAL ==================
            GameObject hangarModal = new GameObject("HangarModal");
            hangarModal.transform.SetParent(canvasObj.transform, false);
            Image hangarOverlay = hangarModal.AddComponent<Image>();
            hangarOverlay.color = new Color(0.04f, 0.08f, 0.2f, 0.96f);
            hangarModal.AddComponent<Outline>().effectColor = new Color(0.2f, 0.8f, 1f, 0.8f);
            RectTransform hmRect = hangarModal.GetComponent<RectTransform>();
            hmRect.anchorMin = new Vector2(0.5f, 0.5f);
            hmRect.anchorMax = new Vector2(0.5f, 0.5f);
            hmRect.anchoredPosition = Vector2.zero;
            hmRect.sizeDelta = new Vector2(880, 1150);

            GameObject hangarTitleObj = new GameObject("HangarTitle");
            hangarTitleObj.transform.SetParent(hangarModal.transform, false);
            Text htt = hangarTitleObj.AddComponent<Text>();
            htt.text = "SHIP HANGAR";
            if (gameFont != null) htt.font = gameFont;
            htt.fontSize = 40;
            htt.alignment = TextAnchor.MiddleCenter;
            htt.color = new Color(0.3f, 0.85f, 1f);
            hangarTitleObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 360);
            hangarTitleObj.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 80);

            // Preview Ship Image
            GameObject shipImgObj = new GameObject("ShipPreview");
            shipImgObj.transform.SetParent(hangarModal.transform, false);
            Image shipPreview = shipImgObj.AddComponent<Image>();
            shipPreview.preserveAspect = true;
            if (shipSprites != null && shipSprites.Length > 0 && shipSprites[0] != null)
            {
                shipPreview.sprite = shipSprites[0];
            }
            RectTransform spRect = shipImgObj.GetComponent<RectTransform>();
            spRect.anchoredPosition = new Vector2(0, 190);
            spRect.sizeDelta = new Vector2(160, 160);

            // Prev & Next Buttons
            GameObject prevBtnObj = CreateButton(hangarModal.transform, "PrevShipBtn", "<", gameFont, blueBtnSprite, new Vector2(-220, 190), new Vector2(80, 80), 36);
            Button prevBtn = prevBtnObj.GetComponent<Button>();

            GameObject nextBtnObj = CreateButton(hangarModal.transform, "NextShipBtn", ">", gameFont, blueBtnSprite, new Vector2(220, 190), new Vector2(80, 80), 36);
            Button nextBtn = nextBtnObj.GetComponent<Button>();

            // Ship Name
            GameObject shipNameObj = new GameObject("ShipNameText");
            shipNameObj.transform.SetParent(hangarModal.transform, false);
            Text shipName = shipNameObj.AddComponent<Text>();
            shipName.text = "BLUE VANGUARD";
            if (gameFont != null) shipName.font = gameFont;
            shipName.fontSize = 32;
            shipName.alignment = TextAnchor.MiddleCenter;
            shipName.color = new Color(1f, 0.9f, 0.2f);
            shipNameObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 60);
            shipNameObj.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 50);

            // Ship Stats Text
            GameObject shipStatsObj = new GameObject("ShipStatsText");
            shipStatsObj.transform.SetParent(hangarModal.transform, false);
            Text shipStats = shipStatsObj.AddComponent<Text>();
            shipStats.text = "Speed: 9.5\nFire Rate: 0.22s\nBombs: 2\nPerk: Balanced Fleet Fighter";
            if (gameFont != null) shipStats.font = gameFont;
            shipStats.fontSize = 22;
            shipStats.lineSpacing = 1.35f;
            shipStats.alignment = TextAnchor.MiddleCenter;
            shipStats.color = Color.white;
            shipStatsObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -70);
            shipStatsObj.GetComponent<RectTransform>().sizeDelta = new Vector2(650, 180);

            // Select Ship Button
            GameObject selectShipBtnObj = CreateButton(hangarModal.transform, "SelectShipBtn", "SELECT SHIP", gameFont, greenBtnSprite, new Vector2(0, -220), new Vector2(400, 85), 30);
            Button selectShipBtn = selectShipBtnObj.GetComponent<Button>();
            Text selectShipBtnText = selectShipBtnObj.GetComponentInChildren<Text>();

            // Close Hangar Button
            GameObject closeHangarBtnObj = CreateButton(hangarModal.transform, "CloseHangarBtn", "BACK", gameFont, redBtnSprite, new Vector2(0, -330), new Vector2(400, 80), 28);
            Button closeHangarBtn = closeHangarBtnObj.GetComponent<Button>();
            hangarModal.SetActive(false);

            // ================== E. ACHIEVEMENTS MODAL ==================
            GameObject achModal = new GameObject("AchievementsModal");
            achModal.transform.SetParent(canvasObj.transform, false);
            Image achOverlay = achModal.AddComponent<Image>();
            achOverlay.color = new Color(0.04f, 0.08f, 0.2f, 0.96f);
            achModal.AddComponent<Outline>().effectColor = new Color(1f, 0.85f, 0.2f, 0.8f);
            RectTransform amRect = achModal.GetComponent<RectTransform>();
            amRect.anchorMin = new Vector2(0.5f, 0.5f);
            amRect.anchorMax = new Vector2(0.5f, 0.5f);
            amRect.anchoredPosition = Vector2.zero;
            amRect.sizeDelta = new Vector2(880, 1180);

            GameObject achTitleObj = new GameObject("AchTitle");
            achTitleObj.transform.SetParent(achModal.transform, false);
            Text act = achTitleObj.AddComponent<Text>();
            act.text = "ACHIEVEMENTS";
            if (gameFont != null) act.font = gameFont;
            act.fontSize = 40;
            act.alignment = TextAnchor.MiddleCenter;
            act.color = new Color(1f, 0.85f, 0.2f);
            achTitleObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 370);
            achTitleObj.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 80);

            GameObject achListObj = new GameObject("AchievementsListText");
            achListObj.transform.SetParent(achModal.transform, false);
            Text achListText = achListObj.AddComponent<Text>();
            achListText.text = "Loading achievements...";
            if (gameFont != null) achListText.font = gameFont;
            achListText.fontSize = 20;
            achListText.lineSpacing = 1.25f;
            achListText.alignment = TextAnchor.MiddleLeft;
            achListText.color = Color.white;
            achListObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 10);
            achListObj.GetComponent<RectTransform>().sizeDelta = new Vector2(760, 600);

            GameObject closeAchBtnObj = CreateButton(achModal.transform, "CloseAchBtn", "CLOSE", gameFont, blueBtnSprite, new Vector2(0, -360), new Vector2(300, 80), 30);
            Button closeAchBtn = closeAchBtnObj.GetComponent<Button>();
            achModal.SetActive(false);

            // ================== F. ACHIEVEMENT TOAST ==================
            GameObject toastObj = new GameObject("AchievementToast");
            toastObj.transform.SetParent(canvasObj.transform, false);
            Image toastBg = toastObj.AddComponent<Image>();
            toastBg.color = new Color(0.08f, 0.12f, 0.28f, 0.95f);
            RectTransform toastRect = toastObj.GetComponent<RectTransform>();
            toastRect.anchorMin = new Vector2(0.5f, 1f);
            toastRect.anchorMax = new Vector2(0.5f, 1f);
            toastRect.pivot = new Vector2(0.5f, 1f);
            toastRect.anchoredPosition = new Vector2(0, -50);
            toastRect.sizeDelta = new Vector2(650, 110);
            toastObj.AddComponent<Outline>().effectColor = new Color(1f, 0.85f, 0.2f, 0.9f);

            GameObject tTitleObj = new GameObject("ToastTitle");
            tTitleObj.transform.SetParent(toastObj.transform, false);
            Text tTitle = tTitleObj.AddComponent<Text>();
            tTitle.text = "ACHIEVEMENT UNLOCKED!";
            if (gameFont != null) tTitle.font = gameFont;
            tTitle.fontSize = 22;
            tTitle.alignment = TextAnchor.MiddleCenter;
            tTitle.color = new Color(1f, 0.9f, 0.2f);
            tTitleObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 20);
            tTitleObj.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 35);

            GameObject tDescObj = new GameObject("ToastDesc");
            tDescObj.transform.SetParent(toastObj.transform, false);
            Text tDesc = tDescObj.AddComponent<Text>();
            tDesc.text = "Achievement Description";
            if (gameFont != null) tDesc.font = gameFont;
            tDesc.fontSize = 18;
            tDesc.alignment = TextAnchor.MiddleCenter;
            tDesc.color = Color.white;
            tDescObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -20);
            tDescObj.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 35);

            toastObj.SetActive(false);

            // ================== G. PAUSE PANEL ==================
            GameObject pausePanel = new GameObject("PausePanel");
            pausePanel.transform.SetParent(canvasObj.transform, false);
            Image pauseBg = pausePanel.AddComponent<Image>();
            pauseBg.color = new Color(0.03f, 0.05f, 0.12f, 0.92f);
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

            GameObject resumeBtnObj = CreateButton(pausePanel.transform, "ResumeBtn", "RESUME", gameFont, greenBtnSprite, new Vector2(0, 110), new Vector2(380, 85), 34);
            Button resumeBtn = resumeBtnObj.GetComponent<Button>();

            GameObject restartBtnObj = CreateButton(pausePanel.transform, "RestartBtn", "RESTART", gameFont, blueBtnSprite, new Vector2(0, 10), new Vector2(380, 85), 32);
            Button restartBtn = restartBtnObj.GetComponent<Button>();

            GameObject pauseSetBtnObj = CreateButton(pausePanel.transform, "PauseSettingsBtn", "SETTINGS", gameFont, blueBtnSprite, new Vector2(0, -90), new Vector2(380, 85), 28);
            Button pauseSetBtn = pauseSetBtnObj.GetComponent<Button>();

            GameObject pauseMenuBtnObj = CreateButton(pausePanel.transform, "PauseMenuBtn", "MAIN MENU", gameFont, redBtnSprite, new Vector2(0, -190), new Vector2(380, 85), 30);
            Button pauseMenuBtn = pauseMenuBtnObj.GetComponent<Button>();
            pausePanel.SetActive(false);

            // ================== H. GAME OVER PANEL ==================
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
            nrt.text = "* NEW HIGH SCORE! *";
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

            // ================== WIRE UIMANAGER ==================
            uiManager.mainMenuPanel = mainMenuPanel;
            uiManager.playButton = playBtn;
            uiManager.howToPlayButton = howToPlayBtn;
            uiManager.howToPlayModal = modalObj;
            uiManager.closeHowToPlayButton = closeModalBtn;
            uiManager.openHangarButton = hangarBtn;
            uiManager.openAchievementsButton = achBtn;
            uiManager.openSettingsButton = settingsBtn;
            uiManager.exitButton = exitBtn;

            uiManager.inGameHUD = inGameHUD;
            uiManager.scoreText = scoreText;
            uiManager.highScoreText = hsText;
            uiManager.heartImages = heartList.ToArray();
            uiManager.pauseButton = pauseBtn;
            uiManager.bombText = bombText;
            uiManager.bombButton = bombBtn;

            uiManager.comboPanel = comboPanel;
            uiManager.comboText = cText;
            uiManager.comboSlider = cSlider;

            uiManager.bossBarPanel = bossBarObj;
            uiManager.bossHpSlider = hpSlider;
            uiManager.bossNameText = bTitleText;
            uiManager.bossHpText = hpText;

            uiManager.settingsModal = settingsModal;
            uiManager.bgmSlider = bgmSlider;
            uiManager.sfxSlider = sfxSlider;
            uiManager.muteToggle = muteToggle;
            uiManager.closeSettingsButton = closeSetBtn;
            uiManager.pauseSettingsButton = pauseSetBtn;

            uiManager.hangarModal = hangarModal;
            uiManager.shipPreviewImage = shipPreview;
            uiManager.shipNameText = shipName;
            uiManager.shipStatsText = shipStats;
            uiManager.prevShipButton = prevBtn;
            uiManager.nextShipButton = nextBtn;
            uiManager.selectShipButton = selectShipBtn;
            uiManager.selectShipButtonText = selectShipBtnText;
            uiManager.closeHangarButton = closeHangarBtn;

            uiManager.achievementsModal = achModal;
            uiManager.achievementsListText = achListText;
            uiManager.closeAchievementsButton = closeAchBtn;

            uiManager.achievementToast = toastObj;
            uiManager.toastTitleText = tTitle;
            uiManager.toastDescText = tDesc;

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

            // 8. Game Manager
            GameObject gmObj = new GameObject("GameManager");
            GameManager gm = gmObj.AddComponent<GameManager>();
            gm.showMainMenuOnStart = true;

            EditorUtility.SetDirty(uiManager);
            EditorUtility.SetDirty(canvasObj);
            EditorUtility.SetDirty(gmObj);
        }

        private static Slider CreateGenericSlider(Transform parent, string name, Vector2 pos, Vector2 size)
        {
            GameObject sliderObj = new GameObject(name);
            sliderObj.transform.SetParent(parent, false);
            Slider slider = sliderObj.AddComponent<Slider>();
            RectTransform sRect = sliderObj.GetComponent<RectTransform>();
            sRect.anchoredPosition = pos;
            sRect.sizeDelta = size;

            GameObject sBg = new GameObject("Background");
            sBg.transform.SetParent(sliderObj.transform, false);
            Image sBgImg = sBg.AddComponent<Image>();
            sBgImg.color = new Color(0.12f, 0.15f, 0.25f);
            RectTransform bgRect = sBg.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObj.transform, false);
            RectTransform faRect = fillArea.AddComponent<RectTransform>();
            faRect.anchorMin = Vector2.zero;
            faRect.anchorMax = Vector2.one;
            faRect.offsetMin = Vector2.zero;
            faRect.offsetMax = Vector2.zero;

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = new Color(0.2f, 0.85f, 1f);
            RectTransform fRect = fill.GetComponent<RectTransform>();
            fRect.anchorMin = Vector2.zero;
            fRect.anchorMax = Vector2.one;
            fRect.offsetMin = Vector2.zero;
            fRect.offsetMax = Vector2.zero;

            slider.targetGraphic = sBgImg;
            slider.fillRect = fRect;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;

            return slider;
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
