using UnityEngine;
using System.Collections;

public class Ball : MonoBehaviour
{
    public float initialSpeed = 15f; // Velocidad constante de la pelota
    private Rigidbody rb;
    private Vector3 initialScale; // Escala inicial de la pelota
    private float maxScaleFactor = 2f; // Máximo 2x la escala inicial
    private float minScaleFactor = 0.5f; // Mínimo 0.5x la escala inicial
    public Material powerBallMaterial;
    public Material defaultMaterial;
    private bool god;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Dispara la pelota hacia adelante
            rb.linearVelocity = Vector3.forward * initialSpeed;
        }
        initialScale = transform.localScale;
        god = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            god = !god;
            
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = god ? powerBallMaterial : defaultMaterial;
            }
            UpdatePickUpColliders(); // Actualizar colliders de PickUps
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
            if (pickupCollider != null)
            {
                // Ignore or re-enable collision with the ball
                Physics.IgnoreCollision(ballCollider, pickupCollider, god);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other == null) return;

        if (other.gameObject.CompareTag("PickUp"))
        {
            PickupHealth pickup = other.GetComponent<PickupHealth>();
            if (pickup != null && !god)
            {
                pickup.TocarPelota();
            }
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
        // Esperamos al final del frame para que la física actúe
        yield return new WaitForFixedUpdate();
        if (pickup != null)
        {
            pickup.SetActive(false);
        }
    }
}