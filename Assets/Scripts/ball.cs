using UnityEngine;

public class Ball : MonoBehaviour
{
    public float initialSpeed = 15f; // Velocidad constante de la pelota
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Dispara la pelota hacia adelante
        rb.linearVelocity = Vector3.forward * initialSpeed;
    }

    void OnCollisionEnter(Collision collision)
    {
        // Normaliza la velocidad para mantenerla constante
        rb.linearVelocity = rb.linearVelocity.normalized * initialSpeed;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PickUp"))
        {
            other.gameObject.SetActive(false);
        }
    }
}