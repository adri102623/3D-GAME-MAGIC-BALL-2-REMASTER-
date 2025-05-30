using UnityEngine;

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
        // Dispara la pelota hacia adelante
        rb.linearVelocity = Vector3.forward * initialSpeed;
        initialScale = transform.localScale;
        god = false;
    }

    // void UpdatePickUpColliders()
    // {
    //     GameObject[] pickUps = GameObject.FindGameObjectsWithTag("PickUp");
    //     foreach (GameObject pickUp in pickUps)
    //     {
    //         Collider collider = pickUp.GetComponent<Collider>();
    //         if (collider != null)
    //         {
    //             collider.isTrigger = god; // Triggers en modo god, no triggers en modo normal
    //         }
    //     }
    // }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            god = !god;
            
            GetComponent<Renderer>().material = god ? powerBallMaterial : defaultMaterial;
            UpdatePickUpColliders(); // Actualizar colliders de PickUps
        }
    }

    void UpdatePickUpColliders(){
    GameObject[] pickUps = GameObject.FindGameObjectsWithTag("PickUp");
    Collider ballCollider = GetComponent<Collider>();

    foreach (GameObject pickUp in pickUps)
    {
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
        // Rebote manteniendo velocidad constante
        rb.linearVelocity = rb.linearVelocity.normalized * initialSpeed;
            if (god && other.CompareTag("PickUp")){
                PickupHealth pickup = other.GetComponent<PickupHealth>();
                if (pickup != null)
                {
                pickup.set_1();
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
        // Esperamos al final del frame para que la f�sica act�e
        yield return new WaitForFixedUpdate();
        pickup.SetActive(false);
    }
}