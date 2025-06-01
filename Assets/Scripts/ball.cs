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
        if (currentSpeed < 0.1f)
        {
            rb.linearVelocity = Vector3.forward * targetSpeed;
        }
        else
        {
            // Ajustar la magnitud de la velocidad para que mantenga la velocidad objetivo
            rb.linearVelocity = rb.linearVelocity.normalized * targetSpeed;
        }

       if (god && transform.position.z < godZLimit)
        {   
            Debug.Log($"Rebote god mode: Ball Z={transform.position.z}, godZLimit={godZLimit}");
            Vector3 vel = rb.linearVelocity;
            if (vel.z < 0) vel.z = -vel.z;
            rb.linearVelocity = vel;
            transform.position = new Vector3(transform.position.x, transform.position.y, godZLimit);
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

        // Si es magnética y colisiona con Player o FrontTrigger, pegarse
        // pero solo si no se acaba de liberar (cooldown)
        if (isMagnetic && (collision.gameObject.CompareTag("Player") || 
                          collision.gameObject.CompareTag("FrontTrigger")) &&
            Time.time - lastReleaseTime > releaseCooldown)
        {
            Debug.Log("Magnetic ball collided with player/front trigger - sticking!");
            // Guardar el punto de contacto exacto
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

    // Añadir detección por trigger también
    void OnTriggerEnter(Collider other)
    {
        if (other == null) return;

        // Detectar pickup
        if (other.gameObject.CompareTag("PickUp"))
        {
            PickupHealth pickup = other.GetComponent<PickupHealth>();
            if (pickup != null)
            {
                pickup.TocarPelota();
            }
        }

        // Detectar colisión magnética por trigger
        // pero solo si no se acaba de liberar (cooldown)
        if (isMagnetic && other.CompareTag("FrontTrigger") &&
            Time.time - lastReleaseTime > releaseCooldown)
        {
            Debug.Log("Magnetic ball triggered with FrontTrigger - sticking!");
            // Guardar posición actual como punto de contacto
            contactPoint = transform.position;
            StickToPlayer();
        }
    }
    public void ApplyUnPowerBall()
    {
        // CORREGIDO: Aplicar el material DEFAULT (quitar PowerBall)
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = defaultMaterial;
            Debug.Log("Default material applied! PowerBall mode OFF");
        }
        else
        {
            Debug.LogWarning("Renderer not found on the ball!");
        }

        // Actualizar el estado god
        god = false;
        UnUpdatePickUpColliders();
    }

    public void ApplyPowerUp_PowerBall()
    {
        // CORREGIDO: Aplicar el material POWERBALL (activar PowerBall)
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = powerBallMaterial;
            Debug.Log("PowerBall material applied! PowerBall mode ON");
        }
        else
        {
            Debug.LogWarning("Renderer not found on the ball!");
        }

        // Actualizar el estado god
        god = true;
        UpdatePickUpColliders();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            god = !god;
        }
        // Detectar espacio para liberar la pelota magnética
        if (Input.GetKeyDown(KeyCode.Space) && isStuckToPlayer)
        {
            ReleaseBall();
        }

        // Debug: Tecla M para testing del magnetismo
        if (Input.GetKeyDown(KeyCode.M))
        {
            ApplyMagnet();
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
            PickupHealth pickup1 = pickUp.GetComponent<PickupHealth>();
            
            // Verificar que pickup1 no sea null antes de usarlo
            if (pickup1 != null)
            {
                pickup1.setPower(1);
            }
            
            if (pickupCollider != null)
            {
                Physics.IgnoreCollision(ballCollider, pickupCollider, true);
            }
        }
    }
    void UnUpdatePickUpColliders()
    {
        GameObject[] pickUps = GameObject.FindGameObjectsWithTag("PickUp");
        Collider ballCollider = GetComponent<Collider>();
        if (ballCollider == null) return;

        foreach (GameObject pickUp in pickUps)
        {
            if (pickUp == null) continue;
            
            Collider pickupCollider = pickUp.GetComponent<Collider>();
            PickupHealth pickup1 = pickUp.GetComponent<PickupHealth>();
            
            // Verificar que pickup1 no sea null antes de usarlo
            if (pickup1 != null)
            {
                pickup1.setPower(0);
            }
            
            if (pickupCollider != null)
            {
                Physics.IgnoreCollision(ballCollider, pickupCollider, false);
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

    // Nuevo método para aplicar efecto magnético
    public void ApplyMagnet()
    {
        isMagnetic = true;
        Debug.Log("Ball is now magnetic! Next collision with player will stick the ball for 10 seconds.");
        
        // Si ya había un efecto magnético activo, detenerlo y reiniciar
        if (magneticEffectCoroutine != null)
        {
            StopCoroutine(magneticEffectCoroutine);
            Debug.Log("Previous magnetic effect cancelled - restarting timer.");
        }
        
        // Iniciar nuevo contador de duración
        magneticEffectCoroutine = StartCoroutine(MagneticEffectTimer());
        
        // Cambiar material visual para indicar que es magnética
        UpdateVisualMaterial();
    }

    private IEnumerator MagneticEffectTimer()
    {
        yield return new WaitForSeconds(magneticEffectDuration);
        
        // Si la pelota está pegada al jugador, liberarla automáticamente
        if (isStuckToPlayer)
        {
            Debug.Log("Magnetic effect expired - auto-releasing ball!");
            ReleaseBall();
        }
        
        // Eliminar efecto magnético
        isMagnetic = false;
        UpdateVisualMaterial();
        magneticEffectCoroutine = null;
        Debug.Log("Magnetic effect has expired after 10 seconds.");
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
                renderer.material = magneticMaterial != null ? magneticMaterial : powerBallMaterial;
            }
            else
            {
                renderer.material = defaultMaterial;
            }
        }
    }

    private Material GetCurrentMaterial()
    {
        if (isMagnetic)
        {
            return magneticMaterial != null ? magneticMaterial : powerBallMaterial;
        }
        return defaultMaterial;
    }

    private void StickToPlayer()
    {
        if (playerTransform == null) 
        {
            Debug.LogWarning("PlayerTransform is null, cannot stick ball!");
            return;
        }

        isStuckToPlayer = true;
        
        // Calcular el radio de la pelota para posicionarla correctamente
        Collider ballCollider = GetComponent<Collider>();
        float ballRadius = 0.5f; // Valor por defecto
        if (ballCollider is SphereCollider sphere)
        {
            ballRadius = sphere.radius * transform.localScale.x;
        }
        
        // Posicionar la pelota ligeramente adelante del punto de contacto
        // para evitar que se vuelva a pegar inmediatamente
        Vector3 adjustedPosition = contactPoint;
        adjustedPosition.z += ballRadius + 0.5f; // Radio de la bola + margen extra
        
        transform.position = adjustedPosition;
        
        // Calcular offset relativo al jugador desde la posición ajustada
        stuckOffset = adjustedPosition - playerTransform.position;
        
        // Detener la pelota
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true; // Hacer kinematic para evitar física

        // Actualizar material visual
        UpdateVisualMaterial();

        Debug.Log($"Ball stuck to player at adjusted position {adjustedPosition} (contact was at {contactPoint})! Press SPACE to release or wait 10 seconds for auto-release.");
    }

    private void ReleaseBall()
    {
        isStuckToPlayer = false;
        rb.isKinematic = false; // Restaurar física
        
        // Registrar el tiempo de liberación para el cooldown
        lastReleaseTime = Time.time;

        // Lanzar la pelota hacia adelante desde la posición actual
        Vector3 releaseVelocity = Vector3.forward * GetCurrentSpeed();
        rb.linearVelocity = releaseVelocity;

        // Pequeño impulso adicional para asegurar separación
        rb.AddForce(Vector3.forward * 3f, ForceMode.Impulse);

        // Restaurar material visual normal
        UpdateVisualMaterial();

        Debug.Log("Ball released from player!");
    }

    private void UpdateBallSpeed()
    {
        if (rb != null && !isStuckToPlayer) // No actualizar velocidad si está pegada
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

    public bool IsStuckToPlayer()
    {
        return isStuckToPlayer;
    }

    public bool IsMagnetic()
    {
        return isMagnetic;
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
        if (rb != null && !isStuckToPlayer) // No reiniciar si está pegada
        {
            rb.linearVelocity = Vector3.forward * GetCurrentSpeed();
        }
    }

    // Cambiar velocidad (dificultad)
    public void SetSpeed(float newSpeed)
    {
        initialSpeed = newSpeed;
        if (rb != null && !isStuckToPlayer) // No cambiar velocidad si está pegada
        {
            Vector3 currentDirection = rb.linearVelocity.normalized;
            rb.linearVelocity = currentDirection * GetCurrentSpeed();
        }
    }
}