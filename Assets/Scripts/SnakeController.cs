using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SnakeController : MonoBehaviour
{
    [Header("Skin Settings")]
    public SnakeSkinData currentSkin;

    [Header("Movement Settings")]
    public float baseMoveSpeed = 5f;
    public float moveSpeedMultiplier = 1f;
    public float rotationSpeed = 200f;
    public float bodySpeed = 5f;
    public float gap = 0.5f;

    [Header("Boost Settings")]
    public float boostMultiplier = 3f;
    public float maxStamina = 100f;
    public float staminaDrainRate = 40f;
    public float staminaRegenRate = 20f;
    protected float currentStamina;
    protected bool isBoosting = false;

    [Header("Growth Settings")]
    public float bodyScaleDecrease = 0.95f;
    protected List<Transform> bodyParts = new List<Transform>();
    protected List<Vector3> positionsHistory = new List<Vector3>();
    protected GameObject headObject;
    protected GameObject bodyPrefab;
    protected Transform bodyParent;

    [Header("Gameplay")]
    public int score = 0;
    public Text scoreText;
    public bool isDead = false;

    [Header("PowerUp Effects")]
    public ParticleSystem powerUpEffect;

    [Header("Terrain Settings")]
    public float groundSnapDistance = 100f;
    public float hoverOffset = 0.05f;
    public float fallSpeed = 5f;
    public float slopeAlignSpeed = 5f;
    public LayerMask terrainLayer;

    [Header("VFX & Death Settings")]
    public ParticleSystem dissolveParticle;
    public Material dissolveMaterial;
    protected Material originalMaterial;

    // AUDIO
    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip eatSound;
    public AudioClip boostStartSound;
    public AudioClip boostLoopSound;
    public AudioClip boostEndSound;
    private bool boostSoundPlaying = false;

    // Sprint VFX
    [Header("Sprint VFX Settings")]
    public TrailRenderer speedTrail;
    public ParticleSystem boostParticle;
    public float trailFadeSpeed = 5f;

    // Power-Up System
    protected bool hasShield = false;
    protected float magnetRadius = 0f;
    protected PowerUpData activePowerUp;
    protected float powerUpTimer;

    // NEW FIX — separate speed multiplier for power-up
    private float speedPowerMultiplier = 1f;

    // -------------------------
    // Lifecycle
    // -------------------------
    protected virtual void Start()
    {
        ApplySkin(currentSkin);
        currentStamina = maxStamina;

        if (bodyParent == null)
        {
            bodyParent = new GameObject("SnakeBodyParent").transform;
            bodyParent.SetParent(null);
        }

        if (speedTrail != null)
            speedTrail.emitting = false;
    }

    protected virtual void Update()
    {
        if (isDead) return;

        scoreText.text = $"Score: {score}";

        HandleBoostInput();
        UpdateStamina();
        UpdatePowerUp();
    }

    protected virtual void FixedUpdate()
    {
        if (isDead) return;
        Move();
    }

    // =================== SKIN SYSTEM ===================
    public virtual void ApplySkin(SnakeSkinData skin)
    {
        if (skin == null) return;
        currentSkin = skin;

        baseMoveSpeed = skin.baseSpeed;
        boostMultiplier = skin.boostMultiplier;
        maxStamina = skin.maxStamina;
        staminaDrainRate = skin.staminaDrainRate;
        staminaRegenRate = skin.staminaRegenRate;

#if UNITY_EDITOR
        if (headObject != null)
            DestroyImmediate(headObject);
#else
        if (headObject != null)
            Destroy(headObject);
#endif

        if (skin.headPrefab != null)
        {
            headObject = Instantiate(skin.headPrefab, transform);
            headObject.transform.localPosition = Vector3.zero;
            headObject.transform.localRotation = Quaternion.identity;
        }

        if (skin.bodyPrefab != null)
            bodyPrefab = skin.bodyPrefab;

        Renderer headRenderer = GetComponentInChildren<Renderer>();
        if (headRenderer != null)
            originalMaterial = headRenderer.material;
    }

    // =================== BOOST SYSTEM ===================
    protected virtual void HandleBoostInput()
    {
        bool wasBoosting = isBoosting;

        if (Input.GetKey(KeyCode.Space) && currentStamina > 0)
            isBoosting = true;
        else if (Input.GetKeyUp(KeyCode.Space) || currentStamina <= 0)
            isBoosting = false;

        if (isBoosting && !wasBoosting)
        {
            if (audioSource != null && boostStartSound != null)
                audioSource.PlayOneShot(boostStartSound);

            if (audioSource != null && boostLoopSound != null)
            {
                audioSource.clip = boostLoopSound;
                audioSource.loop = true;
                audioSource.Play();
                boostSoundPlaying = true;
            }

            if (speedTrail != null)
                speedTrail.emitting = true;
            if (boostParticle != null)
                boostParticle.Play();

            CameraShake(0.2f, 0.1f);
        }
        else if (!isBoosting && wasBoosting)
        {
            if (audioSource != null && boostEndSound != null)
                audioSource.PlayOneShot(boostEndSound);
            if (audioSource != null && boostSoundPlaying)
            {
                audioSource.Stop();
                audioSource.loop = false;
                boostSoundPlaying = false;
            }

            if (speedTrail != null)
                StartCoroutine(FadeOutTrail(speedTrail));
            if (boostParticle != null)
                boostParticle.Stop();
        }
    }

    IEnumerator FadeOutTrail(TrailRenderer trail)
    {
        if (trail == null) yield break;

        float originalTime = trail.time;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * trailFadeSpeed;
            trail.time = Mathf.Lerp(originalTime, 0f, t);
            yield return null;
        }

        trail.emitting = false;
        trail.time = originalTime;
    }

    protected virtual void UpdateStamina()
    {
        if (isBoosting)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
            currentStamina = Mathf.Max(currentStamina, 0);
            moveSpeedMultiplier = boostMultiplier;
        }
        else
        {
            if (currentStamina < maxStamina)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                currentStamina = Mathf.Min(currentStamina, maxStamina);
            }
            moveSpeedMultiplier = 1f;
        }
    }

    // =================== MOVEMENT ===================
    protected virtual void Move()
    {
        // FIXED: Apply additional speed multiplier from power-up
        float currentMoveSpeed = baseMoveSpeed * moveSpeedMultiplier * speedPowerMultiplier;

        float h = Input.GetAxis("Horizontal");

        transform.Rotate(Vector3.up * h * rotationSpeed * Time.deltaTime);

        Vector3 forwardFlat = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
        transform.position += forwardFlat * currentMoveSpeed * Time.deltaTime;

        positionsHistory.Insert(0, transform.position);

        RaycastHit headHit;
        if (Physics.Raycast(transform.position + Vector3.up * 5f, Vector3.down, out headHit, groundSnapDistance, terrainLayer))
            transform.position = new Vector3(transform.position.x, headHit.point.y + hoverOffset, transform.position.z);

        MoveBody();

        if (positionsHistory.Count > 5000)
            positionsHistory.RemoveRange(5000, positionsHistory.Count - 5000);
    }

    protected virtual void MoveBody()
    {
        for (int i = 0; i < bodyParts.Count; i++)
        {
            Transform body = bodyParts[i];
            float distance = gap * (i + 1);
            Vector3 targetPos = GetPositionAtDistance(distance);
            body.position = Vector3.Lerp(body.position, targetPos, bodySpeed * Time.deltaTime);
            body.LookAt(targetPos);
        }
    }

    protected Vector3 GetPositionAtDistance(float distance)
    {
        if (positionsHistory.Count < 2)
            return transform.position;

        float accumulatedDistance = 0f;
        for (int i = 1; i < positionsHistory.Count; i++)
        {
            float segmentDistance = Vector3.Distance(positionsHistory[i - 1], positionsHistory[i]);
            accumulatedDistance += segmentDistance;
            if (accumulatedDistance >= distance)
            {
                float t = (accumulatedDistance - distance) / segmentDistance;
                return Vector3.Lerp(positionsHistory[i], positionsHistory[i - 1], t);
            }
        }

        return positionsHistory[positionsHistory.Count - 1];
    }

    // =================== TRIGGERS ===================
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (isDead) return;

        // ================= HEAD vs HEAD =================
        SnakeController otherSnake = other.GetComponent<SnakeController>();
        if (otherSnake != null && otherSnake != this)
        {
            // Pastikan ini kepala vs kepala (hindari tabrak badan)
            if (!other.transform.IsChildOf(transform) &&
                !other.transform.IsChildOf(bodyParent))
            {
                int myLength = bodyParts.Count;
                int otherLength = otherSnake.bodyParts.Count;

                Debug.Log($"⚔ HEAD-ON collision! {name}({myLength}) vs {otherSnake.name}({otherLength})");

                if (myLength > otherLength)
                {
                    Debug.Log($"{name} WINS head collision!");
                    otherSnake.Die();     // lawan mati
                    return;
                }
                else if (myLength < otherLength)
                {
                    Debug.Log($"{otherSnake.name} WINS head collision!");
                    Die();                // kamu mati
                    return;
                }
                else
                {
                    Debug.Log("Both snakes same size → both die!");
                    Die();
                    otherSnake.Die();
                    return;
                }
            }
        }

        // ================= BODY vs HEAD / BODY =================
        SnakeBodyPart bodyPart = other.GetComponent<SnakeBodyPart>();
        if (bodyPart != null)
        {
            if (bodyPart.owner == this) return;

            if (hasShield)
            {
                hasShield = false;
                return;
            }

            Die();
            return;
        }

        // ================= FOOD =================
        FoodPickup foodPickup = other.GetComponent<FoodPickup>();
        if (foodPickup != null)
        {
            EatFood(foodPickup);
            return;
        }

        // ================= OBSTACLE =================
        if (other.CompareTag("Obstacle"))
        {
            if (hasShield)
            {
                hasShield = false;
                return;
            }
            Die();
            return;
        }

        // ================= POWER-UP =================
        PowerUpPickup powerUp = other.GetComponent<PowerUpPickup>();
        if (powerUp != null)
        {
            ActivatePowerUp(powerUp.data);
            Destroy(other.gameObject);
            return;
        }
    }


    // =================== FOOD ===================
    public virtual void EatFood(FoodPickup food)
    {
        if (food == null || food.data == null) return;

        if (food.data.foodType == FoodData.FoodType.PowerUp && food.data.powerUpData != null)
        {
            ActivatePowerUp(food.data.powerUpData);
        }
        else
        {
            int gainedScore = food.data.scoreValue;
            score += gainedScore;

            if (GameProgressManager.Instance != null)
                GameProgressManager.Instance.AddScore(gainedScore);

            currentStamina = Mathf.Min(currentStamina + food.data.staminaBonus, maxStamina);
            for (int i = 0; i < food.data.growCount; i++)
                Grow();
        }

        if (audioSource != null && eatSound != null)
            audioSource.PlayOneShot(eatSound);

        Destroy(food.gameObject);
    }

    protected virtual void Grow()
    {
        if (bodyPrefab == null) return;

        GameObject newPart = Instantiate(bodyPrefab, bodyParent);
        newPart.transform.localScale *= Mathf.Pow(bodyScaleDecrease, bodyParts.Count + 1);
        newPart.tag = "Body";

        SnakeBodyPart bodyComp = newPart.GetComponent<SnakeBodyPart>();
        if (bodyComp == null)
            bodyComp = newPart.AddComponent<SnakeBodyPart>();
        bodyComp.owner = this;

        bodyParts.Add(newPart.transform);
    }

    // =================== POWER-UP ===================
    public virtual void ActivatePowerUp(PowerUpData data)
    {
        if (data == null) return;

        activePowerUp = data;
        powerUpTimer = data.duration;

        if (powerUpEffect)
        {
            var fx = Instantiate(powerUpEffect, transform.position, Quaternion.identity);
            Destroy(fx.gameObject, 2f);
        }

        switch (data.type)
        {
            case PowerUpData.PowerUpType.Speed:
                speedPowerMultiplier = 5f;
                break;

            case PowerUpData.PowerUpType.Shield:
                hasShield = true;
                break;

            case PowerUpData.PowerUpType.Magnet:
                magnetRadius = 10f;
                break;

            case PowerUpData.PowerUpType.TimeSlow:
                Time.timeScale = 0.5f;
                break;
        }
    }

    protected virtual void UpdatePowerUp()
    {
        if (activePowerUp == null) return;

        // IMPORTANT FIX: unscaled time so slow-motion doesn’t break timer
        powerUpTimer -= Time.unscaledDeltaTime;

        if (powerUpTimer <= 0)
            DeactivatePowerUp();
    }

    protected virtual void DeactivatePowerUp()
    {
        if (activePowerUp == null) return;

        switch (activePowerUp.type)
        {
            case PowerUpData.PowerUpType.Speed:
                speedPowerMultiplier = 1f;
                break;

            case PowerUpData.PowerUpType.Shield:
                hasShield = false;
                break;

            case PowerUpData.PowerUpType.Magnet:
                magnetRadius = 0f;
                break;

            case PowerUpData.PowerUpType.TimeSlow:
                Time.timeScale = 1f;
                break;
        }

        activePowerUp = null;
    }

    // =================== DEATH ===================
    protected virtual IEnumerator DissolveAll()
    {
        float delay = 0.05f;
        var renderers = new List<Renderer>();
        renderers.AddRange(GetComponentsInChildren<Renderer>());
        if (bodyParent != null)
            renderers.AddRange(bodyParent.GetComponentsInChildren<Renderer>());

        foreach (var r in renderers)
        {
            if (r != null && dissolveMaterial != null)
                r.material = dissolveMaterial;
            yield return new WaitForSeconds(delay);
        }
    }

    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;

        StartCoroutine(DissolveAll());

        if (dissolveParticle)
            Instantiate(dissolveParticle, transform.position, Quaternion.identity);

        if (bodyParent != null)
            Destroy(bodyParent.gameObject, 2f);

        enabled = false;

        if (CompareTag("Player"))
            GameManager.Instance?.OnPlayerDeath();
        else
            GameManager.Instance?.OnEnemyDeath();
    }

    // =================== CAMERA SHAKE ===================
    void CameraShake(float intensity, float duration)
    {
        StartCoroutine(DoShake(intensity, duration));
    }

    IEnumerator DoShake(float intensity, float duration)
    {
        Camera cam = Camera.main;
        if (!cam) yield break;

        Vector3 originalPos = cam.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cam.transform.localPosition = originalPos + Random.insideUnitSphere * intensity;
            yield return null;
        }

        cam.transform.localPosition = originalPos;
    }

    // =================== STATE ===================
    public bool IsDead()
    {
        return isDead;
    }

    public void Kill()
    {
        Die();
    }

}
