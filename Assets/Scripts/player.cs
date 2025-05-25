using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Threading;


public class Player : MonoBehaviour
{
    // Rigidbody of the player.
    private Rigidbody rb;

    private int count;




    // Speed at which the player moves.
    public float speed = 0;

    public TextMeshProUGUI countText;




    // Movement along X and Y axes.
    private float movementX;
    private float movementY;



    // Referencia a la pelota
    private Ball ball;

    // Trigger frontal para detectar monedas
    public Transform frontTrigger; // Asignar un objeto hijo con un collider trigger
    private Transform barrier;
    private Vector3 initialBarrierScale;
    private float maxBarrierScaleFactor = 3f; // Máximo 2x la escala inicial en X
    private float minBarrierScaleFactor = 1f; // Mínimo 0.5x la escala inicial en X

    // Start is called before the first frame update.
    void Start()
    {
        count = 0;
        // Get and store the Rigidbody component attached to the player.
        rb = GetComponent<Rigidbody>();

        rb.freezeRotation = true; // Bloquea todas las rotaciones

        // Asegúrate de que rb no sea null
        if (rb == null)
        {
            Debug.LogError("Rigidbody no encontrado en el objeto!");
        }

        // Buscar la pelota al inicio
        ball = FindFirstObjectByType<Ball>();
        if (ball == null)
        {
            Debug.LogWarning("Ball not found! Ensure an object with Ball component exists in the scene.");
        }

        // Asegurarse de que frontTrigger esté asignado
        if (frontTrigger == null)
        {
            Debug.LogWarning("FrontTrigger not assigned! Please assign a Transform with a trigger collider in the Inspector.");
        }

        barrier = transform.Find("Barrier");
        if (barrier == null)
        {
            Debug.LogError("Barrier not found as a child of Fighter_01! Ensure the hierarchy contains a Barrier object.");
        }
        else
        {
            initialBarrierScale = barrier.localScale;
        }
    }

    // This function is called when a move input is detected.
    void OnMove(InputValue movementValue)
    {
        // Convert the input value into a Vector2 for movement.
        Vector2 movementVector = movementValue.Get<Vector2>();

        // Store the X and Y components of the movement.
        movementX = movementVector.x;
        movementY = movementVector.y;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            rb.linearVelocity = Vector3.zero; // Evita que se mueva por el impacto
        }
    }

    public void ApplyPowerUp_MaxBarrier()
    {
        if (barrier != null)
        {
            Vector3 newScale = barrier.localScale;
            Debug.Log("Before scaling - Barrier scale: " + newScale);
            newScale.x *= 1.5f;
            // Limitar el factor entre minBarrierScaleFactor y maxBarrierScaleFactor
            newScale.x = Mathf.Clamp(newScale.x, minBarrierScaleFactor, maxBarrierScaleFactor);
            barrier.localScale = newScale;
            Debug.Log("After scaling - Barrier scale: " + barrier.localScale);

            // Actualizar el Collider si existe
            BoxCollider barrierCollider = barrier.GetComponent<BoxCollider>();
            if (barrierCollider != null)
            {
                barrierCollider.size = new Vector3(newScale.x, barrierCollider.size.y, barrierCollider.size.z);
                Debug.Log("Barrier Collider size updated to: " + barrierCollider.size);
            }
            else
            {
                Debug.LogWarning("Barrier has no BoxCollider or incompatible Collider type!");
            }

            // Verificar el estado del Rigidbody del jugador
            if (rb != null)
            {
                Debug.Log("Player Rigidbody velocity after scaling: " + rb.linearVelocity);
                Debug.Log("Player Rigidbody constraints: " + rb.constraints);
            }
            else
            {
                Debug.LogError("Rigidbody is null after scaling!");
            }
        }
        else
        {
            Debug.LogWarning("Barrier reference is null in Player!");
        }
    }
    // FixedUpdate is called once per fixed frame-rate frame.
    void FixedUpdate()
    {
        Vector3 movement = new Vector3(movementX, 0f, movementY).normalized;
        rb.linearVelocity = movement * speed;
    }
}