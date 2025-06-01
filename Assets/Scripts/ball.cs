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

    public CameraIntroMove cameraIntroMove; 
    private bool ballLaunched = false;

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
        
        // NUEVO: Añadir BallDestroyer si no existe
        if (GetComponent<BallDestroyer>() == null)
        {
            gameObject.AddComponent<BallDestroyer>();
        }
    }

    void FixedUpdate()
    {
        if (rb == null) return;
        
        //  Verificar que cameraIntroMove no sea null
        if (cameraIntroMove != null && !cameraIntroMove.introFinished)
            return;

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
        //  Aplicar el material DEFAULT (quitar PowerBall)
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
        //  Aplicar el material POWERBALL (activar PowerBall)
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

         if (cameraIntroMove != null && !cameraIntroMove.introFinished)
        return;

        if (!ballLaunched)
        {
            LaunchBall();
            ballLaunched = true;
        }
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
        
        // NUEVO: Añadir teclas 1-5 para cambiar niveles
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (SceneTransitionManager.Instance != null)
                SceneTransitionManager.Instance.LoadLevel(0); // lvl1
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (SceneTransitionManager.Instance != null)
                SceneTransitionManager.Instance.LoadLevel(1); // lvl2
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (SceneTransitionManager.Instance != null)
                SceneTransitionManager.Instance.LoadLevel(2); // lvl3
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            if (SceneTransitionManager.Instance != null)
                SceneTransitionManager.Instance.LoadLevel(3); // lvl4
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            if (SceneTransitionManager.Instance != null)
                SceneTransitionManager.Instance.LoadLevel(4); // lvl5
        }
    }

        
    private void LaunchBall()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.forward * GetCurrentSpeed();
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

    //  Método para multiplicar bolas sin errores de NaN
    public void ApplyBallMultiplier()
    {
        Debug.Log("Ball Multiplier activated! Spawning 2 additional balls...");
        
        // Obtener información de la bola actual
        Vector3 currentPosition = transform.position;
        float currentSpeed = GetCurrentSpeed();
        Vector3 currentVelocity = rb.linearVelocity;
        
        // Cargar el prefab de la bola desde Resources
        GameObject ballPrefab = Resources.Load<GameObject>("Prefabs/Ball");
        if (ballPrefab == null)
        {
            Debug.LogError("Ball prefab not found in Resources/Prefabs/Ball");
            return;
        }
        
        // Crear 2 pelotas adicionales SIEMPRE
        for (int i = 0; i < 2; i++)
        {
            // Posiciones separadas para evitar solapamiento
            float offsetX = (i == 0) ? -1.5f : 1.5f;
            Vector3 spawnPosition = currentPosition + new Vector3(offsetX, 0, 0);
            
            // Instanciar la nueva bola
            GameObject newBall = Instantiate(ballPrefab, spawnPosition, Quaternion.identity);
            
            // Configurar la nueva bola
            Ball newBallScript = newBall.GetComponent<Ball>();
            if (newBallScript != null)
            {
                //  Asignar cameraIntroMove si existe
                newBallScript.cameraIntroMove = this.cameraIntroMove;
                
                //  Inicializar manualmente initialScale antes de usarlo
                newBallScript.initialScale = newBall.transform.localScale;
                
                // Copiar propiedades de la bola original
                newBallScript.initialSpeed = this.initialSpeed;
                newBallScript.speedMultiplier = this.speedMultiplier;
                
                // NUEVO: Añadir BallDestroyer a las nuevas bolas
                if (newBall.GetComponent<BallDestroyer>() == null)
                {
                    newBall.AddComponent<BallDestroyer>();
                }
                
                // Configurar Rigidbody
                Rigidbody newRb = newBall.GetComponent<Rigidbody>();
                if (newRb != null)
                {
                    // Aplicar configuración de física similar
                    newRb.linearDamping = 0f;
                    newRb.angularDamping = 0f;
                    newRb.useGravity = false;
                    
                    // Calcular dirección diferente para cada bola
                    float angle = (i == 0) ? -30f : 30f; // -30° y +30° respecto a la dirección actual
                    Vector3 newDirection = RotateVectorY(currentVelocity.normalized, angle);
                    
                    // Aplicar velocidad con la nueva dirección
                    newRb.linearVelocity = newDirection * currentSpeed;
                    
                    Debug.Log($"New ball {i+1} spawned at {spawnPosition} with direction {newDirection} and speed {currentSpeed}");
                }
                
                // Copiar todos los estados de la bola original
                if (god) // Si la bola original está en modo god
                {
                    newBallScript.ApplyPowerUp_PowerBall();
                }
                
                if (isMagnetic) // Si la bola original es magnética
                {
                    newBallScript.ApplyMagnet();
                }
                
                //  Calcular escala de forma segura
                // Calcular el factor de escala actual respecto al tamaño original
                float currentScaleFactor = this.transform.localScale.x / this.initialScale.x;
                
                // Verificar que no hay valores inválidos
                if (float.IsNaN(currentScaleFactor) || float.IsInfinity(currentScaleFactor))
                {
                    currentScaleFactor = 1f; // Valor por defecto seguro
                    Debug.LogWarning("Invalid scale factor detected, using default value 1.0");
                }
                
                // Limitar el factor de escala a máximo x2 de la escala original
                float clampedScaleFactor = Mathf.Clamp(currentScaleFactor, minScaleFactor, maxScaleFactor);
                
                // Aplicar la escala limitada usando la escala inicial de la nueva bola
                Vector3 newBallScale = newBallScript.initialScale * clampedScaleFactor;
                
                // Verificar que la escala resultante es válida antes de aplicarla
                if (!float.IsNaN(newBallScale.x) && !float.IsNaN(newBallScale.y) && !float.IsNaN(newBallScale.z))
                {
                    newBallScript.transform.localScale = newBallScale;
                    Debug.Log($"New ball scale: Original={newBallScript.initialScale}, Current factor={currentScaleFactor:F2}, Clamped factor={clampedScaleFactor:F2}, Final scale={newBallScale}");
                }
                else
                {
                    Debug.LogError($"Invalid scale calculated: {newBallScale}. Using original scale.");
                    // Mantener la escala original del prefab
                }
            }
        }
        
        Debug.Log("Ball Multiplier: 2 additional balls created successfully!");
    }

    // Método auxiliar para rotar un vector en el eje Y
    private Vector3 RotateVectorY(Vector3 vector, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);
        
        return new Vector3(
            vector.x * cos + vector.z * sin,
            vector.y,
            -vector.x * sin + vector.z * cos
        );
    }
}