using UnityEngine;

public class Ball : MonoBehaviour
{
    public float initialSpeed = 15f; // Velocidad constante de la pelota
    private Rigidbody rb;
    private Vector3 initialScale; // Escala inicial de la pelota
    private float maxScaleFactor = 2f; // Máximo 2x la escala inicial
    private float minScaleFactor = 0.5f; // Mínimo 0.5x la escala inicial

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Dispara la pelota hacia adelante
        rb.linearVelocity = Vector3.forward * initialSpeed;
        initialScale = transform.localScale;

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

    public void ApplyPowerUp()
    {
        Vector3 newScale = transform.localScale;

        // Aumentar el tamaño al doble
        newScale = transform.localScale * 2f;

        // Limitar el tamaño entre 0.5x y 2x de la escala inicial
        float scaleFactor = newScale.x / initialScale.x; // Comparar con la escala inicial
        scaleFactor = Mathf.Clamp(scaleFactor, minScaleFactor, maxScaleFactor);
        newScale = initialScale * scaleFactor;

        // Aplicar el nuevo tamaño
        transform.localScale = newScale;
        Debug.Log("New scale applied: " + transform.localScale);
    }

    System.Collections.IEnumerator DeactivateAfterPhysics(GameObject pickup)
    {
        // Esperamos al final del frame para que la f�sica act�e
        yield return new WaitForFixedUpdate();
        pickup.SetActive(false);
    }
}