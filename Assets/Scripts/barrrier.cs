using UnityEngine;

public class BarrierCollision : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            Destroy(collision.gameObject); // Destruye la pelota
            Debug.Log("Pelota destruida por la barrera");
            // Añade aquí más lógica si la necesitas (sonido, restar vidas, etc.)
        }
    }
}