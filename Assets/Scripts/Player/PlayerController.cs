using Core;
using UnityEngine;

namespace Player
{
    public partial class PlayerController : MonoBehaviour
    {
        private static PlayerController _instance;

        [Header("Movement 2D")] public float moveSpeed = 9f;
        public float padding = 0.6f;
        [Range(0.2f, 0.6f)] public float verticalScreenPercent = 0.40f;

        [Header("Cockpit Hitbox")] public float cockpitOffsetY = 0.1f;
        public float cockpitRadius = 0.16f;
        public GameObject cockpitIndicator;

        [Header("Dash & Evasion")] public float dashSpeedMultiplier = 2.5f;
        public float dashDuration = 0.15f;
        public float dashIFrameDuration = 0.22f;
        public float dashCooldown = 1.2f;
        public GameObject dashGhostPrefab;

        [Header("Health & Lives")] public int maxLives = 3;
        public int currentLives = 3;
        public float invulnerableDuration = 1.5f;

        [Header("Weapon Progression (Lv 1 - 5)")]
        public int weaponLevel = 1;

        public GameObject laserPrefab;
        public Transform firePoint;
        public float fireRate = 0.22f;
        public GameObject homingMissilePrefab;
        public float missileInterval = 1.6f;

        [Header("Overload / Fever Mode")] public float maxOverload = 100f;
        public float currentOverload;
        public float feverDuration = 5.0f;
        public bool isFeverActive;

        [Header("Bombs & EMP Shockwave")] public GameObject shockwavePrefab;
        public int maxBombs = 3;
        public int currentBombs = 2;

        [Header("Ship Customization & Damage Sprites")]
        public Sprite[] shipSprites;

        public Sprite damageSpriteTier1; // e.g. playerShip1_damage1
        public Sprite damageSpriteTier2; // e.g. playerShip1_damage3
        public SpriteRenderer damageOverlayRenderer;

        [Header("Power-ups & Shields")] public GameObject shieldVisual;
        public GameObject floatingScorePrefab;

        [Header("Effects")] public GameObject explosionPrefab;
        [Header("Thruster Plasma VFX")] public ParticleSystem thrusterParticleSystem;

        [Header("Demo / Test Mode")] public bool autoFireForDemo;
        [Header("Perks")] public bool hasAfterburner;

        private float _dashCooldownTimer;
        private float _lastDashCooldownRatio = -1f; // throttle dash UI updates
        private Coroutine _feverCoroutine;
        private float _feverTimer;
        private Coroutine _ghostTrailCoroutine;
        private bool _isDashing;
        private bool _isDead;
        private bool _isInvulnerable;
        private Vector2 _lastMoveDir = Vector2.up;
        private float _minX, _maxX;
        private float _minY, _maxY;
        private float _nextFireTime;
        private float _nextMissileTime;
        private SpriteRenderer _spriteRenderer;
        private Vector3 _startPosition;
        private ParticleSystem.EmissionModule _thrusterEmission;
        private ParticleSystem.MainModule _thrusterMain;
        // Thruster dirty-flag: skip GPU state write when values haven't changed
        private float _lastThrusterRate = -1f;
        private float _lastThrusterSpeed = -1f;

        public static PlayerController Instance
        {
            get
            {
                if (!_instance) _instance = FindAnyObjectByType<PlayerController>(FindObjectsInactive.Include);
                return _instance;
            }
            private set => _instance = value;
        }

        public bool HasShield { get; private set; }
        public bool IsInvulnerable => _isInvulnerable || _isDashing || isFeverActive;
        public Vector3 CockpitPosition => transform.position + Vector3.up * cockpitOffsetY;
        private float DashCooldownRatio => Mathf.Clamp01(_dashCooldownTimer / dashCooldown);
        private float OverloadRatio => Mathf.Clamp01(currentOverload / maxOverload);

        private void Awake()
        {
            Instance = this;
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _startPosition = transform.position;

            if (shieldVisual != null) shieldVisual.SetActive(false);

            // Create cockpit indicator dot if not assigned
            if (cockpitIndicator == null) CreateCockpitVisualDot();

            // Setup damage overlay if missing
            EnsureDamageOverlay();

            // Setup plasma thruster exhaust
            EnsureThrusterParticles();
        }

