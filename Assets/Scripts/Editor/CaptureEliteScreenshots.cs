using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceDefender.Editor
{
    public static class CaptureEliteScreenshots
    {
        [MenuItem("Space Defender/Capture All Elite Screenshots", false, 5)]
        public static void RunCaptureSuite()
        {
            // 1. Ensure scene is setup
            SpaceDefenderSetup.SetupGameScene();

            UIManager ui = UIManager.Instance;
            if (ui == null)
            {
                Debug.LogError("[CaptureEliteScreenshots] UIManager not found!");
                return;
            }

            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            Camera cam = Camera.main;
            if (canvas != null && cam != null)
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = cam;
                canvas.planeDistance = 5f;
            }

            // Clean up any stray clones
            foreach (var fs in Object.FindObjectsByType<FloatingScore>()) Object.DestroyImmediate(fs.gameObject);
            foreach (var en in Object.FindObjectsByType<Enemy>()) Object.DestroyImmediate(en.gameObject);
            foreach (var bo in Object.FindObjectsByType<BossController>()) Object.DestroyImmediate(bo.gameObject);
            foreach (var sw in Object.FindObjectsByType<ShockwaveEffect>()) Object.DestroyImmediate(sw.gameObject);

            GameObject playerObj = GameObject.Find("Player");

            // Screenshot 1: Main Menu with 6 Arcade Buttons
            ui.ShowMainMenu();
            if (playerObj != null)
            {
                playerObj.SetActive(true);
                playerObj.transform.position = new Vector3(0, -3.8f, 0);
            }
            Capture("elite1_main_menu.png");

            // Hide player for clean modal card presentation
            if (playerObj != null) playerObj.SetActive(false);

            // Screenshot 2: Hangar Modal
            ui.ShowHangar(true);
            Capture("elite2_hangar_modal.png");
            ui.ShowHangar(false);

            // Screenshot 3: Achievements Modal
            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.EnsureInitialized();
            }
            ui.ShowAchievements(true);
            Capture("elite3_achievements_modal.png");
            ui.ShowAchievements(false);

            // Screenshot 4: Settings Modal
            ui.ShowSettings(true);
            Capture("elite4_settings_modal.png");
            ui.ShowSettings(false);

            // Restore player for In-Game Gameplay
            if (playerObj != null)
            {
                playerObj.SetActive(true);
                playerObj.transform.position = new Vector3(0, -3.8f, 0);
            }

            // Screenshot 5: In-Game HUD (Hearts & Bomb Button)
            ui.ShowInGameHUD();
            ui.UpdateScore(240, 500);
            ui.UpdateLives(3);
            ui.UpdateBombs(2);
            Capture("elite5_ingame_hud.png");

            // Screenshot 6: Combo Multiplier 5x Action with Enemy & Floating Score
            ui.UpdateCombo(5, 5, 0.88f);
            if (ui.comboPanel != null) ui.comboPanel.SetActive(true);
            GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy_Red.prefab");
            GameObject testEnemy = null;
            if (enemyPrefab != null)
            {
                testEnemy = Object.Instantiate(enemyPrefab, new Vector3(0, 2.2f, 0), Quaternion.identity);
            }
            GameObject laserPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Laser.prefab");
            GameObject l1 = null, l2 = null;
            if (laserPrefab != null)
            {
                l1 = Object.Instantiate(laserPrefab, new Vector3(-0.25f, -1.0f, 0), Quaternion.identity);
                l2 = Object.Instantiate(laserPrefab, new Vector3(0.25f, -1.0f, 0), Quaternion.identity);
            }
            GameObject scorePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/FloatingScore.prefab");
            GameObject testScore = null;
            if (scorePrefab != null)
            {
                testScore = Object.Instantiate(scorePrefab, new Vector3(0, 0.6f, 0), Quaternion.identity);
                TextMesh tm = testScore.GetComponent<TextMesh>();
                if (tm != null) { tm.text = "+100 (x5!)"; tm.color = new Color(1f, 0.3f, 0.1f); }
            }
            Capture("elite6_combo_multiplier.png");
            if (testEnemy != null) Object.DestroyImmediate(testEnemy);
            if (l1 != null) Object.DestroyImmediate(l1);
            if (l2 != null) Object.DestroyImmediate(l2);
            if (testScore != null) Object.DestroyImmediate(testScore);

            // Screenshot 7: EMP Bomb Shockwave
            GameObject shockwavePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Shockwave.prefab");
            GameObject shockwaveObj = null;
            if (shockwavePrefab != null)
            {
                shockwaveObj = Object.Instantiate(shockwavePrefab, Vector3.zero, Quaternion.identity);
                shockwaveObj.transform.localScale = new Vector3(9f, 9f, 1f);
            }
            ui.UpdateBombs(1);
            Capture("elite7_emp_shockwave.png");
            if (shockwaveObj != null) Object.DestroyImmediate(shockwaveObj);

            // Screenshot 8: Achievement Toast Notification
            ui.ShowAchievementToast("ACHIEVEMENT UNLOCKED!", "Tactical Nuke - Detonate an EMP Shockwave Bomb");
            Capture("elite8_achievement_toast.png");
            if (ui.achievementToast != null) ui.achievementToast.SetActive(false);

            // Screenshot 9: Boss UFO Fight with Boss Bar & Triple Shot Lasers
            GameObject bossPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Boss_UFO.prefab");
            GameObject bossObj = null;
            if (bossPrefab != null)
            {
                bossObj = Object.Instantiate(bossPrefab, new Vector3(0, 2.6f, 0), Quaternion.identity);
            }
            GameObject bossL1 = null, bossL2 = null, bossL3 = null;
            if (laserPrefab != null)
            {
                bossL1 = Object.Instantiate(laserPrefab, new Vector3(0, -1.2f, 0), Quaternion.identity);
                bossL2 = Object.Instantiate(laserPrefab, new Vector3(-0.6f, -1.5f, 0), Quaternion.Euler(0, 0, 15f));
                bossL3 = Object.Instantiate(laserPrefab, new Vector3(0.6f, -1.5f, 0), Quaternion.Euler(0, 0, -15f));
            }
            ui.ShowBossBar(true);
            ui.UpdateBossHP(14, 20);
            Capture("elite9_boss_fight.png");
            if (bossObj != null) Object.DestroyImmediate(bossObj);
            if (bossL1 != null) Object.DestroyImmediate(bossL1);
            if (bossL2 != null) Object.DestroyImmediate(bossL2);
            if (bossL3 != null) Object.DestroyImmediate(bossL3);

            // Final: Return to Main Menu & Save Scene
            ui.ShowMainMenu();
            if (playerObj != null)
            {
                playerObj.SetActive(true);
                playerObj.transform.position = new Vector3(0, -3.8f, 0);
            }
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            AssetDatabase.Refresh();

            Debug.Log("[CaptureEliteScreenshots] All 9 Elite screenshots captured successfully to Assets/Screenshots!");
        }

        private static void Capture(string fileName)
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            int w = 720, h = 1280;
            RenderTexture rt = new RenderTexture(w, h, 24);
            RenderTexture prevTarget = cam.targetTexture;
            RenderTexture prevActive = RenderTexture.active;

            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;

            Texture2D tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();

            cam.targetTexture = prevTarget;
            RenderTexture.active = prevActive;
            Object.DestroyImmediate(rt);

            string dir = "Assets/Screenshots";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            string path = Path.Combine(dir, fileName);
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }
    }
}
