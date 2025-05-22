using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;


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

    void OnTriggerEnter(Collider other)
    {
        // Solo el trigger frontal detectará las monedas
        if (other.CompareTag("Coin"))
        {
            PowerUpCoin coin = other.GetComponent<PowerUpCoin>();
            if (coin != null)
            {
                if (ball != null)
                {
                    ball.ApplyPowerUp();
                    Debug.Log("Power-up applied: Maximize");
                }
                else
                {
                    Debug.LogWarning("Ball reference is null in Player!");
                }
                Destroy(other.gameObject);
            }
            else
            {
                Debug.LogWarning("PowerUpCoin component not found on collided object!");
            }
        }
    }

    // FixedUpdate is called once per fixed frame-rate frame.
    void FixedUpdate()
    {
        Vector3 movement = new Vector3(movementX, 0f, movementY).normalized;
        rb.linearVelocity = movement * speed;
    }
}