        private void Start()
        {
            CalculateScreenBounds();
            ApplyPermanentUpgrades();
            currentLives = maxLives;
            weaponLevel = 1;
            currentOverload = 0f;
        }

        private void Update()
        {
            if (_isDead) return;
            if (!GameManager.IsActive) return;

            if (_dashCooldownTimer > 0f)
            {
                _dashCooldownTimer -= Time.deltaTime;
                // Throttle: only push to UI when value changes by >1% — avoids canvas rebuild every frame
                var ratio = DashCooldownRatio;
                if (UIManager.Instance && Mathf.Abs(ratio - _lastDashCooldownRatio) > 0.01f)
                {
                    UIManager.Instance.UpdateDashCooldown(ratio);
                    _lastDashCooldownRatio = ratio;
                }
            }

            HandleMovement();
            HandleDashInput();
            HandleShooting();
            HandleSecondaryMissiles();
            HandleBombInput();
            UpdateFeverMode();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            Instance = null;
        }

        public void ResetPlayer()
        {
            _isDead = false;
            _isInvulnerable = false;
            _isDashing = false;
            isFeverActive = false;
            hasAfterburner = false;
            weaponLevel = 1;
            currentOverload = 0f;
            _dashCooldownTimer = 0f;
            transform.position = _startPosition;

            // Cleanup companion drones
            var existingDrones = FindObjectsByType<CompanionDrone>(FindObjectsInactive.Include);
            foreach (var d in existingDrones)
                if (d != null)
                    Destroy(d.gameObject);

            if (PerkManager.Instance != null) PerkManager.Instance.ResetPerks();

            ApplyPermanentUpgrades();
            currentLives = maxLives;

            var selectedShip = PlayerPrefs.GetInt("SD_SELECTED_SHIP", 0);
            ApplyShipConfig(selectedShip);
            UpdateDamageOverlay();

            if (_spriteRenderer != null)
            {
                var c = _spriteRenderer.color;
                c.a = 1f;
                _spriteRenderer.color = c;
            }

            if (cockpitIndicator != null) cockpitIndicator.SetActive(true);

            SetThrusterRate(22f, 2.0f, new Color(0.2f, 1.0f, 1.8f, 0.85f));

            if (PostProcessingManager.Instance != null)
                PostProcessingManager.Instance.UpdateHealthVignette(currentLives, maxLives);

            if (UIManager.Instance == null) return;
            UIManager.Instance.UpdateLives(currentLives);
            UIManager.Instance.UpdateBombs(currentBombs);
            UIManager.Instance.UpdateWeaponLevel(weaponLevel);
            UIManager.Instance.UpdateOverload(OverloadRatio);
        }

        private void ApplyShipConfig(int shipIndex)
        {
            shipIndex = Mathf.Clamp(shipIndex, 0, 3);

            // 0: Blue Vanguard (Balanced)
            // 1: Orange Interceptor (Fast)
            // 2: Green Striker (Rapid Fire)
            // 3: Red Dreadnought (Armored + 3 Bombs)
            switch (shipIndex)
            {
                case 0:
                    moveSpeed = 9.5f;
                    fireRate = 0.22f;
                    maxBombs = 2;
                    currentBombs = 2;
                    HasShield = false;
                    break;
                case 1:
                    moveSpeed = 12.0f;
                    fireRate = 0.22f;
                    maxBombs = 1;
                    currentBombs = 1;
                    HasShield = false;
                    break;
                case 2:
                    moveSpeed = 9.0f;
                    fireRate = 0.16f;
                    maxBombs = 2;
                    currentBombs = 2;
                    HasShield = false;
                    break;
                case 3:
                    moveSpeed = 8.2f;
                    fireRate = 0.24f;
                    maxBombs = 3;
                    currentBombs = 3;
                    HasShield = true;
                    break;
            }

            // Reapply permanent speed upgrade on top of base
            var speedLevel = PlayerPrefs.GetInt("SD_UPGRADE_SPEED_LV", 0);
            moveSpeed *= 1f + speedLevel * 0.06f;

            if (shieldVisual != null) shieldVisual.SetActive(HasShield);

            if (shipSprites == null || shipIndex >= shipSprites.Length || shipSprites[shipIndex] == null) return;
            if (_spriteRenderer != null)
                _spriteRenderer.sprite = shipSprites[shipIndex];
        }

