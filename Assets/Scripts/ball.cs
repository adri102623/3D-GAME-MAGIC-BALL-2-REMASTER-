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
        // Rebote manteniendo velocidad constante
        rb.linearVelocity = rb.linearVelocity.normalized * initialSpeed;

        if (collision.gameObject.CompareTag("PickUp"))
        {
            // Empezamos una rutina para desactivar el objeto despu�s de un frame
            //StartCoroutine(DeactivateAfterPhysics(collision.gameObject));
        }
    }

    System.Collections.IEnumerator DeactivateAfterPhysics(GameObject pickup)
    {
        // Esperamos al final del frame para que la f�sica act�e
        yield return new WaitForFixedUpdate();
        pickup.SetActive(false);
    }
}