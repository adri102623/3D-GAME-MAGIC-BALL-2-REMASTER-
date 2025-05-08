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


    // Movement along X and Y axes.
    private float movementX;
    private float movementY;

    // Speed at which the player moves.
    public float speed = 0;

    public TextMeshProUGUI countText;

    
    // Start is called before the first frame update.
    void Start()
    {
        count = 0;
        // Get and store the Rigidbody component attached to the player.
        rb = GetComponent<Rigidbody>();

        rb.freezeRotation = true; // Bloquea todas las rotaciones


        // Aseg�rate de que rb no sea null
        if (rb == null)
        {
            Debug.LogError("Rigidbody no encontrado en el objeto!");
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
    // FixedUpdate is called once per fixed frame-rate frame.
    void FixedUpdate()
    {
        Vector3 movement = new Vector3(movementX, 0f, movementY).normalized;
        rb.linearVelocity = movement * speed;
    }

}