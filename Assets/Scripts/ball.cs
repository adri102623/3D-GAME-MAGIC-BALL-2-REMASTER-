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
            // Obtener el punto más cercano en el collider del PickUp
            Vector3 closestPoint = other.ClosestPoint(transform.position);
            // Calcular la dirección desde el centro del PickUp al punto de contacto
            Vector3 directionToClosest = closestPoint - other.transform.position;

            // Transformar la dirección al espacio local del PickUp para manejar rotaciones
            Vector3 localDirection = other.transform.InverseTransformDirection(directionToClosest);

            // Determinar la cara del cubo más cercana en el espacio local
            Vector3 normal = Vector3.zero;
            float absX = Mathf.Abs(localDirection.x);
            float absZ = Mathf.Abs(localDirection.z);

            if (absX > absZ)
            {
                normal = (localDirection.x > 0) ? Vector3.right : Vector3.left; // Cara este u oeste
            }
            else
            {
                normal = (localDirection.z > 0) ? Vector3.forward : Vector3.back; // Cara norte o sur
            }

            // Convertir la normal al espacio global
            normal = other.transform.TransformDirection(normal);
            normal.y = 0f; // Restringir a 2D

            if (normal != Vector3.zero)
            {
                rb.linearVelocity = Vector3.Reflect(rb.linearVelocity.normalized, normal) * initialSpeed;
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            }

            other.gameObject.SetActive(false);
        }
    }
}