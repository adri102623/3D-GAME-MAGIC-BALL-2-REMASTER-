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


        // Asegúrate de que rb no sea null
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

    // FixedUpdate is called once per fixed frame-rate frame.
    private void FixedUpdate()
    {
        // Crear un vector de movimiento usando las entradas de movimiento.
        Vector3 movement = new Vector3(movementX, 0.0f, movementY);

        // Mover la nave solo horizontalmente en el plano X-Z
        Vector3 newPosition = rb.position + movement * speed * Time.deltaTime;

        // Solo ajustar la posición en X y Z para que no se mueva en Y (ni caiga)
        rb.MovePosition(new Vector3(newPosition.x, rb.position.y, newPosition.z));
    }
}