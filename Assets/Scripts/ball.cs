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
    public Material magneticMaterial; // Nuevo material para bola magnética
    private bool god;

    // Variables para el sistema magnético
    private bool isMagnetic = false;
    private bool isStuckToPlayer = false;
    private Transform playerTransform;
    private Vector3 stuckOffset; // Offset relativo al jugador cuando está pegada
    private Vector3 contactPoint; // Punto exacto de contacto
    
    // Variables para duración del efecto magnético
    private float magneticEffectDuration = 10f; // 10 segundos
    private Coroutine magneticEffectCoroutine;
    
    // Variables para evitar re-pegado inmediato
    private float lastReleaseTime = 0f;
    private float releaseCooldown = 0.5f; // Medio segundo de cooldown

    public float godZLimit = -35f;
    public bool showDebugInfo = true; // Añadido para solucionar error CS0103

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

        // Encontrar el jugador
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        SetPhysicsMaterial();
        UpdateVisualMaterial();
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        // Si está pegada al jugador, mantener posición relativa
        if (isStuckToPlayer && playerTransform != null)
        {
            transform.position = playerTransform.position + stuckOffset;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            return;
        }

        float currentSpeed = rb.linearVelocity.magnitude;
        float targetSpeed = GetCurrentSpeed();

        // Si está casi detenida (por bloqueo o rozamiento), relanzarla
        if (currentSpeed < 0.1f && !isStuckToPlayer)
        {
            rb.linearVelocity = (rb.linearVelocity.normalized != Vector3.zero ? rb.linearVelocity.normalized : Vector3.forward) * targetSpeed;
        }
        else if (!isStuckToPlayer)
        {
            // Ajustar la magnitud de la velocidad para que mantenga la velocidad objetivo
            rb.linearVelocity = rb.linearVelocity.normalized * targetSpeed;
        }

       if (god && transform.position.z < godZLimit && !isStuckToPlayer)
        {   
            if (showDebugInfo) Debug.Log($"Rebote god mode: Ball Z={transform.position.z}, godZLimit={godZLimit}");
            Vector3 vel = rb.linearVelocity;
            if (vel.z < 0) vel.z = -vel.z;
            rb.linearVelocity = vel;
            transform.position = new Vector3(transform.position.x, transform.position.y, godZLimit + 0.1f);
        }
    }

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
        if (showDebugInfo) Debug.Log($"Ball collided with: {collision.gameObject.name}, Tag: {collision.gameObject.tag}");

        if (isMagnetic && (collision.gameObject.CompareTag("Player") || 
                          collision.gameObject.CompareTag("FrontTrigger")) &&
            Time.time - lastReleaseTime > releaseCooldown)
        {
            if (showDebugInfo) Debug.Log("Magnetic ball collided with player/front trigger - sticking!");
            if (collision.contacts.Length > 0)
            {
                contactPoint = collision.contacts[0].point;
            }
            else
            {
                contactPoint = transform.position;
            }
            StickToPlayer();
            return;
        }

        if (collision.gameObject.CompareTag("Walls") ||
            collision.gameObject.CompareTag("FrontTrigger") ||
            collision.gameObject.CompareTag("Player"))
        {
            if (showDebugInfo) Debug.Log("Playing wall/player sound");
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayWallsPlayerSound();
            }
            else
            {
                if (showDebugInfo) Debug.LogWarning("AudioManager.Instance is null!");
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other == null) return;

        if (other.gameObject.CompareTag("PickUp"))
        {
            PickupHealth pickup = other.GetComponent<PickupHealth>();
            if (pickup != null)
            {
                if(god) pickup.setVidas(1);
                pickup.TocarPelota();
            }
        }

        if (isMagnetic && other.CompareTag("FrontTrigger") &&
            Time.time - lastReleaseTime > releaseCooldown)
        {
            if (showDebugInfo) Debug.Log("Magnetic ball triggered with FrontTrigger - sticking!");
            contactPoint = transform.position;
            StickToPlayer();
        }
    }
    public void ApplyPowerUp_PowerBall()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = powerBallMaterial;
            if (showDebugInfo) Debug.Log("PowerBall material applied!");
        }
        else
        {
            if (showDebugInfo) Debug.LogWarning("Renderer not found on the ball!");
        }
        UpdatePickUpColliders(true); // Ignorar pickups con PowerBall
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            god = !god;
            UpdateVisualMaterial();
        }
        if (Input.GetKeyDown(KeyCode.Space) && isStuckToPlayer)
        {
            ReleaseBall();
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            ApplyMagnet();
        }
    }

    public void UpdatePickUpColliders(bool ignore)
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
                Physics.IgnoreCollision(ballCollider, pickupCollider, ignore);
            }
        }
        if (showDebugInfo) Debug.Log($"Ball: PickUp colliders set to ignore: {ignore}");
    }

    public void ApplyPowerUp() // Este es el que se usa para escalar la bola (antes ApplyScaleUp)
    {
        Vector3 newScale = transform.localScale * 1.5f; // Crecer un 50%
        float scaleFactor = newScale.x / initialScale.x;
        scaleFactor = Mathf.Clamp(scaleFactor, minScaleFactor, maxScaleFactor);
        newScale = initialScale * scaleFactor;

        transform.localScale = newScale;
        if (showDebugInfo) Debug.Log("New scale applied: " + transform.localScale);
    }

    public void ApplySpeedUp()
    {
        speedMultiplier = Mathf.Min(speedMultiplier * 1.25f, 2.0f);
        UpdateBallSpeed();
        if (showDebugInfo) Debug.Log($"Speed increased! New multiplier: {speedMultiplier}, Current speed: {GetCurrentSpeed()}");
    }

    public void ApplySpeedDown()
    {
        speedMultiplier = Mathf.Max(speedMultiplier * 0.75f, 0.5f);
        UpdateBallSpeed();
        if (showDebugInfo) Debug.Log($"Speed decreased! New multiplier: {speedMultiplier}, Current speed: {GetCurrentSpeed()}");
    }

    public void ResetSpeedMultiplier()
    {
        speedMultiplier = 1f;
        UpdateBallSpeed();
        if (showDebugInfo) Debug.Log($"Speed reset! New multiplier: {speedMultiplier}, Current speed: {GetCurrentSpeed()}");
    }

    public void ApplyMagnet()
    {
        isMagnetic = true;
        if (showDebugInfo) Debug.Log("Ball is now magnetic! Next collision with player will stick the ball for " + magneticEffectDuration + " seconds.");
        
        if (magneticEffectCoroutine != null)
        {
            StopCoroutine(magneticEffectCoroutine);
            if (showDebugInfo) Debug.Log("Previous magnetic effect cancelled - restarting timer.");
        }
        
        magneticEffectCoroutine = StartCoroutine(MagneticEffectTimer());
        UpdateVisualMaterial();
    }

    private IEnumerator MagneticEffectTimer()
    {
        yield return new WaitForSeconds(magneticEffectDuration);
        
        if (isStuckToPlayer)
        {
            if (showDebugInfo) Debug.Log("Magnetic effect expired - auto-releasing ball!");
            ReleaseBall();
        }
        
        isMagnetic = false;
        UpdateVisualMaterial();
        magneticEffectCoroutine = null;
        if (showDebugInfo) Debug.Log("Magnetic effect has expired after " + magneticEffectDuration + " seconds.");
    }

    private void UpdateVisualMaterial()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            if (god)
            {
                renderer.material = powerBallMaterial;
            }
            else if (isMagnetic)
            {
                renderer.material = magneticMaterial != null ? magneticMaterial : defaultMaterial;
            }
            else
            {
                renderer.material = defaultMaterial;
            }
        }
    }

    private Material GetCurrentMaterial() // No usado activamente pero puede ser útil
    {
        if (god) return powerBallMaterial;
        if (isMagnetic) return magneticMaterial != null ? magneticMaterial : defaultMaterial;
        return defaultMaterial;
    }

    private void StickToPlayer()
    {
        if (playerTransform == null) 
        {
            if (showDebugInfo) Debug.LogWarning("PlayerTransform is null, cannot stick ball!");
            return;
        }

        isStuckToPlayer = true;
        
        Collider ballCollider = GetComponent<Collider>();
        float ballRadius = 0.5f; 
        if (ballCollider is SphereCollider sphere)
        {
            ballRadius = sphere.radius * transform.localScale.x;
        }
        
        Vector3 adjustedPosition = contactPoint;
        // Ajustar la Z para que se pegue al frente del jugador, considerando la dirección del jugador
        Vector3 playerForward = playerTransform != null ? playerTransform.forward : Vector3.forward;
        adjustedPosition = playerTransform.position + playerForward * ( (playerTransform.localScale.z / 2f) + ballRadius + 0.1f ); // Asume que el pivot del jugador está en su centro.
                                                                                                                                  // Y que la barrera/nave tiene una profundidad.
        
        transform.position = adjustedPosition;
        transform.rotation = playerTransform.rotation; // Alinear rotación
        
        stuckOffset = transform.position - playerTransform.position; // Recalcular offset
        
        if(rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true; 
        }

        UpdateVisualMaterial();
        if (showDebugInfo) Debug.Log($"Ball stuck to player at adjusted position {adjustedPosition} (contact was at {contactPoint})! Press SPACE to release or wait {magneticEffectDuration} seconds for auto-release.");
    }

    private void ReleaseBall()
    {
        if (!isStuckToPlayer) return;

        isStuckToPlayer = false;
        if(rb != null) rb.isKinematic = false; 
        
        lastReleaseTime = Time.time;

        Vector3 releaseDirection = playerTransform != null ? playerTransform.forward : Vector3.forward;
        Vector3 releaseVelocity = releaseDirection * GetCurrentSpeed();
        if(rb != null)
        {
            rb.linearVelocity = releaseVelocity;
            rb.AddForce(releaseDirection * 3f, ForceMode.Impulse);
        }

        UpdateVisualMaterial();
        if (showDebugInfo) Debug.Log("Ball released from player!");
    }

    private void UpdateBallSpeed()
    {
        if (rb != null && !isStuckToPlayer) 
        {
            Vector3 currentDirection = rb.linearVelocity.normalized;
            if (currentDirection == Vector3.zero && rb.linearVelocity.magnitude < 0.1f) {
                 currentDirection = Vector3.forward; 
            }
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

    public bool IsStuckToPlayer()
    {
        return isStuckToPlayer;
    }

    public bool IsMagnetic()
    {
        return isMagnetic;
    }

    public void DeactivateGameObjectAfterPhysics(GameObject go) // Usado por PickupHealth
    {
        StartCoroutine(DeactivateRoutine(go));
    }

    private IEnumerator DeactivateRoutine(GameObject go)
    {
        yield return new WaitForFixedUpdate(); 
        if (go != null)
        {
            go.SetActive(false);
        }
    }
    
    public void ResetBallStateAndPosition(Vector3 startPosition, Vector3 startDirection)
    {
        if (showDebugInfo) Debug.Log("Ball: Resetting state and position.");
        transform.position = startPosition;
        transform.rotation = Quaternion.LookRotation(startDirection.normalized != Vector3.zero ? startDirection.normalized : Vector3.forward);
        transform.localScale = initialScale; 
        speedMultiplier = 1f; 
        god = false;
        isMagnetic = false;
        if (isStuckToPlayer) ReleaseBall(); 
        if (magneticEffectCoroutine != null) StopCoroutine(magneticEffectCoroutine);
        magneticEffectCoroutine = null;

        UpdateVisualMaterial();

        if (rb != null)
        {
            rb.isKinematic = false; 
            rb.linearVelocity = startDirection.normalized * GetCurrentSpeed();
            rb.angularVelocity = Vector3.zero;
        }
    }

    // Este es el ResetBall que LevelPresentation está buscando
    public void ResetBall()
    {
        // Implementación simple: resetear velocidad y dirección si no está pegada.
        // Para un reseteo completo de posición, usar ResetBallStateAndPosition.
        if (rb != null && !isStuckToPlayer)
        {
            rb.linearVelocity = Vector3.forward * GetCurrentSpeed(); // Lanza hacia adelante
            rb.angularVelocity = Vector3.zero;
            transform.position = new Vector3(0, 1, 0); // Posición inicial por defecto (ajustar si es necesario)
            transform.rotation = Quaternion.identity;
        }
        if (showDebugInfo) Debug.Log("Ball: ResetBall() called. Velocity and angular velocity reset. Position reset to default.");
    }

    public void SetSpeed(float newSpeed)
    {
        initialSpeed = newSpeed;
        UpdateBallSpeed();
    }
}