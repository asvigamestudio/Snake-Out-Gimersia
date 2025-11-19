using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SnakeAIController : SnakeController
{
    public enum AIDifficulty { Easy, Medium, Hard, Insane }

    [Header("AI Settings")]
    public AIDifficulty difficulty = AIDifficulty.Medium;
    public Transform playerSnake;
    public float detectionRadius = 40f;
    public float avoidDistance = 3f;
    public float reTargetInterval = 1.2f;
    public float attackDistance = 15f;
    public float wanderRadius = 25f;
    public float avoidWeight = 2f;

    private Vector3 targetPosition;
    private float decisionTimer;
    private float boostChance;
    private float difficultyScale;
 
    private float nextPowerUpTime = 10f;

    [Header("Feedback Settings (Optional)")]
    public ParticleSystem bumpDustVFX; // Efek debu saat tabrakan
    public AudioClip bumpSound;        // Suara "thud" saat nabrak 

    // ✅ NavMesh
    private NavMeshPath navPath;
    private int currentCorner = 0;

    protected override void Start()
    {
        base.Start();
        navPath = new NavMeshPath();
        SetDifficultySettings();
        PickNewTarget();

        // Kasih body awal biar gak kependekan
        for (int i = 0; i < 5; i++)
            Grow();
    }

    void SetDifficultySettings()
    {
        switch (difficulty)
        {
            case AIDifficulty.Easy:
                detectionRadius = 25f;
                avoidDistance = 2.5f;
                boostChance = 0.05f;
                difficultyScale = 0.6f;
                break;
            case AIDifficulty.Medium:
                detectionRadius = 35f;
                avoidDistance = 3f;
                boostChance = 0.1f;
                difficultyScale = 0.8f;
                break;
            case AIDifficulty.Hard:
                detectionRadius = 50f;
                avoidDistance = 3.5f;
                boostChance = 0.2f;
                difficultyScale = 1.1f;
                break;
            case AIDifficulty.Insane:
                detectionRadius = 70f;
                avoidDistance = 4f;
                boostChance = 0.4f;
                difficultyScale = 1.4f;
                break;
        }

        baseMoveSpeed *= difficultyScale;
        rotationSpeed *= difficultyScale;
    }

    protected override void Update()
    {
        if (isDead) return;

        HandleAI();
        UpdateStamina();
        UpdatePowerUp();
    }

    protected override void FixedUpdate()
    {
        if (isDead) return;
        MoveTowardsTarget();
    }

    // ================= AI Logic =================
    void HandleAI()
    {
        decisionTimer -= Time.deltaTime;
        powerUpTimer += Time.deltaTime;

        if (decisionTimer <= 0)
        {
            PickNewTarget();
            decisionTimer = reTargetInterval;
        }

        // Boost smart
        isBoosting = Random.value < boostChance && currentStamina > 30f;

        // Simulate power-up
        // if (powerUpTimer >= nextPowerUpTime)
        // {
        //     SimulatePowerUp();
        //     powerUpTimer = 0f;
        //     nextPowerUpTime = Random.Range(8f, 15f);
        // }
    }

    void PickNewTarget()
    {
        Vector3 bestTarget = transform.position;
        float bestScore = float.MinValue;

        // Cari makanan terdekat yang bisa dilihat
        FoodPickup[] foods = FindObjectsOfType<FoodPickup>();
        foreach (var food in foods)
        {
            float dist = Vector3.Distance(transform.position, food.transform.position);
            if (dist > detectionRadius) continue;

            if (!Physics.Raycast(transform.position + Vector3.up, (food.transform.position - transform.position).normalized, dist, terrainLayer))
            {
                float score = 100f / (dist + 1f);

                if (food.data != null && food.data.scoreValue > 20)
                    score *= 1.5f; // bonus untuk makanan besar

                if (score > bestScore)
                {
                    bestScore = score;
                    bestTarget = food.transform.position;
                }
            }
        }

        // Kalau player dekat & difficulty tinggi → kejar player
        if (playerSnake != null && (difficulty == AIDifficulty.Hard || difficulty == AIDifficulty.Insane))
        {
            float distToPlayer = Vector3.Distance(transform.position, playerSnake.position);
            if (distToPlayer < attackDistance)
                bestTarget = playerSnake.position;
        }

        // Kalau gak nemu target valid → wander random
        if (bestTarget == transform.position)
        {
            Vector3 randomDir = Random.insideUnitSphere * wanderRadius;
            randomDir.y = 0;
            bestTarget = transform.position + randomDir;
        }

        SetPath(bestTarget);
    }

    // ================= Pathfinding =================
    void SetPath(Vector3 destination)
    {
        if (NavMesh.SamplePosition(destination, out NavMeshHit hit, 10f, NavMesh.AllAreas))
        {
            if (NavMesh.CalculatePath(transform.position, hit.position, NavMesh.AllAreas, navPath))
            {
                if (navPath.corners.Length > 0)
                {
                    targetPosition = navPath.corners[0];
                    currentCorner = 0;
                }
            }
        }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (isDead) return;

        // --------- HIT BODY PART ----------
        SnakeBodyPart bodyPart = other.GetComponent<SnakeBodyPart>();
        if (bodyPart != null)
        {
            // Abaikan badan sendiri
            if (bodyPart.owner == this)
                return;

            // Kena badan PLAYER atau AI lain → mati
            Debug.Log($"{name} hit {bodyPart.owner.name}'s body → AI dies");
            Kill();
            return;
        }

        // --------- HIT FOOD ----------
        FoodPickup foodPickup = other.GetComponent<FoodPickup>();
        if (foodPickup != null)
        {
            EatFood(foodPickup);
            return;
        }

        // --------- HIT POWERUP ----------
        PowerUpPickup power = other.GetComponent<PowerUpPickup>();
        if (power != null)
        {
            ActivatePowerUp(power.data);
            Destroy(other.gameObject);
            return;
        }

        // --------- HIT OBSTACLE ----------
        if (other.CompareTag("Obstacle"))
        {
            StartCoroutine(QuickBoostEscape());
            PickNewTarget();
            return;
        }

        // --------- HIT PLAYER HEAD ----------
        SnakeController player = other.GetComponent<SnakeController>();
        if (player != null && player != this)
        {
            Debug.Log($"{name} was killed by player snake!");
            Kill();
            return;
        }
    }


    void MoveTowardsTarget()
    {
        if (navPath == null || navPath.corners.Length == 0)
            return;

        Vector3 dir = (navPath.corners[currentCorner] - transform.position);
        float dist = dir.magnitude;
        dir.Normalize();

        Vector3 avoidDir = AvoidObstacles();
        dir += avoidDir * avoidWeight;
        dir.Normalize();

        float turn = Mathf.Clamp(Vector3.SignedAngle(transform.forward, dir, Vector3.up) / 45f, -1f, 1f);
        transform.Rotate(Vector3.up * turn * rotationSpeed * Time.deltaTime);

        float speed = baseMoveSpeed * moveSpeedMultiplier;
        transform.position += transform.forward * speed * Time.deltaTime;

        if (dist < 1.2f && currentCorner < navPath.corners.Length - 1)
            currentCorner++;

        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 5f, Vector3.down, out hit, groundSnapDistance, terrainLayer))
            transform.position = new Vector3(transform.position.x, hit.point.y + hoverOffset, transform.position.z);

        positionsHistory.Insert(0, transform.position);
        MoveBody();
    }

    Vector3 AvoidObstacles()
    {
        Vector3 avoid = Vector3.zero;
        RaycastHit hit;

        if (Physics.SphereCast(transform.position + Vector3.up, 0.5f, transform.forward, out hit, avoidDistance, terrainLayer))
            avoid -= hit.normal;
        if (Physics.Raycast(transform.position, transform.right, out hit, avoidDistance / 2, terrainLayer))
            avoid -= transform.right * 0.5f;
        if (Physics.Raycast(transform.position, -transform.right, out hit, avoidDistance / 2, terrainLayer))
            avoid += transform.right * 0.5f;

        foreach (var part in bodyParts)
        {
            if (part == null) continue;
            float dist = Vector3.Distance(transform.position, part.position);
            if (dist < avoidDistance)
                avoid += (transform.position - part.position).normalized * (avoidDistance - dist);
        }

        return avoid.normalized;
    }

    // =================== REACTION: OBSTACLE HIT ===================
    IEnumerator QuickBoostEscape()
    {
        Debug.Log($"{name} 🚧 AI hit wall → bounce back!");

        Vector3 backward = -transform.forward * 2f;
        transform.position += backward;

        if (bumpDustVFX != null)
            Instantiate(bumpDustVFX, transform.position, Quaternion.identity);

        if (bumpSound != null && audioSource != null)
            audioSource.PlayOneShot(bumpSound);

        float originalMultiplier = moveSpeedMultiplier;
        moveSpeedMultiplier = boostMultiplier;

        // ⛔ FIX: gunakan realtime agar tidak terkena timeScale
        yield return new WaitForSecondsRealtime(1f);

        moveSpeedMultiplier = originalMultiplier;

        PickNewTarget();
    }


    void SimulatePowerUp()
    {
        if (System.Enum.GetValues(typeof(PowerUpData.PowerUpType)).Length == 0) return;
        PowerUpData.PowerUpType type = (PowerUpData.PowerUpType)
            Random.Range(0, System.Enum.GetValues(typeof(PowerUpData.PowerUpType)).Length);

        PowerUpData fakeData = new PowerUpData
        {
            powerUpName = type.ToString(),
            type = type,
            duration = Random.Range(5f, 10f)
        };
        ActivatePowerUp(fakeData);
    }
}
