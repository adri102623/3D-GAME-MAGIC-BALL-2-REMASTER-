using UnityEngine;
using System.Collections;

public class Ball : MonoBehaviour
{
    public float initialSpeed = 15f; // Velocidad base de la pelota
    private float speedMultiplier = 1f; // Multiplicador de velocidad
    private Rigidbody rb;
    private Vector3 initialScale;
    private float maxScaleFactor = 2f;
    private float minScaleFactor = 0.5f;

    public Material powerBallMaterial;
    public Material defaultMaterial;
    private bool god;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Anular pérdidas de velocidad
            rb.linearDamping = 0f;
            rb.angularDamping = 0f;
            rb.useGravity = false;

            // Lanzar la pelota hacia adelante
            rb.linearVelocity = Vector3.forward * GetCurrentSpeed();
        }

        initialScale = transform.localScale;
        god = false;

        SetPhysicsMaterial();
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        float currentSpeed = rb.linearVelocity.magnitude;
        float targetSpeed = GetCurrentSpeed();

        // Si está casi detenida (por bloqueo o rozamiento), relanzarla
        if (currentSpeed < 0.1f)
        {
            rb.linearVelocity = Vector3.forward * targetSpeed;
        }
        else
        {
            // Ajustar la magnitud de la velocidad para que mantenga la velocidad objetivo
            rb.linearVelocity = rb.linearVelocity.normalized * targetSpeed;
        }
    }

    // Configura material de física para rebotes sin pérdida
    void SetPhysicsMaterial()
    {
        Collider ballCollider = GetComponent<Collider>();
        if (ballCollider != null)
        {
            PhysicsMaterial ballPhysics = new PhysicsMaterial("BallPhysics");
            ballPhysics.bounciness = 1f;
            ballPhysics.staticFriction = 0f;
            ballPhysics.dynamicFriction = 0f;
            ballPhysics.frictionCombine = PhysicsMaterialCombine.Minimum;
            ballPhysics.bounceCombine = PhysicsMaterialCombine.Maximum;

            ballCollider.material = ballPhysics;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Ball collided with: {collision.gameObject.name}, Tag: {collision.gameObject.tag}");

        // Reproducir sonido cuando la pelota toque paredes, FrontTrigger o Player
        if (collision.gameObject.CompareTag("Walls") ||
            collision.gameObject.CompareTag("FrontTrigger") ||
            collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Playing wall/player sound");
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayWallsPlayerSound();
            }
            else
            {
                Debug.LogWarning("AudioManager.Instance is null!");
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            god = !god;
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = god ? powerBallMaterial : defaultMaterial;
            }
            UpdatePickUpColliders();
        }
    }

    void UpdatePickUpColliders()
    {
        GameObject[] pickUps = GameObject.FindGameObjectsWithTag("PickUp");
        Collider ballCollider = GetComponent<Collider>();
        if (ballCollider == null) return;

        foreach (GameObject pickUp in pickUps)
        {
            if (pickUp == null) continue;
            Collider pickupCollider = pickUp.GetComponent<Collider>();
            if (pickupCollider != null)
            {
                // Ignorar colisiones cuando es "god" (opcional)
                Physics.IgnoreCollision(ballCollider, pickupCollider, god);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other == null) return;
        if (other.gameObject.CompareTag("PickUp"))
        {
            PickupHealth pickup = other.GetComponent<PickupHealth>();
            if (pickup != null && !god)
            {
                pickup.TocarPelota();
            }
        }
    }

    public void ApplyPowerUp()
    {
        Vector3 newScale = transform.localScale;
        newScale *= 2f; // crecer al doble

        // Limitar el escalado entre 0.5x y 2x de la escala original
        float scaleFactor = newScale.x / initialScale.x;
        scaleFactor = Mathf.Clamp(scaleFactor, minScaleFactor, maxScaleFactor);
        newScale = initialScale * scaleFactor;

        transform.localScale = newScale;
        Debug.Log("New scale applied: " + transform.localScale);
    }

    // Nuevos métodos para gestión de velocidad
    public void ApplySpeedUp()
    {
        speedMultiplier = 1.5f;
        UpdateBallSpeed();
        Debug.Log($"Speed increased! New multiplier: {speedMultiplier}, Current speed: {GetCurrentSpeed()}");
    }

    public void ApplySpeedDown()
    {
        speedMultiplier = 0.65f;
        UpdateBallSpeed();
        Debug.Log($"Speed decreased! New multiplier: {speedMultiplier}, Current speed: {GetCurrentSpeed()}");
    }

    public void ResetSpeedMultiplier()
    {
        speedMultiplier = 1f;
        UpdateBallSpeed();
        Debug.Log($"Speed reset! New multiplier: {speedMultiplier}, Current speed: {GetCurrentSpeed()}");
    }

    private void UpdateBallSpeed()
    {
        if (rb != null)
        {
            Vector3 currentDirection = rb.linearVelocity.normalized;
            rb.linearVelocity = currentDirection * GetCurrentSpeed();
        }
    }

    private float GetCurrentSpeed()
    {
        return initialSpeed * speedMultiplier;
    }

    public float GetSpeedMultiplier()
    {
        return speedMultiplier;
    }

    IEnumerator DeactivateAfterPhysics(GameObject pickup)
    {
        yield return new WaitForFixedUpdate();
        if (pickup != null)
        {
            pickup.SetActive(false);
        }
    }

    // Reiniciar la pelota
    public void ResetBall()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.forward * GetCurrentSpeed();
        }
    }

    // Cambiar velocidad (dificultad)
    public void SetSpeed(float newSpeed)
    {
        initialSpeed = newSpeed;
        if (rb != null)
        {
            Vector3 currentDirection = rb.linearVelocity.normalized;
            rb.linearVelocity = currentDirection * GetCurrentSpeed();
        }
    }
}