        private void CalculateScreenBounds()
        {
            var cam = Camera.main;
            if (cam != null)
            {
                var halfWidth = cam.orthographicSize * cam.aspect;
                _minX = -halfWidth + padding;
                _maxX = halfWidth - padding;

                var screenHeight = cam.orthographicSize * 2f;
                _minY = -cam.orthographicSize + padding;
                _maxY = _minY + screenHeight * verticalScreenPercent;
            }
            else
            {
                _minX = -4.5f;
                _maxX = 4.5f;
                _minY = -4.5f;
                _maxY = -0.5f;
            }
        }

        private void CreateCockpitVisualDot()
        {
            var dotGo = new GameObject("CockpitDot");
            dotGo.transform.SetParent(transform);
            dotGo.transform.localPosition = new Vector3(0f, cockpitOffsetY, 0f);
            dotGo.transform.localScale = new Vector3(0.2f, 0.2f, 1f);

            var sr = dotGo.AddComponent<SpriteRenderer>();
            sr.color = new Color(0.2f, 0.95f, 1f, 0.8f);
            sr.sortingOrder = _spriteRenderer ? _spriteRenderer.sortingOrder + 2 : 12;

            cockpitIndicator = dotGo;
        }

        private void EnsureThrusterParticles()
        {
            if (thrusterParticleSystem == null)
            {
                var child = transform.Find("ThrusterParticles");
                if (child != null)
                {
                    thrusterParticleSystem = child.GetComponent<ParticleSystem>();
                }
                else
                {
                    var thrusterObj = new GameObject("ThrusterParticles");
                    thrusterObj.transform.SetParent(transform, false);
                    thrusterObj.transform.localPosition = new Vector3(0f, -0.45f, 0f);
                    thrusterObj.transform.localEulerAngles = new Vector3(90f, 0f, 0f);

                    thrusterParticleSystem = thrusterObj.AddComponent<ParticleSystem>();
                    var main = thrusterParticleSystem.main;
                    main.startLifetime = 0.22f;
                    main.startSpeed = 2.4f;
                    main.startSize = 0.24f;
                    main.startColor = new Color(0.2f, 1.1f, 2.0f, 0.85f);
                    main.simulationSpace = ParticleSystemSimulationSpace.World;
                    main.scalingMode = ParticleSystemScalingMode.Hierarchy;

                    var emission = thrusterParticleSystem.emission;
                    emission.rateOverTime = 25f;

                    var shape = thrusterParticleSystem.shape;
                    shape.shapeType = ParticleSystemShapeType.Cone;
                    shape.angle = 12f;
                    shape.radius = 0.08f;

                    var rend = thrusterObj.GetComponent<ParticleSystemRenderer>();
                    if (rend != null)
                    {
                        var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ??
                                     Shader.Find("Mobile/Particles/Additive") ??
                                     Shader.Find("Sprites/Default");
                        if (shader != null) rend.material = new Material(shader);
                        rend.sortingOrder = 9;
                    }
                }
            }

            if (thrusterParticleSystem == null) return;
            _thrusterEmission = thrusterParticleSystem.emission;
            _thrusterMain = thrusterParticleSystem.main;
        }

        private void SetThrusterRate(float rate, float speed, Color color)
        {
            if (!thrusterParticleSystem) return;
            // Dirty-flag guard: skip write if values haven't meaningfully changed
            if (Mathf.Abs(rate - _lastThrusterRate) < 0.5f && Mathf.Abs(speed - _lastThrusterSpeed) < 0.1f) return;
            _lastThrusterRate = rate;
            _lastThrusterSpeed = speed;
            _thrusterEmission.rateOverTime = rate;
            _thrusterMain.startSpeed = speed;
            _thrusterMain.startColor = color;
        }
    }
}